using JosephM.Record.Metadata;

namespace JosephM.Record.Application.RecordEntry.Metadata
{
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