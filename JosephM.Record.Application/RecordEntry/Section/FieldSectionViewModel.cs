#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Section
{
    public class FieldSectionViewModel : SectionViewModelBase
    {
        private ObservableCollection<FieldViewModelBase> _fields;

        public FieldSectionViewModel(
            FormFieldSection formSection,
            RecordEntryFormViewModel recordForm
            )
            : base(formSection, recordForm)
        {
        }

        public bool WrapHorizontal {  get { return FormFieldSection.WrapHorizontal; } }

        private FormFieldSection FormFieldSection
        {
            get { return FormSection as FormFieldSection; }
        }

        public ObservableCollection<FieldViewModelBase> Fields
        {
            get
            {
                if (_fields == null)
                {
                    _fields = CreateFieldViewModels(FormFieldSection.FormFields);
                }
                return _fields;
            }
        }

        private ObservableCollection<FieldViewModelBase> CreateFieldViewModels(IEnumerable<FormFieldMetadata> formFields)
        {
            var fieldViewModels = new ObservableCollection<FieldViewModelBase>();
            foreach (var formField in formFields.OrderBy(f => f.Order))
            {
                var fieldVm = formField.CreateFieldViewModel(RecordType, RecordService, RecordForm,
                    ApplicationController);
                if (IsReadOnly)
                    fieldVm.IsEditable = false;
                fieldViewModels.Add(fieldVm);
            }
            IsLoaded = true;
            return fieldViewModels;
        }

        public override string RecordType
        {
            get { return RecordForm.RecordType; }
        }

        internal override bool Validate()
        {
            var isValid = true;

            foreach (var recordField in Fields)
            {
                if (recordField.IsVisible && !recordField.Validate())
                    isValid = false;
            }
            return isValid;
        }

        public bool IsReadOnly { get { return RecordForm.IsReadOnly; } }
    }
}