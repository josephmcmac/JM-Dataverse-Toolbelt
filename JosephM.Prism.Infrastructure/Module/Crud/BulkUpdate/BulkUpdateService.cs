using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System;

namespace JosephM.Prism.Infrastructure.Module.Crud.BulkUpdate
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
            LogController controller)
        {
            var countToUpdate = request.RecordCount;
            var countUpdated = 0;
            foreach (var record in request.GetRecordsToUpdate())
            {
                try
                {
                    controller.UpdateProgress(countUpdated++, countToUpdate, "Executing Updates");
                    var newRecord = RecordService.NewRecord(request.RecordType.Key);
                    newRecord.Id = record.Id;
                    if (request.ClearValue)
                        newRecord.SetField(request.FieldToSet.Key, null, RecordService);
                    else
                        newRecord.SetField(request.FieldToSet.Key, request.ValueToSet, RecordService);
                    RecordService.Update(newRecord, new[] { request.FieldToSet.Key });
                }
                catch(Exception ex)
                {
                    response.AddResponseItem(new BulkUpdateResponseItem(record.Id, record.GetStringField(RecordService.GetPrimaryField(record.Type)), ex));
                }
            }
        }
    }
}