using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.Desktop.Module.Crud.BulkReplace
{
    public class BulkReplaceService :
        ServiceBase<BulkReplaceRequest, BulkReplaceResponse, BulkReplaceResponseItem>
    {
        public IRecordService RecordService { get; set; }
        public BulkReplaceService(IRecordService recordService)
        {
            RecordService = recordService;
        }

        public override void ExecuteExtention(BulkReplaceRequest request, BulkReplaceResponse response,
            ServiceRequestController controller)
        {
            var countToUpdate = request.RecordCount;
            var countUpdated = 0;
            controller.UpdateProgress(0, countToUpdate, "Executing Replacements");
            var estimator = new TaskEstimator(countToUpdate);

            var recordsRemaining = request.GetRecordsToUpdate().ToList();
            while (recordsRemaining.Any())
            {
                controller.UpdateProgress(countUpdated, countToUpdate, estimator.GetProgressString(countUpdated, taskName: "Executing Replacements"));

                var thisSetOfRecords = recordsRemaining
                    .Take(request.ExecuteMultipleSetSize ?? 50)
                    .ToList();

                recordsRemaining.RemoveRange(0, thisSetOfRecords.Count);

                var thisSetOfRecordsNew = RecordService.GetMultiple(thisSetOfRecords.First().Type,
                    thisSetOfRecords.Select(r => r.Id),
                    request.FieldsToReplace.Select(f => f.RecordField.Key).ToArray())
                    .ToArray();

                var errorsThisIteration = 0;

                var recordsToUpdate = new List<IRecord>();
                foreach (var record in thisSetOfRecordsNew)
                {
                    try
                    {
                        var newRecord = RecordService.NewRecord(record.Type);
                        newRecord.Id = record.Id;
                        var isUpdate = false;
                        foreach (var field in request.FieldsToReplace)
                        {
                            var previousValue = record.GetStringField(field.RecordField.Key);
                            var newValue = previousValue;
                            foreach (var replacement in request.ReplacementTexts)
                            {
                                newValue = newValue == null ? null : newValue.Replace(replacement.OldText, replacement.ReplaceWithEmptyString ? string.Empty : replacement.NewText);
                            }
                            if (previousValue != newValue)
                            {
                                newRecord.SetField(field.RecordField.Key, newValue, RecordService);
                                isUpdate = true;
                            }
                        }
                        if (isUpdate)
                        {
                            recordsToUpdate.Add(newRecord);
                        }
                    }
                    catch (Exception ex)
                    {
                        errorsThisIteration++;
                        response.AddResponseItem(new BulkReplaceResponseItem(record.Id, record.GetStringField(RecordService.GetPrimaryField(record.Type)), ex));
                    }
                }

                //old versions dont have execute multiple so if 1 then do each request
                if (recordsToUpdate.Count() == 1)
                {
                    var record = recordsToUpdate.First();
                    try
                    {
                        RecordService.Update(record);
                    }
                    catch (Exception ex)
                    {
                        errorsThisIteration++;
                        response.AddResponseItem(new BulkReplaceResponseItem(record.Id, null, ex));
                    }
                }
                else
                {
                    var multipleResponse = RecordService.UpdateMultiple(recordsToUpdate);
                    foreach (var item in multipleResponse)
                    {
                        var originalRecord = recordsToUpdate[item.Key];
                        response.AddResponseItem(new BulkReplaceResponseItem(originalRecord.Id, null, item.Value));
                    }
                    errorsThisIteration += multipleResponse.Count;
                }

                countUpdated += thisSetOfRecords.Count();
                response.TotalRecordsProcessed = countUpdated;
                response.TotalRecordsUpdated += recordsToUpdate.Count();
                response.NumberOfErrors += errorsThisIteration;
            }

            controller.UpdateProgress(1, 1, "Replaces Completed");
            response.Message = "Replaces Completed";
        }
    }
}