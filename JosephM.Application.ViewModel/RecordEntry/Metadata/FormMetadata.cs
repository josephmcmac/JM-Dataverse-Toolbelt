using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.RecordEntry.Metadata
{
    public class FormMetadata
    {
        public FormMetadata(IEnumerable<FormSection> formSections)
        {
            FormSections = formSections;
        }

        public FormMetadata(IEnumerable<string> fields, string heading = null)
        {
            FormSections = new[] {new FormFieldSection(heading ?? "Fields", fields.Select(s => new PersistentFormField(s)))};
        }

        public FormMetadata(FormSection formSection)
        {
            FormSections = new[] {formSection};
        }

        public IEnumerable<FormSection> FormSections { get; private set; }

        public string GridOnlyField { get; set; }
    }
}