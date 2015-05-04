using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JosephM.Record.Sql
{
    [Serializable]
    public class SqlRecord : RecordBase
    {
        private readonly DataRow _row;

        internal SqlRecord(DataRow row)
            : base(row.Table.TableName)
        {
            _row = row;
        }

        public override IEnumerable<string> GetFieldsInEntity()
        {
            return (from DataColumn column in _row.Table.Columns select column.ColumnName).ToList();
        }

        public override object GetField(string fieldName)
        {
            return !IsDbNull(fieldName) ? _row[fieldName] : null;
        }

        private bool IsDbNull(string fieldName)
        {
            return _row[fieldName] is DBNull;
        }

        public override void SetField(string field, object value, IRecordService service)
        {
            throw new NotImplementedException();
        }

        public override bool ContainsField(string field)
        {
            throw new NotImplementedException();
        }
    }
}