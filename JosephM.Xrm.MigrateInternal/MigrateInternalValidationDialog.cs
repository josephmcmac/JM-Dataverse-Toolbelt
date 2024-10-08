﻿using JosephM.Application.ViewModel.Dialog;
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
            //okay lets load the spreadsheet
            //and if there are any error display them
            //else continue
            var logController = new LogController(LoadingViewModel);
            var dictionary = MigrateInternalService.LoadMappingDictionary(Request, logController);

            var importService = new MappedImportService(XrmRecordService);
            var parseResponse = importService.ParseIntoEntities(dictionary, logController);
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
