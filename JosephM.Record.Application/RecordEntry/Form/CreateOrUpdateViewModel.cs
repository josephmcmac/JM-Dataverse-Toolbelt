#region

using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class CreateOrUpdateViewModel : RecordEntryFormViewModel
    {
        private IRecord _record;

        public CreateOrUpdateViewModel(IRecord record, FormController formController, Action postSave, Action onCancel, bool explicitIsCreate = false)
            : base(formController, saveButtonLabel: record.Id == null ? "Create" : "Update")
        {
            _record = record;
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
            get { return "Create Or Update"; }
        }

        public override IRecord GetRecord()
        {
            return _record;
        }
    }
}