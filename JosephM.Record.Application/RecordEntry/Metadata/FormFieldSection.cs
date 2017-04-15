#region

using System.Collections.Generic;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Metadata
{
    public class FormFieldSection : FormSection
    {
        public FormFieldSection(string sectionLabel, IEnumerable<FormFieldMetadata> formFields, bool wrapHorizontal = false, int order = 10000)
            : base(sectionLabel, order)
        {
            FormFields = formFields;
            WrapHorizontal = wrapHorizontal;
        }

        public IEnumerable<FormFieldMetadata> FormFields { get; private set; }

        public bool WrapHorizontal { get; set; }
    }
}