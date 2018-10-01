using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.XrmTestModule.DebugModule
{
    public class DebugDialogService :
        ServiceBase<DebugDialogRequest, DebugDialogResponse, DebugDialogResponseItem>
    {
        public XrmRecordService Service { get; set; }

        public DebugDialogService(XrmRecordService service)
        {
            Service = service;
        }

        public override void ExecuteExtention(DebugDialogRequest request, DebugDialogResponse response,
            ServiceRequestController controller)
        {
            
        }
    }
}