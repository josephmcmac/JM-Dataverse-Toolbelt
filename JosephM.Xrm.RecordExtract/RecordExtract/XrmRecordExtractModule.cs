using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    [DependantModule(typeof(XrmRecordExtractSettingsModule))]
    public class XrmRecordExtractModule :
        ServiceRequestModule
            <XrmRecordExtractDialog, XrmRecordExtractService, RecordExtractRequest, RecordExtractResponse, RecordExtractResponseItem>
    {
        protected override string MainOperationName
        {
            get { return "CRM Record Report"; }
        }

        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddHelp("CRM Record Report", "Record Extract Help.docx");
        }
    }
}