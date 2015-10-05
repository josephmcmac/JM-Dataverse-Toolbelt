#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using JosephM.Core.FieldType;

#endregion

namespace JosephM.Core.Sql
{
    /// <summary>
    ///     Contains several static functions for executing commands to an sql database
    /// </summary>
    public class SqlProvider : IDisposable
    {
        /// <summary>
        ///     NEED TO CALL CLOSE() WHEN FINISHED
        /// </summary>
        public SqlProvider(ISqlSettings settings)
            : this(settings.SqlServer, settings.Database)
        {
        }

        /// <summary>
        ///     NEED TO CALL CLOSE() WHEN FINISHED
        /// </summary>
        public SqlProvider(string sqlserver, string sqldatabase)
            : this(GetConnectionString(sqlserver, sqldatabase))
        {
        }

        /// <summary>
        ///     NEED TO CALL CLOSE() WHEN FINISHED
        /// </summary>
        public SqlProvider(string connectionString)
        {
            Timeout = 600;
            DbConnection = new OleDbConnection(connectionString);
            DbConnection.Open();
        }

        public SqlTransaction BeginTransacton()
        {
            if (DbConnection.State != ConnectionState.Open)
                DbConnection.Open();

            if (DbConnection.State != ConnectionState.Open)
                DbConnection.Open();
            var transaction = DbConnection.BeginTransaction();
            return new SqlTransaction(transaction);
        }

        private OleDbConnection DbConnection { get; set; }

        private int Timeout { get; set; }

        public static string ToSqlDateString(DateTime clrDate)
        {
            return "'" + clrDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
        }

        public IEnumerable<QueryRow> SelectRows(string sqlQuery)
        {
            return SelectRows(sqlQuery, null);
        }

        public IEnumerable<QueryRow> SelectRows(string selectQuery, SqlTransaction transaction)
        {
            return GetDataRows(selectQuery, transaction).Select(r => new QueryRow(r)).ToArray();
        }

        private IEnumerable<DataRow> GetDataRows(string sqlQuery, SqlTransaction transaction)
        {
            var allData = GetDataTable(sqlQuery, transaction);
            var itemsToAdd = new List<DataRow>();
            foreach (DataRow row in allData.Rows)
            {
                itemsToAdd.Add(row);
            }
            return itemsToAdd;
        }

        /// <summary>
        ///     Executes a sql query into a DataTable object.
        /// </summary>
        /// <returns>The query results in a DataTable object</returns>
        public DataTable GetDataTable(string sqlQuery, SqlTransaction transaction)
        {
            try
            {
                if (DbConnection.State != ConnectionState.Open)
                    DbConnection.Open();
                using (var myCommand = new OleDbCommand(sqlQuery, DbConnection, transaction == null ? null : transaction.Transaction))
                {
                    using (var myReader = myCommand.ExecuteReader())
                    {
                        myCommand.CommandTimeout = Timeout;
                        var myTable = new DataTable();
                        if (myReader != null) myTable.Load(myReader);
                        return myTable;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SqlException(sqlQuery, ex);
            }
        }

        /// <summary>
        ///     Returns a sql connection string for the database
        /// </summary>
        /// <param name="dbServer">the sql server instance name</param>
        /// <param name="dbName">the database as the inital catalog</param>
        /// <returns></returns>
        public static string GetConnectionString(string dbServer, string dbName)
        {
            return "Provider=sqloledb;Data Source=" + dbServer + ";Initial Catalog=" + dbName +
                   ";Integrated Security=SSPI;";
        }

        public void ExecuteNonQuery(string sqlQuery)
        {
            ExecuteNonQuery(sqlQuery, null);
        }

        public void ExecuteNonQuery(string sql, SqlTransaction transaction)
        {
            try
            {
                if (DbConnection.State != ConnectionState.Open)
                    DbConnection.Open();

                //var transaction = DbConnection.tra();
                //transaction.
                using (var myCommand = new OleDbCommand(sql, DbConnection))
                {
                    if (transaction != null)
                        myCommand.Transaction = transaction.Transaction;
                    myCommand.CommandTimeout = Timeout;
                    myCommand.UpdatedRowSource = UpdateRowSource.OutputParameters;
                    myCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new SqlException(sql, ex);
            }
        }

        public void ExecuteScalar(string sqlQuery)
        {
            try
            {
                if (DbConnection.State != ConnectionState.Open)
                    DbConnection.Open();
                using (var myCommand = new OleDbCommand(sqlQuery, DbConnection))
                {
                    myCommand.CommandTimeout = Timeout;
                    myCommand.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new SqlException(sqlQuery, ex);
            }
        }

        public static string ToSqlString(object value)
        {
            if (value == null)
                return "null";
            if (value is DateTime)
                return ToSqlDateString((DateTime)value);
            if (value is Lookup)
                return ToSqlString(((Lookup)value).Id);
            if (value.GetType().IsEnum)
                return ((int)value).ToString();
            if (value is bool)
                return ((bool)value) ? "1" : "0";
            return @"'" + value.ToString().Replace("'", "''") + @"'";
        }

        public void Dispose()
        {
            try
            {
                if (DbConnection != null && DbConnection.State == ConnectionState.Open)
                    DbConnection.Close();
            }
            catch (Exception)
            {
            }
        }

        public void ExecuteUpdate(string table, IDictionary<string, object> values, string idColumn, object idValue)
        {
            ExecuteNonQuery(GetUpdateString(table, values, idColumn, idValue));
        }

        public static string GetUpdateString(string table, IDictionary<string, object> values, string idColumn,
            object idValue)
        {
            var sql = string.Format("{0} where {1}={2}"
                , GetUpdateString(table, values)
                , string.Format("[{0}]", idColumn)
                , ToSqlString(idValue));

            return sql;
        }

        public IEnumerable<QueryRow> SelectWhere(string table, string idColumn, object idValue)
        {
            var sql = GetSelectWhere(table, idColumn, idValue);

            return SelectRows(sql);
        }

        public IEnumerable<QueryRow> SelectAllRows(string table)
        {
            var sql = GetSelectAllRows(table);
            return SelectRows(sql);
        }

        public static string GetSelectAllRows(string table)
        {
            var sql = string.Format("select * from {0}", ToIdentifier(table));
            return sql;
        }

        public static string GetSelectWhere(string table, string idColumn, object idValue)
        {
            var sql = string.Format("select * from {0} where {1}={2}"
                , ToIdentifier(table)
                , ToIdentifier(idColumn)
                , ToSqlString(idValue));
            return sql;
        }

        private static string ToIdentifier(string objectName)
        {
            return string.Format("[{0}]", objectName);
        }

        public static string GetUpdateString(string table, IDictionary<string, object> values)
        {
            var sql = string.Format("update {0} set {1}"
                , table
                , string.Join(",",
                    values.Select(kv => string.Format("{0}={1}", string.Format("[{0}]", kv.Key), ToSqlString(kv.Value)))));
            return sql;
        }

        public void ExecuteInsert(string table, Dictionary<string, object> values)
        {
            ExecuteNonQuery(GetInsertString(table, values, false));
        }

        public static string GetInsertString(string table, IDictionary<string, object> values, bool selectIdentity)
        {
            var sql = string.Format("insert into [{0}]  ({1}) {2} values ({3})"
                , table
                , string.Join(",", values.Select(k => string.Format("[{0}]", k.Key)))
                , selectIdentity ? " OUTPUT INSERTED.ID " : null
                , string.Join(",", values.Select(k => ToSqlString(k.Value))));
            return sql;
        }

        public bool SqlRowExists(string table, string column, object value)
        {
            var sql = string.Format("select top 1 1 from [{0}] where [{1}] = {2}"
                , table
                , column
                , ToSqlString(value));
            return SelectRows(sql).Any();
        }

        public int GetTableCount(string table)
        {
            var sql = string.Format("select count(*) as TheCount from {0}", table);
            return SelectRows(sql).First().GetFieldAsInteger("TheCount");
        }

        public static bool DatabaseExists(string sqlServer, string database)
        {
            using (var sqlService = CreateMasterConnection(sqlServer))
            {
                var rows =
                    sqlService.SelectRows(string.Format("select top 1 1 from [sys].[databases] where name = {0}",
                        ToSqlString(database)));
                return rows.Any();
            }
        }

        public static SqlProvider CreateMasterConnection(string sqlServer)
        {
            var connectionString = GetConnectionString(sqlServer, "master");
            return new SqlProvider(connectionString);
        }

        public static void CreateDatabase(string sqlServer, string database)
        {
            using (var sqlService = CreateMasterConnection(sqlServer))
            {
                var sql = string.Format("create database [{0}]", database);
                sqlService.ExecuteNonQuery(sql);
            }
        }
    }
}