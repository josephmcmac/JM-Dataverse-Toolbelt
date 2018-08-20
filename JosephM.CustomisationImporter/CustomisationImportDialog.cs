using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.CustomisationImporter.Service;
using JosephM.XrmModule.Extentions;

namespace JosephM.CustomisationImporter
{
    public class CustomisationImportDialog :
        ServiceRequestDialog
            <XrmCustomisationImportService, CustomisationImportRequest, CustomisationImportResponse,
                CustomisationImportResponseItem>
    {
        public CustomisationImportDialog(XrmCustomisationImportService service, IDialogController dialogController)
            : this(service, dialogController, false)
        {
        }

        protected CustomisationImportDialog(XrmCustomisationImportService service, IDialogController dialogController, bool dontAddRedirect)
            : base(service, dialogController, service.RecordService)
        {
            if (!dontAddRedirect)
                this.AddRedirectToConnectionEntryWhenNotConnected(service.RecordService);
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            if (Response.ExcelReadErrors)
                CompletionMessage = "There were errors loading the metadata in the Excel spreadsheet. You will need to review and fix these errors then rerun the import";
        }
    }
}