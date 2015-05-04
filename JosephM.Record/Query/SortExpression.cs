namespace JosephM.Record.Query
{
    public class SortExpression
    {
        public SortExpression(string fieldName, SortType sortType)
        {
            FieldName = fieldName;
            SortType = sortType;
        }

        public string FieldName { get; set; }
        public SortType SortType { get; set; }
    }
}