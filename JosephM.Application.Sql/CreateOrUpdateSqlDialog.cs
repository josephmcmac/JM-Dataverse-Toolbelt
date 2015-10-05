using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Sql;

namespace JosephM.Migration.Prism.Module.Sql
{
    public class CreateOrUpdateSqlDialog<T> : DialogViewModel
        where T : new()
    {
        public T SqlObject { get; set; }

        public ISqlRecordMetadataService RecordService { get; set; }

        public CreateOrUpdateSqlDialog(IDialogController dialogController, ISqlRecordMetadataService recordService, T objectLoaded)
            : base(dialogController)
        {
            SqlObject = objectLoaded;
            if(objectLoaded == null)
                SqlObject = new T();
            RecordService = recordService;

            var configEntryDialog = new ObjectEntryDialog(SqlObject, this, ApplicationController, RecordService,
                null, () => { SaveObject(); });
            SubDialogs = new DialogViewModel[] { configEntryDialog };
        }

        public CreateOrUpdateSqlDialog(IDialogController dialogController, ISqlRecordMetadataService recordService)
            : this(dialogController, recordService, new T())
        {
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            CompletionMessage = "Object Created";
        }

        protected virtual string SaveObject()
        {
            return RecordService.SaveObject(SqlObject);
        }
    }
}