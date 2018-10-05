using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Log;
using JosephM.Record.Xrm.XrmRecord;
using System.Linq;

namespace JosephM.CustomisationImporter.Service
{
    public class CustomisationImportValidationDialog : DialogViewModel
    {
        public CustomisationImportValidationDialog(CustomisationImportDialog parentDialog, CustomisationImportRequest importRequest)
            : base(parentDialog)
        {
            CustomisationImportService = parentDialog.Service;
            XrmRecordService = parentDialog.Service.RecordService;
            Request = importRequest;
        }

        public XrmRecordService XrmRecordService { get; private set; }

        public CustomisationImportService CustomisationImportService { get; private set; }
        public CustomisationImportRequest Request { get; }

        protected override void CompleteDialogExtention()
        {
        }

        protected override void LoadDialogExtention()
        {
            //okay lets load the spreadsheet
            //and if there are any error display them
            //else continue
            var readExcelResponse = CustomisationImportService.ReadExcel(Request, new LogController());

            if (readExcelResponse.ResponseItems.Any())
            {
                AddObjectToUi(readExcelResponse,
                    backAction: () =>
                    {
                        RemoveObjectFromUi(readExcelResponse);
                        MoveBackToPrevious();
                    });

            }
            else
            {
                StartNextAction();
            }
        }
    }
}
