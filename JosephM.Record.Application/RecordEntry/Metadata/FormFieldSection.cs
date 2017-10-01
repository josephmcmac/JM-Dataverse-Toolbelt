#region

using JosephM.Core.Attributes;
using System.Collections.Generic;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Metadata
{
    public class FormFieldSection : FormSection
    {
        public FormFieldSection(string sectionLabel, IEnumerable<FormFieldMetadata> formFields, Group.DisplayLayoutEnum displayLayout = Group.DisplayLayoutEnum.VerticalList, int order = 10000)
            : base(sectionLabel, order)
        {
            FormFields = formFields;
            DisplayLayout = displayLayout;
        }

        public IEnumerable<FormFieldMetadata> FormFields { get; private set; }

        public Group.DisplayLayoutEnum DisplayLayout { get; set; }
    }
}