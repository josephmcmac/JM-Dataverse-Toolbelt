#region

using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.Shared;
using JosephM.Record.IService;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Section
{
    public abstract class SectionViewModelBase : ViewModelBase
    {
        private bool _isVisible = true;

        protected SectionViewModelBase(
            FormSection formSection,
            RecordEntryViewModelBase recordForm
            )
            : base(recordForm.FormController.ApplicationController)
        {
            FormSection = formSection;
            RecordForm = recordForm;
        }

        public FormSection FormSection { get; private set; }
        public RecordEntryViewModelBase RecordForm { get; private set; }

        public virtual string SectionIdentifier
        {
            get { return SectionLabel; }
        }

        public IRecordService RecordService
        {
            get { return FormController.RecordService; }
        }

        public string SectionLabel
        {
            get { return FormSection.SectionLabel; }
        }

        public IFormService FormService
        {
            get { return FormController.FormService; }
        }

        public FormController FormController
        {
            get { return RecordForm.FormController; }
        }

        public FormMetadata FormMetadata
        {
            get { return FormController.FormService.GetFormMetadata(RecordType, RecordService); }
        }

        public abstract string RecordType { get; }

        internal virtual bool Validate()
        {
            return true;
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }

        public abstract bool IsLoaded { get; }

        public void LoadCustomFunctions()
        {
            ApplicationController.DoOnMainThread(() =>
            {

                _customFunctions = new ObservableCollection<XrmButtonViewModel>(RecordForm is RecordEntryFormViewModel
                        ? ((RecordEntryFormViewModel)RecordForm).FormFunctionsToXrmButtons(FormSection.CustomFunctions)
                        : new XrmButtonViewModel[0]);

                OnPropertyChanged(nameof(CustomFunctions));
            });
        }

        private ObservableCollection<XrmButtonViewModel> _customFunctions;

        public ObservableCollection<XrmButtonViewModel> CustomFunctions
        {
            get
            {
                if(_customFunctions == null)
                {
                    LoadCustomFunctions();
                }
                return _customFunctions;
            }
        }
    }
}