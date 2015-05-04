#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Core.Log;

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

        private OleDbConnection DbConnection { get; set; }

        private int Timeout { get; set; }

        public static string ToSqlDateString(DateTime clrDate)
        {
            return "'" + clrDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
        }

        public IEnumerable<QueryRow> SelectRows(string sqlQuery)
        {
            return GetDataRows(sqlQuery).Select(r => new QueryRow(r));
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
                using (var myCommand = new OleDbCommand(sqlQuery, DbConnection))
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
        private static string GetConnectionString(string dbServer, string dbName)
        {
            return "Provider=sqloledb;Data Source=" + dbServer + ";Initial Catalog=" + dbName + ";Integrated Security=SSPI;";
        }

        public void ExecuteNonQuery(string sqlQuery)
        {
            try
            {
                if (DbConnection.State != ConnectionState.Open)
                    DbConnection.Open();
                using (var myCommand = new OleDbCommand(sqlQuery, DbConnection))
                {
                    myCommand.CommandTimeout = Timeout;
                    myCommand.ExecuteNonQuery();
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

        private string ToSqlString(object value)
        {
            if (value == null)
                return "null";
            if (value is DateTime)
                return ToSqlDateString((DateTime) value);
            return @"'" + value.ToString().Left(255).Replace("'", "''") + @"'";
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
}