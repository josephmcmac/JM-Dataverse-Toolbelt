namespace JosephM.Application.ViewModel.RecordEntry.Metadata
{
    public class PersistentFormField : FormFieldMetadata
    {
        public PersistentFormField(string fieldName, string otherType = null, bool displayLabel = true)
            : base(fieldName, otherType, displayLabel)
        {
        }
    }
}