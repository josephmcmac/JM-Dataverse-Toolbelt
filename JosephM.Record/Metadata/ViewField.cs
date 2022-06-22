namespace JosephM.Record.Metadata
{
    public class ViewField
    {
        public ViewField(string fieldName, int order, int width)
        {
            FieldName = fieldName;
            Order = order;
            Width = width;
        }

        public string FieldName { get; private set; }
        public int Width { get; set; }
        public int Order { get; set; }
        public bool ReadOnly { get; set; }
    }
}