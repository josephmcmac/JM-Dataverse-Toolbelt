using JosephM.Core.Extentions;
using JosephM.Core.Sql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;

namespace JosephM.Spreadsheet
{
    /// <summary>
    ///     Provides several methods for reading data into Datatable objects from an Excel file
    /// </summary>
    public static class ExcelUtility
    {
        private static string GetXlsxFileName(string name)
        {
            return name.ToLower().EndsWith(".xlsx") ? name : name + ".xlsx";
        }

        public static void CreateXlsx(string path, string name, IDictionary<string, IEnumerable> sheets, Dictionary<string, IEnumerable<string>> propertyNames, Dictionary<string, Func<object, string, object>> getFields, Dictionary<string, Func<string, string>> getLabels, Dictionary<string, Func<string, CellDataType>> getCellTypes = null)
        {
            name = GetXlsxFileName(name);

            var workSheets = new List<WorksheetDfn>();

            foreach (var sheet in sheets)
            {
                var thesePropertyNames = propertyNames != null && propertyNames.ContainsKey(sheet.Key) ? propertyNames[sheet.Key] : throw new NullReferenceException("No Property Names For Sheet " + sheet.Key);
                var getField = getFields != null && getFields.ContainsKey(sheet.Key) ? getFields[sheet.Key] : throw new NullReferenceException("No Get Field Method For Sheet " + sheet.Key);
                var getLabel = getLabels != null && getLabels.ContainsKey(sheet.Key) ? getLabels[sheet.Key] : (s) => s;
                var getCellType = getCellTypes != null && getCellTypes.ContainsKey(sheet.Key) ? getCellTypes[sheet.Key] : (s) => CellDataType.String;

                var workSheet = new WorksheetDfn();
                workSheet.Name = sheet.Key.Replace(" ", "_");
                workSheet.TableName = sheet.Key.Replace(" ", "_");
                workSheet.ColumnHeadings = thesePropertyNames.Select(p => new CellDfn()
                {
                    Bold = true,
                    Value = getLabel(p)
                }).ToArray();

                var rows = new List<RowDfn>();
                foreach (var e in sheet.Value)
                {
                    rows.Add(new RowDfn
                    {
                        Cells = thesePropertyNames.Select(p => new CellDfn
                        {
                            Value = getField(e, p),
                            CellDataType = getCellType(p)
                        }).ToArray()
                    });
                }
                workSheet.Rows = rows.ToArray();
                workSheets.Add(workSheet);
            }

            var wb = new WorkbookDfn();
            wb.Worksheets = workSheets.ToArray();
            var outXlsx = new FileInfo(Path.Combine(path, name));
            SpreadsheetWriter.Write(outXlsx.FullName, wb);
        }

        public static void CreateXlsx(string path, string name, IDictionary<string, IEnumerable> sheets)
        {
            var propertyNames = new Dictionary<string, IEnumerable<string>>();
            var getLabels = new Dictionary<string, Func<string, string>>();
            var getFields = new Dictionary<string, Func<object, string, object>>();
            var getCellTypes = new Dictionary<string, Func<string, CellDataType>>();

            foreach (var sheet in sheets)
            {
                var typeToOutput = sheet.Value.GetType().GenericTypeArguments[0];
                propertyNames.Add(sheet.Key, typeToOutput.GetReadableProperties().Select(s => s.Name).ToArray());
                getLabels.Add(sheet.Key, (s) => typeToOutput.GetProperty(s).GetDisplayName());
                getFields.Add(sheet.Key, (o, s) => o.GetPropertyValue(s)?.ToString());
                Func<string, string> getLabel = (s) => typeToOutput.GetProperty(s).GetDisplayName();
                Func<object, string, object> getField = (o, s) => o.GetPropertyValue(s)?.ToString();
                
                
                var cellTypes = new Dictionary<string, CellDataType>();
                foreach (var property in propertyNames[sheet.Key])
                {
                    var propertyType = typeToOutput.GetProperty(property).PropertyType;
                    if (propertyType.Name == "Nullable`1")
                        propertyType = propertyType.GetGenericArguments()[0];

                    if (propertyType == typeof(DateTime))
                        cellTypes.Add(property, CellDataType.Date);
                    else if (propertyType == typeof(int) || propertyType == typeof(decimal) || propertyType == typeof(double) || propertyType == typeof(float) || propertyType == typeof(long))
                        cellTypes.Add(property, CellDataType.Number);
                    else
                        cellTypes.Add(property, CellDataType.String);
                }

                getCellTypes.Add(sheet.Key, (s) => cellTypes[s]);
            }

            CreateXlsx(path, name, sheets, propertyNames, getFields, getLabels, getCellTypes);
        }

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