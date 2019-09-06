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
                    new[] { request.FieldToReplaceIn.Key })
                    .ToArray();

                var recordsToUpdate = new List<IRecord>();
                foreach (var record in thisSetOfRecordsNew)
                {
                    try
                    {
                        var previousValue = record.GetStringField(request.FieldToReplaceIn.Key);
                        var newValue = previousValue == null ? null : previousValue.Replace(request.OldValue, request.NewValue);
                        if (previousValue != newValue)
                        {
                            record.SetField(request.FieldToReplaceIn.Key, newValue, RecordService);
                            recordsToUpdate.Add(record);
                        }
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(new BulkReplaceResponseItem(record.Id, record.GetStringField(RecordService.GetPrimaryField(record.Type)), ex));
                    }
                }

                var errorsThisIteration = 0;

                //old versions dont have execute multiple so if 1 then do each request
                if (recordsToUpdate.Count() == 1)
                {
                    var record = recordsToUpdate.First();
                    try
                    {
                        RecordService.Update(record, new[] { request.FieldToReplaceIn.Key });
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(new BulkReplaceResponseItem(record.Id, record.GetStringField(RecordService.GetPrimaryField(record.Type)), ex));
                    }
                }
                else
                {
                    var multipleResponse = RecordService.UpdateMultiple(recordsToUpdate, new[] { request.FieldToReplaceIn.Key });
                    foreach (var item in multipleResponse)
                    {
                        var originalRecord = recordsToUpdate[item.Key];
                        response.AddResponseItem(new BulkReplaceResponseItem(originalRecord.Id, originalRecord.GetStringField(RecordService.GetPrimaryField(originalRecord.Type)), item.Value));
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