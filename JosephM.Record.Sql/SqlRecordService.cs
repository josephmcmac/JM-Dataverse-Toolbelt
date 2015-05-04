using System.Collections.Generic;
using System.Data;
using System.Linq;
using JosephM.Core;

namespace JosephM.Record.Sql
{
    public class SqlRecordService : RecordServiceBase
    {
        private SqlRecordConfiguration SqlRecordConfiguration { get; set; }

        private SqlProvider SqlProvider { get; set; }

        public SqlRecordService(SqlRecordConfiguration sqlRecordConfiguration)
        {
            SqlRecordConfiguration = sqlRecordConfiguration;
            SqlProvider = new SqlProvider(sqlRecordConfiguration.GetConnectionString());
        }

        public override IEnumerable<IRecord> RetrieveAll(string recordType, IEnumerable<string> fields)
        {
            var query = CreateSelectString(fields, recordType);
            return ToRecords(SqlProvider.GetDataRows(query));
        }

        private IEnumerable<IRecord> ToRecords(IEnumerable<DataRow> rows)
        {
            return rows.Select(ToRecord);
        }

        private IRecord ToRecord(DataRow row)
        {
            return new SqlRecord(row);
        }

        private string CreateSelectString(IEnumerable<string> fields, string table)
        {
            return "select [{0}] from [{1}]".FormatString(string.Join("],[", fields), table);
        }
    }
}