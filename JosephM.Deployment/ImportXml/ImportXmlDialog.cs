#region

using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Record.Xrm.XrmRecord;

#endregion

namespace JosephM.Deployment.ImportXml
{
    public class ImportXmlDialog :
        ServiceRequestDialog
            <ImportXmlService, ImportXmlRequest,
                ImportXmlResponse, DataImportResponseItem>
    {
        public ImportXmlDialog(ImportXmlService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
        }
    }
}