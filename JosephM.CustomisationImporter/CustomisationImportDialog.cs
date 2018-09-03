using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.CustomisationImporter.Service;

namespace JosephM.CustomisationImporter
{
    [RequiresConnection]
    public class CustomisationImportDialog :
        ServiceRequestDialog
            <XrmCustomisationImportService, CustomisationImportRequest, CustomisationImportResponse,
                CustomisationImportResponseItem>
    {
        public CustomisationImportDialog(XrmCustomisationImportService service, IDialogController dialogController)
            : base(service, dialogController)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            if (Response.ExcelReadErrors)
                CompletionMessage = "There were errors loading the metadata in the Excel spreadsheet. You will need to review and fix these errors then rerun the import";
            else
                CompletionMessage = "TheCustomisations Have Been Imported And Published";
        }
    }
}