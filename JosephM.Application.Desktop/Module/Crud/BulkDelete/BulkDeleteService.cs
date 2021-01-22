using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System;
using System.Linq;

namespace JosephM.Application.Desktop.Module.Crud.BulkDelete
{
    public class BulkDeleteService :
        ServiceBase<BulkDeleteRequest, BulkDeleteResponse, BulkDeleteResponseItem>
    {
        public IRecordService RecordService { get; set; }
        public BulkDeleteService(IRecordService recordService)
        {
            RecordService = recordService;
        }

        public override void ExecuteExtention(BulkDeleteRequest request, BulkDeleteResponse response,
            ServiceRequestController controller)
        {
            var countToUpdate = request.RecordCount;
            var countUpdated = 0;
            controller.UpdateProgress(0, countToUpdate, "Executing Deletions");
            var estimator = new TaskEstimator(countToUpdate);

            var recordsRemaining = request.GetRecordsToDelete().ToList();
            while (recordsRemaining.Any())
            {
                controller.UpdateProgress(countUpdated, countToUpdate, estimator.GetProgressString(countUpdated, taskName: "Executing Deletions"));

                var thisSetOfRecords = recordsRemaining
                    .Take(request.ExecuteMultipleSetSize ?? 50)
                    .ToList();

                recordsRemaining.RemoveRange(0, thisSetOfRecords.Count);

                var thisSetOfRecordsNew = thisSetOfRecords
                    .Select(r =>
                    {
                        var newRecord = RecordService.NewRecord(request.RecordType.Key);
                        newRecord.Id = r.Id;
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
                        RecordService.Delete(record);
                    }
                    catch (Exception ex)
                    {
                        var primaryField = RecordService.GetPrimaryField(record.Type);
                        response.AddResponseItem(new BulkDeleteResponseItem(record.Id, primaryField == null ? record.Id  : record.GetStringField(primaryField), ex));
                        errorsThisIteration++;
                    }
                }
                else
                {
                    var multipleResponse = RecordService.DeleteMultiple(thisSetOfRecordsNew);
                    foreach (var item in multipleResponse)
                    {
                        var originalRecord = thisSetOfRecords[item.Key];
                        response.AddResponseItem(new BulkDeleteResponseItem(originalRecord.Id, originalRecord.GetStringField(RecordService.GetPrimaryField(originalRecord.Type)), item.Value));
                    }
                    errorsThisIteration += multipleResponse.Count;
                }

                countUpdated += thisSetOfRecords.Count();
                response.NumberOfErrors += errorsThisIteration;
                response.TotalRecordsProcessed = countUpdated;
            }

            controller.UpdateProgress(1, 1, "Deletions Completed");
            response.Message = "Deletions Completed";
        }
    }
}