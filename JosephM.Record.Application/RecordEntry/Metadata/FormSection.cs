namespace JosephM.Application.ViewModel.RecordEntry.Metadata
{
    public class FormSection
    {
        public FormSection(string sectionLabel, int order)
        {
            SectionLabel = sectionLabel;
            Order = order;
        }

        public string SectionLabel { get; private set; }

        public int Order { get; set; }
    }
}