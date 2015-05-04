#region

using JosephM.CustomisationImporter.Service;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Record.Application.Dialog;

#endregion

namespace JosephM.CustomisationImporter.Prism
{
    public class XrmCustomisationImportDialog :
        ServiceRequestDialog
            <XrmCustomisationImportService, CustomisationImportRequest, CustomisationImportResponse,
                CustomisationImportResponseItem>
    {
        public XrmCustomisationImportDialog(XrmCustomisationImportService service, IDialogController dialogController)
            : base(service, dialogController)
        {
        }
    }
}