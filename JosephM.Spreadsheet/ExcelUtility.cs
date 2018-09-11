using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Core.Sql;

namespace JosephM.Spreadsheet
{
    /// <summary>
    ///     Provides several methods for reading data into Datatable objects from an Excel file
    /// </summary>
    public static class ExcelUtility
    {
        /// <summary>
        ///     Converts a sheets display name into the internal name used by excel
        /// </summary>
        /// <param name="tabName"></param>
        /// <returns></returns>
        private static string GetInternalTabName(string tabName)
        {
            return tabName.Contains(" ") ? "'" + tabName + "$'" : tabName + "$";
        }

        /// <returns>DataTable containing the rows and columns from the excel sheet</returns>
        public static IEnumerable<QueryRow> SelectFromExcel(string fileName, string query)
        {
            OleDbDataAdapter dAdapter = null;
            try
            {
                var connString = GetConnectionString(fileName);
                var excelQuery = query;
                var dTable = new DataTable();
                dAdapter = new OleDbDataAdapter(excelQuery, connString);
                dAdapter.Fill(dTable);
                var itemsToAdd = new List<DataRow>();
                foreach (DataRow row in dTable.Rows)
                {
                    itemsToAdd.Add(row);
                }
                return itemsToAdd.Select(r => new QueryRow(r)).ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing query from excel\nFile: " + fileName + "\nQuery: " + query + "\n" +
                                    ex.DisplayString());
            }
            finally
            {
                if (dAdapter != null)
                {
                    dAdapter.Dispose();
                }
            }
        }

        public static IEnumerable<QueryRow> SelectPropertyBagsFromExcelTabName(string fileName, string tabName)
        {
            var excelQuery = "select * from [" + GetInternalTabName(tabName) + "]";
            return SelectFromExcel(fileName, excelQuery);
        }

        /// <summary>
        ///     This method retrieves the excel sheet names from an excel workbook.
        /// </summary>
        /// <param name="excelFile">The excel file.</param>
        /// <returns>String[]</returns>
        public static IDictionary<string, IEnumerable<string>> GetExcelColumnNames(string excelFile)
        {
            var result = new Dictionary<string, List<string>>();

            OleDbConnection objConn = null;
            DataTable dt = null;

            try
            {
                var connString = GetConnectionString(excelFile);
                objConn = new OleDbConnection(connString);
                objConn.Open();
                dt = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, null);

                var ignoreColumnNames = new List<string>();
                for (var i = 0; i < 100; i++)
                    ignoreColumnNames.Add("F" + i);

                if (dt != null)
                {
                    var excelSheets = new String[dt.Rows.Count];
                    foreach (DataRow row in dt.Rows)
                    {

                        var table = row["TABLE_NAME"].ToString();
                        var column = row["COLUMN_NAME"].ToString();
                        if (!ignoreColumnNames.Contains(column))
                        {
                            if (!result.ContainsKey(table))
                                result.Add(table, new List<string>());
                            result[table].Add(column);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to read sheet names from Excel File (" + excelFile + ") - " +
                                    ex.DisplayString());
            }
            finally
            {
                if (objConn != null)
                {
                    objConn.Close();
                    objConn.Dispose();
                }
                if (dt != null)
                {
                    dt.Dispose();
                }
            }
            return result.ToDictionary(kv => kv.Key, kv => (IEnumerable<string>)kv.Value);
        }

        /// <summary>
        ///     This method retrieves the excel sheet names from an excel workbook.
        /// </summary>
        /// <param name="excelFile">The excel file.</param>
        /// <returns>String[]</returns>
        public static IEnumerable<string> GetExcelTabNames(string excelFile)
        {
            OleDbConnection objConn = null;
            DataTable dt = null;

            try
            {
                var connString = GetConnectionString(excelFile);
                objConn = new OleDbConnection(connString);
                objConn.Open();
                dt = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                if (dt != null)
                {
                    var excelSheets = new String[dt.Rows.Count];
                    var i = 0;

                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[i] = row["TABLE_NAME"].ToString();
                        i++;
                    }

                    return excelSheets;
                }
                return new string[] {};
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to read sheet names from Excel File (" + excelFile + ") - " +
                                    ex.DisplayString());
            }
            finally
            {
                if (objConn != null)
                {
                    objConn.Close();
                    objConn.Dispose();
                }
                if (dt != null)
                {
                    dt.Dispose();
                }
            }
        }

        /// <summary>
        ///     The connection string for reading data from an excel file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string GetConnectionString(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("fileName");
            if(fileName.EndsWith(".xls"))
                return @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\"";
            return "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=1\"";
        }
    }
}