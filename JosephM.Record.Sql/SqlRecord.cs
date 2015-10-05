using JosephM.Core.Sql;
using JosephM.Record.Service;

namespace JosephM.Record.Sql
{
    public class SqlRecord : RecordObject
    {
        public SqlRecord(string type, QueryRow queryRow)
            : base(type)
        {
            foreach(var column in queryRow.GetColumnNames())
                SetField(column, queryRow.GetField(column));
        }

        public SqlRecord(string type)
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
