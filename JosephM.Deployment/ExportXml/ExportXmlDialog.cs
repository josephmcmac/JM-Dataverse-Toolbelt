#region

using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Record.Xrm.XrmRecord;

#endregion

namespace JosephM.Deployment.ExportXml
{
    public class ExportXmlDialog :
        ServiceRequestDialog
            <ExportXmlService, ExportXmlRequest,
                ExportXmlResponse, ExportXmlResponseItem>
    {
        public ExportXmlDialog(ExportXmlService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
        }
    }
}