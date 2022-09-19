using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Log;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.DataImportExport.MappedImport;
using System.Linq;

namespace JosephM.Xrm.CsvImport
{
    public class CsvImportValidationDialog : DialogViewModel
    {
        public CsvImportValidationDialog(CsvImportDialog parentDialog, CsvImportRequest importRequest)
            : base(parentDialog)
        {
            ImportCsvsService = parentDialog.Service;
            XrmRecordService = parentDialog.Service.XrmRecordService;
            Request = importRequest;
        }

        public XrmRecordService XrmRecordService { get; private set; }

        public CsvImportService ImportCsvsService { get; private set; }
        public CsvImportRequest Request { get; }

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

            var importService = new MappedImportService(XrmRecordService);
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
