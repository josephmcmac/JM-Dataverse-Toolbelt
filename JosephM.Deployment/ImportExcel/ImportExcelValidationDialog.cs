using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Log;
using JosephM.Deployment.SpreadsheetImport;
using JosephM.Record.Xrm.XrmRecord;
using System.Linq;

namespace JosephM.Deployment.ImportExcel
{
    public class ImportExcelValidationDialog : DialogViewModel
    {
        public ImportExcelValidationDialog(ImportExcelDialog parentDialog, ImportExcelRequest importRequest)
            : base(parentDialog)
        {
            ImportExcelService = parentDialog.Service;
            XrmRecordService = parentDialog.Service.XrmRecordService;
            Request = importRequest;
        }

        public XrmRecordService XrmRecordService { get; private set; }

        public ImportExcelService ImportExcelService { get; private set; }
        public ImportExcelRequest Request { get; }

        protected override void CompleteDialogExtention()
        {
        }

        protected override void LoadDialogExtention()
        {
            //okay lets load the spreadsheet
            //and if there are any error display them
            //else continue
            var logController = new LogController(LoadingViewModel);
            var dictionary = ImportExcelService.LoadMappingDictionary(Request, logController);

            var importService = new SourceImportService(XrmRecordService);
            var parseResponse = importService.ParseIntoEntities(dictionary, logController, ignoreNullValues: Request.IgnoreEmptyCells);
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
