using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Log;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.DataImportExport.MappedImport;
using System.Linq;

namespace JosephM.Xrm.SqlImport
{
    public class SqlImportValidationDialog : DialogViewModel
    {
        public SqlImportValidationDialog(SqlImportDialog parentDialog, SqlImportRequest importRequest)
            : base(parentDialog)
        {
            ImportSqlService = parentDialog.Service;
            XrmRecordService = parentDialog.Service.XrmRecordService;
            Request = importRequest;
        }

        public XrmRecordService XrmRecordService { get; private set; }

        public SqlImportService ImportSqlService { get; private set; }
        public SqlImportRequest Request { get; }

        protected override void CompleteDialogExtention()
        {
        }

        protected override void LoadDialogExtention()
        {
            var logController = new LogController(LoadingViewModel);
            var dictionary = ImportSqlService.LoadMappingDictionaryWithSourceData(Request);
            var importService = new MappedImportService(XrmRecordService);
            var parseResponse = importService.TransformMappingDictionaryDataForTarget(dictionary, logController);
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
