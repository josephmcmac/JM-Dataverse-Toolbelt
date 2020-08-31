using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Log;
using JosephM.Deployment.SpreadsheetImport;
using JosephM.Record.Xrm.XrmRecord;
using System.Linq;

namespace JosephM.Deployment.ImportCsvs
{
    public class ImportCsvsValidationDialog : DialogViewModel
    {
        public ImportCsvsValidationDialog(ImportCsvsDialog parentDialog, ImportCsvsRequest importRequest)
            : base(parentDialog)
        {
            ImportCsvsService = parentDialog.Service;
            XrmRecordService = parentDialog.Service.XrmRecordService;
            Request = importRequest;
        }

        public XrmRecordService XrmRecordService { get; private set; }

        public ImportCsvsService ImportCsvsService { get; private set; }
        public ImportCsvsRequest Request { get; }

        protected override void CompleteDialogExtention()
        {
        }

        protected override void LoadDialogExtention()
        {
            //okay lets load the spreadsheet
            //and if there are any error display them
            //else continue
            var logController = new LogController(LoadingViewModel);
            var dictionary = ImportCsvsService.LoadMappingDictionary(Request);

            var importService = new SpreadsheetImportService(XrmRecordService);
            var parseResponse = importService.ParseIntoEntities(dictionary, logController, useAmericanDates: Request.DateFormat == DateFormat.American);
            if (parseResponse.ResponseItems.Any())
            {
                AddObjectToUi(parseResponse
                    , cancelAction: Controller.Close
                    , nextAction: () =>
                    {
                        RemoveObjectFromUi(parseResponse);
                        StartNextAction();
                    }
                    , nextActionLabel: "Import"
                    , backAction: () =>
                    {
                        RemoveObjectFromUi(parseResponse);
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
