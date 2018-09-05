using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.CustomisationImporter.Service;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.CustomisationImporter
{
    [RequiresConnection]
    public class CustomisationImportDialog :
        ServiceRequestDialog
            <XrmCustomisationImportService, CustomisationImportRequest, CustomisationImportResponse,
                CustomisationImportResponseItem>
    {
        public CustomisationImportDialog(XrmCustomisationImportService service, IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
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