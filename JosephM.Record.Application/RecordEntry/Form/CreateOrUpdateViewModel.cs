#region

using System;
using JosephM.Application.ViewModel.Navigation;
using JosephM.Core.Extentions;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using Microsoft.Practices.Prism.Regions;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class CreateOrUpdateViewModel : RecordEntryFormViewModel
    {
        private IRecord _record;

        public CreateOrUpdateViewModel(IRecord record, FormController formController, Action postSave, Action onCancel, bool explicitIsCreate = false)
            : base(formController)
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

        public override string SaveButtonLabel
        {
            get { return "Save"; }
        }

        public override IRecord GetRecord()
        {
            return _record;
        }
    }
}