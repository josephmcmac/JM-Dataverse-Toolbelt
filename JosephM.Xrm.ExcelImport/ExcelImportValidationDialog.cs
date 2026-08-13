using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Log;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.DataImportExport.MappedImport;
using System.Linq;

namespace JosephM.Xrm.ExcelImport
{
    public class ExcelImportValidationDialog : DialogViewModel
    {
        public ExcelImportValidationDialog(ExcelImportDialog parentDialog, ExcelImportRequest importRequest)
            : base(parentDialog)
        {
            ImportExcelService = parentDialog.Service;
            XrmRecordService = parentDialog.Service.XrmRecordService;
            Request = importRequest;
        }

        public XrmRecordService XrmRecordService { get; private set; }

        public ExcelImportService ImportExcelService { get; private set; }
        public ExcelImportRequest Request { get; }

        protected override void CompleteDialogExtention()
        {
        }

        protected override void LoadDialogExtention()
        {
            var logController = new LogController(LoadingViewModel);
            var dictionary = ImportExcelService.LoadMappingDictionaryWithSourceData(Request, logController);
            var importService = new MappedImportService(XrmRecordService);
            var parseResponse = importService.TransformMappingDictionaryDataForTarget(dictionary, logController, ignoreNullValues: Request.IgnoreEmptyCells);
            Request.LoadMappingDictionary(dictionary);
            Request.LoadValidationResponse(parseResponse);
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
                        Request.UnloadMappingDictionary();
                        Request.UnloadValidationResponse();
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
