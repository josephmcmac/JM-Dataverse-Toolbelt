#region

using JosephM.Application.ViewModel.Dialog;
using JosephM.CustomisationImporter.Service;
using JosephM.Prism.Infrastructure.Dialog;

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