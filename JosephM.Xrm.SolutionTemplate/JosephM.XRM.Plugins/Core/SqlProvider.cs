#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Linq;

#endregion

namespace $safeprojectname$.Core
{
    public abstract class SqlProvider<T> : IDisposable
        where T : DbConnection, new()
    {
        private int Timeout { get; set; }

        public SqlProvider(string connectionString)
        {
            Timeout = 600;
            DbConnection = new T();
            DbConnection.ConnectionString = connectionString;
            DbConnection.Open();
        }

        protected T DbConnection { get; set; }

        public static string ToSqlString(object value)
        {
            if (value == null)
                return "null";
            if (value is DateTime)
                return ToSqlDateString((DateTime)value);
            return @"'" + value.ToString().Left(255).Replace("'", "''") + @"'";
        }

        public static string ToSqlDateString(DateTime clrDate)
        {
            return "'" + clrDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
        }

        public IEnumerable<DataRow> GetDataRows(string sqlQuery)
        {
            var allData = GetDataTable(sqlQuery);
            var itemsToAdd = new List<DataRow>();
            foreach (DataRow row in allData.Rows)
            {
                itemsToAdd.Add(row);
            }
            return itemsToAdd;
        }

        public IEnumerable<QueryRow> SelectRows(string sqlQuery)
        {
            return GetDataRows(sqlQuery).Select(r => new QueryRow(r)).ToArray();
        }

        public abstract DbCommand GetCommand(string sql);
        /// <summary>
        ///     Executes a sql query into a DataTable object.
        /// </summary>
        /// <returns>The query results in a DataTable object</returns>
        public DataTable GetDataTable(string sqlQuery)
        {
            try
            {
                if (DbConnection.State != ConnectionState.Open)
                    DbConnection.Open();
                using (var myCommand = GetCommand(sqlQuery))
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

        public int ExecuteNonQuery(string sqlQuery, DbTransaction transaction = null)
        {
            try
            {
                if (DbConnection.State != ConnectionState.Open)
                    DbConnection.Open();
                using (var myCommand = GetCommand(sqlQuery))
                {
                    myCommand.CommandTimeout = Timeout;
                    if (transaction != null)
                        myCommand.Transaction = transaction;
                    var result = myCommand.ExecuteNonQuery();
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new SqlException(sqlQuery, ex);
            }
        }

        public void ExecuteScalar(string sqlQuery)
        {
            try
            {
                if (DbConnection.State != ConnectionState.Open)
                    DbConnection.Open();
                using (var myCommand = GetCommand(sqlQuery))
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

        public void InsertObjects<T>(IEnumerable<T> objects, string table)
        {
            InsertObjects(objects, table, new LogController());
        }

        public void InsertObjects<T>(IEnumerable<T> objects, string table, LogController ui)
        {
            if (objects != null)
            {
                var typeToOutput = objects.First().GetType();

                var todo = objects.Count();
                var done = 0;
                var propertyNames = typeToOutput.GetProperties().Select(p => p.Name).ToArray();
                for (var i = 0; i < objects.Count(); i++)
                {
                    ui.UpdateProgress(done++, todo, null);
                    var instance = objects.ElementAt(i);
                    var propertyPart = string.Format("[{0}]", String.Join("],[", propertyNames));
                    var propertyValues = propertyNames.Select(p => ToSqlString(instance.GetPropertyValue(p)));
                    var query = string.Format(@"insert into [{0}] ({1}) values ({2})", table, propertyPart,
                        string.Join(",", propertyValues));
                    ExecuteNonQuery(query);
                }
            }
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
    }

    public class SqlOdbcProvider : SqlProvider<OdbcConnection>
    {
        public SqlOdbcProvider(string connectionString)
               : base(connectionString)
        {

        }

        public OdbcTransaction BeginTransaction()
        {
            return DbConnection.BeginTransaction();
        }

        public override DbCommand GetCommand(string sql)
        {
            return new OdbcCommand(sql, DbConnection);
            
        }
    }

    /// <summary>
    ///     Contains several static functions for executing commands to an sql database
    /// </summary>
    public class SqlOledbProvider : SqlProvider<OleDbConnection>
    {
        public SqlOledbProvider(string connectionString)
            : base(connectionString)
        {

        }

        /// <summary>
        ///     Returns a sql connection string for the database
        /// </summary>
        /// <param name="dbServer">the sql server instance name</param>
        /// <param name="dbName">the database as the inital catalog</param>
        /// <returns></returns>
        private static string GetConnectionString(string dbServer, string dbName)
        {
            return "Provider=SQLOLEDB;Data Source=" + dbServer + ";Initial Catalog=" + dbName +
                   ";Integrated Security=SSPI;";
        }

        public override DbCommand GetCommand(string sql)
        {
            return new OleDbCommand(sql, DbConnection);
        }
    }
}