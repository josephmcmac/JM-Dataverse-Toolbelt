using JosephM.Application.Modules;
using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Core.Attributes;
using JosephM.Prism.XrmModule.XrmConnection;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    [DependantModule(typeof(XrmConnectionModule))]
    [MyDescription("Generate A Document Detailing The Field Values And Related Records For A Specific Record In The CRM Instance")]
    public class XrmRecordExtractModule :
        ServiceRequestModule
            <XrmRecordExtractDialog, XrmRecordExtractService, RecordExtractRequest, RecordExtractResponse, RecordExtractResponseItem>
    {
        public override string MainOperationName
        {
            get { return "Record Report"; }
        }

        public override string MenuGroup => "Reports";
    }
}