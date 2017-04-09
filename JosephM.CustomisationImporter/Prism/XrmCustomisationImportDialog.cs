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
            : base(service, dialogController, service.RecordService)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            if (Response.ExcelReadErrors)
                CompletionMessage = "There were errors loading the metadata in the Excel spreadsheet. You will need to review and fix these errors then rerun the import";
        }
    }
}