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
    public abstract class OpenViewModel : RecordEntryFormViewModel
    {
        private IRecord _record;

        protected OpenViewModel(FormController formController)
            : base(formController)
        {
            OnCancel = () => ApplicationController.Remove(RegionNames.MainTabRegion, this);
        }

        public override IRecord GetRecord()
        {
            if (_record == null)
                LoadRecord();
            return _record;
        }


        public void SetRecord(IRecord record)
        {
            _record = record;
            RecordType = _record.Type;
            RecordId = _record.Id;
        }

        protected virtual void LoadRecord()
        {
            if (RecordId.IsNullOrWhiteSpace())
                throw new NullReferenceException("Record Id Cannot Be Empty When Loading The Record");
            if (RecordType.IsNullOrWhiteSpace())
                throw new NullReferenceException("Record Type Cannot Be Empty When Loading The Record");
            if (RecordIdName.IsNullOrWhiteSpace())
                RecordIdName = RecordService.GetPrimaryKey(RecordType);

            _record = RecordService.GetFirst(RecordType, RecordIdName, RecordId);
        }

        #region INavigationAware Members

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            OnNavigatedTo(new PrismNavigationProvider(navigationContext));
        }

        #endregion
    }
}