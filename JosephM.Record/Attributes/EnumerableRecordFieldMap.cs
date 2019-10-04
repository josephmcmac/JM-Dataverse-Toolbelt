namespace JosephM.Record.Attributes
{
    public class EnumerableRecordFieldMap : RecordPropertyMap
    {
        public string LookupField { get; set; }

        public EnumerableRecordFieldMap(string lookupField)
        {
            LookupField = lookupField;
        }
    }
}
