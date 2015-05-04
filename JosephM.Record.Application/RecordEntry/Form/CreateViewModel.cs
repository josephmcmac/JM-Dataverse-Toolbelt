#region

using System;
using Microsoft.Practices.Prism.Regions;
using JosephM.Core.Extentions;
using JosephM.Record.Application.Navigation;
using JosephM.Record.IService;

#endregion

namespace JosephM.Record.Application.RecordEntry.Form
{
    public abstract class CreateViewModel : RecordEntryFormViewModel, INavigationAware
    {
        private IRecord _record;

        protected CreateViewModel(FormController formController)
            : base(formController)
        {
        }

        public override string TabLabel
        {
            get { return "Create"; }
        }

        public override string SaveButtonLabel
        {
            get { return "Create"; }
        }

        public override IRecord GetRecord()
        {
            if (_record == null)
            {
                if (RecordType.IsNullOrWhiteSpace())
                    throw new NullReferenceException("Record Tyoe Is Empty");
                _record = RecordService.NewRecord(RecordType);
            }
            return _record;
        }

        public override void OnSaveExtention()
        {
            RecordService.Create(GetRecord());
        }

        #region INavigationAware Members

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            OnNavigatedTo(new PrismNavigationProvider(navigationContext));
        }

        #endregion
    }
}