#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Core.Sql;

#endregion

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

        /// <summary>
        ///     Reads data from Excel into DataTable object
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="tabName"></param>
        /// <returns>DataTable containing the rows and columns from the excel sheet</returns>
        private static DataTable SelectFromExcelTabName(string fileName, string tabName)
        {
            OleDbDataAdapter dAdapter = null;
            try
            {
                var connString = GetConnectionString(fileName);
                var excelQuery = "select * from [" + GetInternalTabName(tabName) + "]";
                var dTable = new DataTable();
                dAdapter = new OleDbDataAdapter(excelQuery, connString);
                dAdapter.Fill(dTable);
                return dTable;
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading from excel tab\nFile: " + fileName + "\nTab: " + tabName + "\n" +
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
            return SelectRowsFromExcelTabName(fileName, tabName).Select(r => new QueryRow(r));
        }


        /// <summary>
        ///     Reads data from Excel into DataTable object
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="tabName"></param>
        /// <returns>DataTable containing the rows and columns from the excel sheet</returns>
        private static IEnumerable<DataRow> SelectRowsFromExcelTabName(string fileName, string tabName)
        {
            var allData = SelectFromExcelTabName(fileName, tabName);
            var itemsToAdd = new List<DataRow>();
            foreach (DataRow row in allData.Rows)
            {
                itemsToAdd.Add(row);
            }
            return itemsToAdd;
        }

        /// <summary>
        ///     This method retrieves the excel sheet names from an excel workbook.
        /// </summary>
        /// <param name="excelFile">The excel file.</param>
        /// <returns>String[]</returns>
        public static String[] GetExcelTabNames(string excelFile)
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