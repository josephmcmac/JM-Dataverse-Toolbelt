using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System;

namespace JosephM.Prism.Infrastructure.Module.Crud.BulkDelete
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
            LogController controller)
        {
            var countToUpdate = request.RecordCount;
            var countUpdated = 0;
            controller.UpdateProgress(0, countToUpdate, "Executing Deletions");
            var estimator = new TaskEstimator(countToUpdate);
            foreach (var record in request.GetRecordsToUpdate())
            {
                try
                {
                    var newRecord = RecordService.NewRecord(request.RecordType.Key);
                    newRecord.Id = record.Id;
                    RecordService.Delete(newRecord);
                }
                catch(Exception ex)
                {
                    response.AddResponseItem(new BulkDeleteResponseItem(record.Id, record.GetStringField(RecordService.GetPrimaryField(record.Type)), ex));
                }
                countUpdated++;
                controller.UpdateProgress(countUpdated, countToUpdate, estimator.GetProgressString(countUpdated, taskName: "Executing Deletions"));
            }
        }
    }
}