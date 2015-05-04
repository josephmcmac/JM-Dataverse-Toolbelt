#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace JosephM.Record.Application.RecordEntry.Metadata
{
    public class FormMetadata
    {
        public FormMetadata(IEnumerable<FormSection> formSections)
        {
            FormSections = formSections;
        }

        public FormMetadata(IEnumerable<string> fields)
        {
            FormSections = new[] {new FormFieldSection("Fields", fields.Select(s => new PersistentFormField(s)))};
        }

        public FormMetadata(FormSection formSection)
        {
            FormSections = new[] {formSection};
        }

        public IEnumerable<FormSection> FormSections { get; private set; }
    }
}