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
    public abstract class CreateViewModel : RecordEntryFormViewModel, INavigationAware
    {
        private IRecord _record;

        protected CreateViewModel(FormController formController)
            : base(formController)
        {
            OnSave = () => RecordService.Create(GetRecord());
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

        #region INavigationAware Members

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            OnNavigatedTo(new PrismNavigationProvider(navigationContext));
        }

        #endregion
    }
}