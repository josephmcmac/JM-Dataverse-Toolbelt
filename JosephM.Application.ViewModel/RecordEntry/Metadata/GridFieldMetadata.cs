using JosephM.Record.Metadata;

namespace JosephM.Application.ViewModel.RecordEntry.Metadata
{
    public class GridFieldMetadata : FormFieldMetadata
    {
        public GridFieldMetadata(string fieldName, double? widthPart = null)
            : base(fieldName)
        {
            WidthPart = widthPart ?? 200;
        }

        public GridFieldMetadata(ViewField viewField)
            : this(viewField.FieldName)
        {
            WidthPart = viewField.Width;
            IsReadOnly = viewField.ReadOnly;
        }

        public double WidthPart { get; set; }
        public bool IsReadOnly { get; set; }
        public string OverrideLabel { get; internal set; }
    }
}