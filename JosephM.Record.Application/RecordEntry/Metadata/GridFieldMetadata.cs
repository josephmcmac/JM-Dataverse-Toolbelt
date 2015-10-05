using JosephM.Record.Metadata;

namespace JosephM.Application.ViewModel.RecordEntry.Metadata
{
    public class GridFieldMetadata : FormFieldMetadata
    {
        public GridFieldMetadata(string fieldName)
            : base(fieldName)
        {
            WidthPart = 200;
        }

        public GridFieldMetadata(ViewField viewField)
            : this(viewField.FieldName)
        {
            WidthPart = viewField.Width;
        }

        public double WidthPart { get; set; }

        public bool IsEditable { get; set; }
    }
}