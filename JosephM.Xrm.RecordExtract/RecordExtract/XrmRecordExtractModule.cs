using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    public class XrmRecordExtractModule :
        ServiceRequestModule
            <XrmRecordExtractDialog, XrmRecordExtractService, RecordExtractRequest, RecordExtractResponse, RecordExtractResponseItem>
    {
        protected override string MainOperationName
        {
            get { return "Record Report"; }
        }

        public override void InitialiseModule()
        {
            base.InitialiseModule();
            //AddHelp("Record Report", "Record Extract Help.docx");
        }
    }
}