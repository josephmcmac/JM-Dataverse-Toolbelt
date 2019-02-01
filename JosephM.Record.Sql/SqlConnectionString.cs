using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using System;

namespace JosephM.Record.Sql
{
    [ServiceConnection(typeof(SqlRecordService))]
    public class SqlConnectionString : IValidatableObject
    {
        public SqlConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; set; }

        public override string ToString()
        {
            return "SQL Connection";
        }

        public IsValidResponse Validate()
        {
            try
            {
                var service = new SqlRecordService(this);
                return service.VerifyConnection();
            }
            catch (Exception ex)
            {
                var response = new IsValidResponse();
                response.AddInvalidReason("Error creating service: " + ex.DisplayString());
                return response;
            }
        }
    }
}