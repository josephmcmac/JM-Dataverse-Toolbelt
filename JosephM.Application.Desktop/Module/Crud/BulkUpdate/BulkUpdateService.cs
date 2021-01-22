using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System;
using System.Linq;

namespace JosephM.Application.Desktop.Module.Crud.BulkUpdate
{
    public class BulkUpdateService :
        ServiceBase<BulkUpdateRequest, BulkUpdateResponse, BulkUpdateResponseItem>
    {
        public IRecordService RecordService { get; set; }
        public BulkUpdateService(IRecordService recordService)
        {
            RecordService = recordService;
        }

        public override void ExecuteExtention(BulkUpdateRequest request, BulkUpdateResponse response,
            ServiceRequestController controller)
        {
            var countToUpdate = request.RecordCount;
            var countUpdated = 0;
            controller.UpdateProgress(0, countToUpdate, "Executing Updates");
            var estimator = new TaskEstimator(countToUpdate);
            var recordsRemaining = request.GetRecordsToUpdate().ToList();
            while(recordsRemaining.Any())
            {
                controller.UpdateProgress(countUpdated, countToUpdate, estimator.GetProgressString(countUpdated, taskName: "Executing Updates"));

                var thisSetOfRecords = recordsRemaining
                    .Take(request.ExecuteMultipleSetSize ?? 50)
                    .ToList();

                recordsRemaining.RemoveRange(0, thisSetOfRecords.Count);

                var thisSetOfRecordsNew = thisSetOfRecords
                    .Select(r =>
                    {
                        var newRecord = RecordService.NewRecord(request.RecordType.Key);
                        newRecord.Id = r.Id;
                        if (request.ClearValue)
                            newRecord.SetField(request.FieldToSet.Key, null, RecordService);
                        else
                            newRecord.SetField(request.FieldToSet.Key, request.ValueToSet, RecordService);
                        if (request.AddUpdateField2)
                        {
                            if (request.ClearValue2)
                                newRecord.SetField(request.FieldToSet2.Key, null, RecordService);
                            else
                                newRecord.SetField(request.FieldToSet2.Key, request.ValueToSet2, RecordService);
                        }
                        if (request.AddUpdateField3)
                        {
                            if (request.ClearValue3)
                                newRecord.SetField(request.FieldToSet3.Key, null, RecordService);
                            else
                                newRecord.SetField(request.FieldToSet3.Key, request.ValueToSet3, RecordService);
                        }
                        if (request.AddUpdateField4)
                        {
                            if (request.ClearValue4)
                                newRecord.SetField(request.FieldToSet4.Key, null, RecordService);
                            else
                                newRecord.SetField(request.FieldToSet4.Key, request.ValueToSet4, RecordService);
                        }
                        if (request.AddUpdateField5)
                        {
                            if (request.ClearValue5)
                                newRecord.SetField(request.FieldToSet5.Key, null, RecordService);
                            else
                                newRecord.SetField(request.FieldToSet5.Key, request.ValueToSet5, RecordService);
                        }
                        return newRecord;
                    })
                    .ToArray();

                var errorsThisIteration = 0;

                //old versions dont have execute multiple so if 1 then do each request
                if (thisSetOfRecordsNew.Count() == 1)
                {
                    var record = thisSetOfRecordsNew.First();
                    try
                    {
                        RecordService.Update(record);
                    }
                    catch (Exception ex)
                    {
                        var primaryField = RecordService.GetPrimaryField(record.Type);
                        response.AddResponseItem(new BulkUpdateResponseItem(record.Id, primaryField == null ? record.Id : record.GetStringField(primaryField), ex));
                        errorsThisIteration++;
                    }
                }
                else
                {
                    var multipleResponse = RecordService.UpdateMultiple(thisSetOfRecordsNew);
                    foreach (var item in multipleResponse)
                    {
                        var originalRecord = thisSetOfRecords[item.Key];
                        response.AddResponseItem(new BulkUpdateResponseItem(originalRecord.Id, originalRecord.GetStringField(RecordService.GetPrimaryField(originalRecord.Type)), item.Value));
                    }
                    errorsThisIteration += multipleResponse.Count;
                }

                countUpdated += thisSetOfRecords.Count();
                response.NumberOfErrors += errorsThisIteration;
                response.TotalRecordsProcessed = countUpdated;
            }
            controller.UpdateProgress(1, 1, "Updates Completed");
            response.Message = "Updates Completed";
        }
    }
}