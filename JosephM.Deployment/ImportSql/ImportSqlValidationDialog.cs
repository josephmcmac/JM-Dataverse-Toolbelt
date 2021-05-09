﻿using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Log;
using JosephM.Deployment.SpreadsheetImport;
using JosephM.Record.Xrm.XrmRecord;
using System.Linq;

namespace JosephM.Deployment.ImportSql
{
    public class ImportSqlValidationDialog : DialogViewModel
    {
        public ImportSqlValidationDialog(ImportSqlDialog parentDialog, ImportSqlRequest importRequest)
            : base(parentDialog)
        {
            ImportSqlService = parentDialog.Service;
            XrmRecordService = parentDialog.Service.XrmRecordService;
            Request = importRequest;
        }

        public XrmRecordService XrmRecordService { get; private set; }

        public ImportSqlService ImportSqlService { get; private set; }
        public ImportSqlRequest Request { get; }

        protected override void CompleteDialogExtention()
        {
        }

        protected override void LoadDialogExtention()
        {
            //okay lets load the spreadsheet
            //and if there are any error display them
            //else continue
            var logController = new LogController(LoadingViewModel);
            var dictionary = ImportSqlService.LoadMappingDictionary(Request);

            var importService = new SourceImportService(XrmRecordService);
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
