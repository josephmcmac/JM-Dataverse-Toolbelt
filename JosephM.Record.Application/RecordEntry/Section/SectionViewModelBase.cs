#region

using JosephM.Record.Application.RecordEntry.Form;
using JosephM.Record.Application.RecordEntry.Metadata;
using JosephM.Record.IService;

#endregion

namespace JosephM.Record.Application.RecordEntry.Section
{
    public abstract class SectionViewModelBase : ViewModelBase
    {
        private bool _isVisible = true;

        protected SectionViewModelBase(
            FormController formController,
            FormSection formSection,
            RecordEntryFormViewModel recordForm
            )
            : base(formController.ApplicationController)
        {
            FormSection = formSection;
            RecordForm = recordForm;
            FormController = formController;
        }

        public FormSection FormSection { get; private set; }
        public RecordEntryFormViewModel RecordForm { get; private set; }

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

        public FormServiceBase FormService
        {
            get { return FormController.FormService; }
        }

        public FormController FormController { get; set; }

        public FormMetadata FormMetadata
        {
            get { return FormController.FormService.GetFormMetadata(RecordType); }
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
    }
}