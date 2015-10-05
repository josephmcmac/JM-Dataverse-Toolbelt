using System;
using System.Collections.Generic;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Record.Sql;

namespace JosephM.Migration.Prism.Module.Sql
{
    public class ViewSqlRecordDialog : DialogViewModel
    {
        public OpenSqlRecordViewModel ViewModel { get; set; }
        private string Type { get; set; }
        private string Id { get; set; }
        protected ISqlRecordMetadataService RecordService { get; set; }

        public ViewSqlRecordDialog(ISqlRecordMetadataService recordService, IDialogController dialogController, string type, string id, Action onCancel)
            : base(dialogController)
        {
            OnCancel = onCancel;
            Id = id;
            Type = type;
            RecordService = recordService;
        }

        protected override void LoadDialogExtention()
        {
            ViewModel = new OpenSqlRecordViewModel(new FormController(RecordService,
                new RecordMetadataFormService(RecordService.RecordMetadata), ApplicationController), OnCancel);
            var record = RecordService.Get(Type, Id);
            ViewModel.SetRecord(record);
            Controller.LoadToUi(ViewModel);
        }

        protected override void CompleteDialogExtention()
        {
        }
    }
}
