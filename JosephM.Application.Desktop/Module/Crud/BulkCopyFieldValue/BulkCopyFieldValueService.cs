using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System;

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
            foreach (var record in request.GetRecordsToUpdate())
            {
                controller.UpdateProgress(countUpdated, countToUpdate, estimator.GetProgressString(countUpdated, taskName: "Executing Updates"));
                try
                {
                    var reloadRecord = RecordService.Get(record.Type, record.Id, new[] { request.SourceField.Key, request.TargetField.Key });
                    if ((request.CopyIfNull || reloadRecord.GetField(request.SourceField.Key) != null)
                        && (request.OverwriteIfPopulated || reloadRecord.GetField(request.TargetField.Key) == null))
                    {
                        reloadRecord.SetField(request.TargetField.Key, reloadRecord.GetField(request.SourceField.Key), RecordService);
                        RecordService.Update(reloadRecord, new[] { request.TargetField.Key });
                    }
                }
                catch(Exception ex)
                {
                    response.AddResponseItem(new BulkCopyFieldValueResponseItem(record.Id, record.GetStringField(RecordService.GetPrimaryField(record.Type)), ex));
                }
                countUpdated++;
            }
            controller.UpdateProgress(1, 1, "Copies Completed");
            response.Message = "Copies Completed";
        }
    }
}