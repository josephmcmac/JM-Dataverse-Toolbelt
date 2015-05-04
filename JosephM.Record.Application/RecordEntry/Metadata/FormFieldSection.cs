#region

using System.Collections.Generic;

#endregion

namespace JosephM.Record.Application.RecordEntry.Metadata
{
    public class FormFieldSection : FormSection
    {
        public FormFieldSection(string sectionLabel, IEnumerable<FormFieldMetadata> formFields)
            : base(sectionLabel)
        {
            FormFields = formFields;
        }

        public IEnumerable<FormFieldMetadata> FormFields { get; private set; }
    }
}