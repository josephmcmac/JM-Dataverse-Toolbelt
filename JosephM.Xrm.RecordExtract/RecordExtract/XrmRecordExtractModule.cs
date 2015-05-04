using JosephM.Prism.Infrastructure.Attributes;
using JosephM.Prism.Infrastructure.Constants;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Prism;

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
            ApplicationOptions.AddHelp("CRM Record Report", "Record Extract Help.htm");
        }
    }
}