using JosephM.Core.Sql;
using JosephM.Record.Service;

namespace JosephM.Record.Sql
{
    public class CsvRecord : RecordObject
    {
        public CsvRecord(string type, QueryRow queryRow)
            : base(type)
        {
            foreach(var column in queryRow.GetColumnNames())
                SetField(column, queryRow.GetField(column));
        }

        public CsvRecord(string type)
            : base(type)
        {
        }

        public override string GetStringField(string field)
        {
            var fieldValue = GetField(field);
            return fieldValue == null ? null : fieldValue.ToString();
        }
    }
}
