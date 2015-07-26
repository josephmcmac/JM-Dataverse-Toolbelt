namespace JosephM.Record.Application.RecordEntry.Metadata
{
    public class GridFieldMetadata : FormFieldMetadata
    {
        public GridFieldMetadata(string fieldName)
            : base(fieldName)
        {
            WidthPart = 200;
        }

        public double WidthPart { get; set; }

        public bool IsEditable { get; set; }
    }
}