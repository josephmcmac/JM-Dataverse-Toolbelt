using JosephM.Record.Metadata;

namespace JosephM.Application.ViewModel.RecordEntry.Metadata
{
    /// <summary>
    /// class initially written to support implementing a derived field
    /// never actually used and could probably be removed or rewritten
    /// </summary>
    public class NonPersistentFormField : FormFieldMetadata
    {
        public NonPersistentFormField(string fieldName, string label, RecordFieldType recordFieldType)
            : base(fieldName)
        {
            RecordFieldType = recordFieldType;
            Label = label;
        }

        public string Label { get; private set; }
        public RecordFieldType RecordFieldType { get; private set; }
    }
}