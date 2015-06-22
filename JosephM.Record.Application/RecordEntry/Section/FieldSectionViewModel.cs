#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JosephM.Record.Application.RecordEntry.Field;
using JosephM.Record.Application.RecordEntry.Form;
using JosephM.Record.Application.RecordEntry.Metadata;

#endregion

namespace JosephM.Record.Application.RecordEntry.Section
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
                fieldViewModels.Add(fieldVm);
            }
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
    }
}