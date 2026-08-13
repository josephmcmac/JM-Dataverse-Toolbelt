using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Log;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.DataImportExport.MappedImport;
using System.Linq;

namespace JosephM.Xrm.MigrateInternal
{
    public class MigrateInternalValidationDialog : DialogViewModel
    {
        public MigrateInternalValidationDialog(MigrateInternalDialog parentDialog, MigrateInternalRequest importRequest)
            : base(parentDialog)
        {
            MigrateInternalService = parentDialog.Service;
            XrmRecordService = parentDialog.Service.XrmRecordService;
            Request = importRequest;
        }

        public XrmRecordService XrmRecordService { get; private set; }

        public MigrateInternalService MigrateInternalService { get; private set; }
        public MigrateInternalRequest Request { get; }

        protected override void CompleteDialogExtention()
        {
        }

        protected override void LoadDialogExtention()
        {
            var logController = new LogController(LoadingViewModel);
            var dictionary = MigrateInternalService.LoadMappingDictionaryWithSourceData(Request, logController);
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
