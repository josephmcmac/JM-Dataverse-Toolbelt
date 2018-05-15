using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System;

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
            LogController controller)
        {
            var countToUpdate = request.RecordCount;
            var countUpdated = 0;
            controller.UpdateProgress(0, countToUpdate, "Executing Replacements");
            var estimator = new TaskEstimator(countToUpdate);
            foreach (var record in request.GetRecordsToUpdate())
            {
                try
                {
                    var newRecord = RecordService.NewRecord(request.RecordType.Key);
                    newRecord.Id = record.Id;
                    var previousValue = record.GetStringField(request.FieldToReplaceIn.Key);
                    var newValue = previousValue == null ? null : previousValue.Replace(request.OldValue, request.NewValue);
                    if(previousValue != newValue)
                    {
                        newRecord.SetField(request.FieldToReplaceIn.Key, newValue, RecordService);
                        RecordService.Update(newRecord, new[] { request.FieldToReplaceIn.Key });
                    }
                }
                catch(Exception ex)
                {
                    response.AddResponseItem(new BulkReplaceResponseItem(record.Id, record.GetStringField(RecordService.GetPrimaryField(record.Type)), ex));
                }
                countUpdated++;
                controller.UpdateProgress(countUpdated, countToUpdate, estimator.GetProgressString(countUpdated, taskName: "Executing Replacements"));
            }
        }
    }
}