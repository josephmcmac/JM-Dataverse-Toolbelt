using JosephM.Core.Log;
using JosephM.Core.Service;
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
            //todo this
            throw new NotImplementedException();
        }
    }
}