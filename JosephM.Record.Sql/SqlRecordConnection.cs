using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Core.Sql;

namespace JosephM.Record.Sql
{
    [ServiceConnection(typeof(SqlRecordService))]
    public class SqlRecordConnection : SqlServerAndDbSettings, IValidatableObject
    {
        public SqlRecordConnection()
        {

        }

        public SqlRecordConnection(string sqlServer, string database)
            : base(sqlServer, database)
        {

        }

        public IsValidResponse Validate()
        {
            var service = new SqlRecordService(this);
            return service.VerifyConnection();
        }
    }
}
