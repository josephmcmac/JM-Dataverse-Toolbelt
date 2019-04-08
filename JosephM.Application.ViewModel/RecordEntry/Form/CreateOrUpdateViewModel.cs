using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class CreateOrUpdateViewModel : RecordEntryFormViewModel
    {
        private IRecord _record;

        public CreateOrUpdateViewModel(IRecord record, FormController formController, Action postSave, Action onCancel, bool explicitIsCreate = false, string cancelButtonLabel = null)
            : base(formController, saveButtonLabel: record.Id == null ? "Create" : "Update", cancelButtonLabel: cancelButtonLabel)
        {
            _record = record;
            ExplicitIsCreate = explicitIsCreate;
            RecordType = record.Type;
            OnSave = () =>
            {
                LoadingViewModel.IsLoading = true;
                try
                {
                    if (GetRecord().Id == null || explicitIsCreate)
                        GetRecord().Id = RecordService.Create(GetRecord());
                    else
                        RecordService.Update(GetRecord(), ChangedPersistentFields);
                    if (postSave != null)
                        postSave();
                    LoadingViewModel.IsLoading = true;
                }
                finally
                {
                    LoadingViewModel.IsLoading = false;
                }
            };
            OnCancel = onCancel;
        }

        public override string TabLabel
        {
            get {
                if (GetRecord()?.Id == null || ExplicitIsCreate)
                {
                    return "Create " + RecordService.GetDisplayName(RecordType);
                }
                else
                    return GetRecord().GetStringField(RecordService.GetPrimaryField(RecordType)) ?? ("Update " + RecordService.GetDisplayName(RecordType));
            }
        }

        public bool ExplicitIsCreate { get; }

        public override IRecord GetRecord()
        {
            return _record;
        }
    }
}