#region

using System;
using Microsoft.Practices.Prism.Regions;
using JosephM.Core.Extentions;
using JosephM.Record.Application.Navigation;
using JosephM.Record.IService;

#endregion

namespace JosephM.Record.Application.RecordEntry.Form
{
    public abstract class OpenViewModel : RecordEntryFormViewModel
    {
        private IRecord _record;

        protected OpenViewModel(FormController formController)
            : base(formController)
        {
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