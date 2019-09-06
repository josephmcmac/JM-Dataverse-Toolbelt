using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.Desktop.Module.Crud.BulkCopyFieldValue
{
    public class BulkCopyFieldValueService :
        ServiceBase<BulkCopyFieldValueRequest, BulkCopyFieldValueResponse, BulkCopyFieldValueResponseItem>
    {
        public IRecordService RecordService { get; set; }
        public BulkCopyFieldValueService(IRecordService recordService)
        {
            RecordService = recordService;
        }

        public override void ExecuteExtention(BulkCopyFieldValueRequest request, BulkCopyFieldValueResponse response,
            ServiceRequestController controller)
        {
            var countToUpdate = request.RecordCount;
            var countUpdated = 0;
            controller.UpdateProgress(0, countToUpdate, "Executing Updates");
            var estimator = new TaskEstimator(countToUpdate);

            var recordsRemaining = request.GetRecordsToUpdate().ToList();
            while (recordsRemaining.Any())
            {
                controller.UpdateProgress(countUpdated, countToUpdate, estimator.GetProgressString(countUpdated, taskName: "Executing Updates"));

                var thisSetOfRecords = recordsRemaining
                    .Take(request.ExecuteMultipleSetSize ?? 50)
                    .ToList();

                recordsRemaining.RemoveRange(0, thisSetOfRecords.Count);

                var thisSetOfRecordsNew = RecordService.GetMultiple(thisSetOfRecords.First().Type,
                    thisSetOfRecords.Select(r => r.Id),
                    new[] { request.SourceField.Key, request.TargetField.Key })
                    .ToArray();

                var recordsToUpdate = new List<IRecord>();
                foreach(var record in thisSetOfRecordsNew)
                {
                    try
                    {
                        var sourceValue = record.GetField(request.SourceField.Key);
                        var targetValue = record.GetField(request.TargetField.Key);
                        var parseSourceintoTarget = RecordService.ParseField(request.SourceField.Key, record.Type, sourceValue);
                        if (!record.FieldsEqual(request.TargetField.Key, parseSourceintoTarget))
                        {
                            if ((request.CopyIfNull || sourceValue != null)
                                && (request.OverwriteIfPopulated || targetValue == null))
                            {
                                record.SetField(request.TargetField.Key, parseSourceintoTarget, RecordService);
                                recordsToUpdate.Add(record);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        response.AddResponseItem(new BulkCopyFieldValueResponseItem(record.Id, record.GetStringField(RecordService.GetPrimaryField(record.Type)), ex));
                    }
                }

                var errorsThisIteration = 0;

                //old versions dont have execute multiple so if 1 then do each request
                if (recordsToUpdate.Count() == 1)
                {
                    var record = recordsToUpdate.First();
                    try
                    {
                       RecordService.Update(record, new[] { request.TargetField.Key });
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(new BulkCopyFieldValueResponseItem(record.Id, record.GetStringField(RecordService.GetPrimaryField(record.Type)), ex));
                    }
                }
                else
                {
                    var multipleResponse = RecordService.UpdateMultiple(recordsToUpdate, new[] { request.TargetField.Key });
                    foreach (var item in multipleResponse)
                    {
                        var originalRecord = recordsToUpdate[item.Key];
                        response.AddResponseItem(new BulkCopyFieldValueResponseItem(originalRecord.Id, originalRecord.GetStringField(RecordService.GetPrimaryField(originalRecord.Type)), item.Value));
                    }
                    errorsThisIteration += multipleResponse.Count;
                }

                countUpdated += thisSetOfRecords.Count();
                response.TotalRecordsProcessed = countUpdated;
                response.TotalRecordsUpdated += recordsToUpdate.Count();
                response.NumberOfErrors += errorsThisIteration;
            }

            controller.UpdateProgress(1, 1, "Copies Completed");
            response.Message = "Copies Completed";
        }
    }
}