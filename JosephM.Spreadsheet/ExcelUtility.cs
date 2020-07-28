using JosephM.Core.Extentions;
using JosephM.Core.Sql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace JosephM.Spreadsheet
{
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

                    //if (propertyType == typeof(DateTime))
                    //    cellTypes.Add(property, CellDataType.Date);
                    //else
                    if (propertyType == typeof(int) || propertyType == typeof(decimal) || propertyType == typeof(double) || propertyType == typeof(float) || propertyType == typeof(long))
                        cellTypes.Add(property, CellDataType.Number);
                    else
                        cellTypes.Add(property, CellDataType.String);
                }

                getCellTypes.Add(sheet.Key, (s) => cellTypes[s]);
            }

            CreateXlsx(path, name, sheets, propertyNames, getFields, getLabels, getCellTypes);
        }

        public static IEnumerable<QueryRow> SelectPropertyBagsFromExcelTab(string fileName, string tabName)
        {
            var dTable = SpreadsheetReader.Read(fileName, tabName);
            var itemsToAdd = new List<DataRow>();
            foreach (DataRow row in dTable.Rows)
            {
                itemsToAdd.Add(row);
            }
            return itemsToAdd.Select(r => new QueryRow(r)).ToArray();
        }

        public static IDictionary<string, IEnumerable<string>> GetExcelColumnNames(string excelFile)
        {
            return SpreadsheetReader.GetExcelColumnNames(excelFile);
        }

        public static IEnumerable<string> GetExcelTabNames(string excelFile)
        {
            return SpreadsheetReader.GetTabNames(excelFile);
        }
    }
}