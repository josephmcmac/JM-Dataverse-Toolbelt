using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Core.Sql;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;

namespace JosephM.Record.Sql
{
    public class SqlRecordService : SqlRecordServiceBase
    {
        private SqlServerAndDbSettings Connection { get; set; }

        public SqlRecordService(SqlServerAndDbSettings connection)
        {
            Connection = connection;
        }

        protected SqlProvider CreateSqlService()
        {
            return new SqlProvider(Connection);
        }

        public void ExecuteSql(string sql, SqlProvider service, SqlTransaction transaction)
        {
            service.ExecuteNonQuery(sql, transaction);
        }

        protected override IEnumerable<QueryRow> ExecuteSelect(string selectQuery)
        {
            return ExecuteSelect(selectQuery, null, null);
        }

        protected IEnumerable<QueryRow> ExecuteSelect(string selectQuery, SqlProvider service, SqlTransaction transaction)
        {
            if (service != null)
                return service.SelectRows(selectQuery, transaction);
            using (var sqlService = CreateSqlService())
            {
                return sqlService.SelectRows(selectQuery);
            }
        }

        public override void ExecuteSql(string sql)
        {
            using (var sqlService = CreateSqlService())
            {
                sqlService.ExecuteNonQuery(sql);
            }
        }

        public override IEnumerable<string> GetAllRecordTypes()
        {
            var sql = "select name from sys.tables";
            var results = ExecuteSelect(sql);
            return results
                .Select(r => r.GetFieldAsString("name"))
                .ToArray();
        }


        private object _lockObject = new object();
        public override void ClearCache()
        {
            lock (_lockObject)
            {
                _fieldMetadatas = new Dictionary<string, IEnumerable<IFieldMetadata>>();
            }
        }

        private IDictionary<string, IEnumerable<IFieldMetadata>> _fieldMetadatas = new Dictionary<string, IEnumerable<IFieldMetadata>>();

        public override IEnumerable<IFieldMetadata> GetFieldMetadata(string recordType)
        {
            lock (_lockObject)
            {
                if (!_fieldMetadatas.ContainsKey(recordType))
                {
                    var sql = "select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = " +
                              SqlProvider.ToSqlString(recordType);
                    var results = ExecuteSelect(sql);
                    _fieldMetadatas.Add(recordType, results.Select(r =>
                        new StringFieldMetadata(recordType, r.GetFieldAsString("COLUMN_NAME"),
                            r.GetFieldAsString("COLUMN_NAME")))
                        .ToArray());
                }
                return _fieldMetadatas[recordType];
            }
        }

        public override void CreateOrUpdate(RecordMetadata recordMetadata)
        {
            if (!this.RecordTypeExists(recordMetadata.SchemaName))
            {
                if (recordMetadata.Fields == null || !recordMetadata.Fields.Any())
                    throw new NullReferenceException(string.Format("Columns are required to create the table but there are no fields in the metadata"));
                var sql = new StringBuilder();
                sql.Append(string.Format("CREATE TABLE [dbo].[{0}]\n", recordMetadata.SchemaName));
                sql.Append("(\n");
                sql.Append(string.Join(",\n", recordMetadata.Fields.Select(f => GetFieldDefinition(f, true))));
                sql.Append(" ) ");
                ExecuteSql(sql.ToString());
            }
            else
                throw new NotImplementedException("Update not implemented");
        }

        public override void CreateOrUpdate(FieldMetadata field, string recordType)
        {
            if (!this.RecordTypeExists(recordType))
                throw new NullReferenceException(string.Format("There is no table with name {0}", recordType));
            var fieldExists = this.FieldExists(field.SchemaName, recordType);
            var fieldSqlDefinition = GetFieldDefinition(field, !fieldExists);
            var sql = string.Format("ALTER TABLE [dbo].[{0}] {1} {2}"
                , recordType, fieldExists ? "ALTER  COLUMN" : "ADD", fieldSqlDefinition);
            ExecuteSql(sql);
        }

        public override IsValidResponse VerifyConnection()
        {
            var response = new IsValidResponse();
            try
            {
                var dbExists = SqlProvider.DatabaseExists(Connection.SqlServer, Connection.Database);
                if (!dbExists)
                    response.AddInvalidReason(string.Format("The database {0} could not be found on the server", Connection.Database));
            }
            catch (Exception ex)
            {
                response.AddInvalidReason(string.Format("A connection to verify the database could not be made: {0}", ex.DisplayString()));
            }
            return response;
        }

        private string GetFieldDefinition(FieldMetadata field, bool includeIdentity)
        {
            var type = GetSqlType(field);
            var fieldSqlDefinition = string.Format("[{0}] {1} {2} {3}"
                , field.SchemaName
                , type
                , includeIdentity && field.IsPrimaryKey && field is BigIntFieldMetadata ? "IDENTITY(1, 1)" : null
                , field.IsPrimaryKey || field.IsMandatory ? "NOT NULL" : "NULL");
            return fieldSqlDefinition;
        }

        private string GetSqlType(FieldMetadata field)
        {
            if (field is BooleanFieldMetadata)
            {
                return "bit";
            }
            if (field is DateFieldMetadata)
            {
                return "datetime";
            }
            if (field is BigIntFieldMetadata)
            {
                return "bigint";
            }
            if (field is LookupFieldMetadata)
            {
                return "bigint";
            }
            return "nvarchar(max)";
        }

        public override IsValidResponse IsValidForNewType(string typeName)
        {
            var response = new IsValidResponse();
            var objects =
                ExecuteSelect("select name, type_desc from sys.objects where name =" + SqlProvider.ToSqlString(typeName));
            if (objects.Any())
                response.AddInvalidReason(string.Format("There is already a '{0}' object in the database named {1}", objects.First().GetFieldAsString("type_desc"), typeName));
            return response;
        }
    }
}