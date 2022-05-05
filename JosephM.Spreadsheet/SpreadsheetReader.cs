using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JosephM.Spreadsheet
{
    public class SpreadsheetReader
    {
        public static IDictionary<string, IEnumerable<string>> GetExcelColumnNames(string fileName)
        {
            var result = new Dictionary<string, IEnumerable<string>>();

            using (var spreadSheetDocument = SpreadsheetDocument.Open(fileName, false))
            {
                var indexedStringTable = IndexStringTable(spreadSheetDocument);
                var workbookPart = spreadSheetDocument.WorkbookPart;
                var sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();

                foreach (var sheet in sheets)
                {
                    if (sheet.Name.HasValue)
                    {
                        var columns = new List<string>();
                        result.Add(sheet.Name.Value, columns);

                        string relationshipId = sheet.Id.Value;
                        var worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relationshipId);
                        var workSheet = worksheetPart.Worksheet;
                        var sheetData = workSheet.GetFirstChild<SheetData>();
                        var rows = sheetData.Descendants<Row>();

                        if (rows.Any())
                        {
                            foreach (Cell cell in rows.ElementAt(0))
                            {
                                var cellValue = GetCellValue(indexedStringTable, cell);
                                if (!string.IsNullOrWhiteSpace(cellValue))
                                {
                                    columns.Add(cellValue);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        public static IEnumerable<string> GetTabNames(string fileName)
        {
            using (var spreadSheetDocument = SpreadsheetDocument.Open(fileName, false))
            {
                var workbookPart = spreadSheetDocument.WorkbookPart;
                var sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                return sheets.Where(s => s.Name.HasValue).Select(s => s.Name.Value).ToArray();
            }
        }

        public static DataTable ReadToDataTable(string fileName, string tabName)
        {
            var tabColumnNames = GetExcelColumnNames(fileName);
            if(!tabColumnNames.ContainsKey(tabName))
            {
                throw new NullReferenceException($"Columns for tab named '{tabName}' were not found in the spreadsheet");
            }
            var columnNames = tabColumnNames[tabName];
            var dictionaries = ReadToDictionaries(fileName, tabName);
            var dt = new DataTable();
            foreach (var columnName in columnNames)
            {
                dt.Columns.Add(columnName);
            }
            foreach(var row in dictionaries)
            {
                var newRow = dt.NewRow();
                foreach(var column in row)
                {
                    newRow[column.Key] = row[column.Key];
                }
                dt.Rows.Add(newRow);
            }
            return dt;
        }

        public static IEnumerable<IDictionary<string, object>> ReadToDictionaries(string fileName, string tabName)
        {
            var rowDictionaries = new List<IDictionary<string, object>>();

            using (var spreadSheetDocument = SpreadsheetDocument.Open(fileName, false))
            {
                var indexedStringTable = IndexStringTable(spreadSheetDocument);
                var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                var alphabetHash = new Hashtable();
                foreach(var chr in alphabet)
                {
                    alphabetHash.Add(chr, alphabet.IndexOf(chr));
                }

                var workbookPart = spreadSheetDocument.WorkbookPart;
                var sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();

                Sheet sheet = null;
                foreach (var sheetTemp in sheets)
                {
                    if (sheetTemp.Name?.Value == tabName)
                    {
                        sheet = sheetTemp;
                    }
                }

                if (sheet == null)
                {
                    throw new NullReferenceException("Could not find sheet with name " + tabName);
                }

                string relationshipId = sheet.Id.Value;
                var worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relationshipId);
                var workSheet = worksheetPart.Worksheet;
                var sheetData = workSheet.GetFirstChild<SheetData>();
                var rows = sheetData.Descendants<Row>().ToArray();

                var columnIndexes = new Dictionary<int, string>();
                var i = 0;
                foreach (Cell cell in rows.ElementAt(0))
                {
                    var cellValue = GetCellValue(indexedStringTable, cell);
                    if (!string.IsNullOrWhiteSpace(cellValue))
                    {
                        columnIndexes.Add(i, cellValue);
                    }
                    i++;
                }

                foreach (var row in rows) //this will also include your header row...
                {
                    var newRow = new ConcurrentDictionary<string, object>();
                    var rowHasValue = false;
                    var cells = row.Descendants<Cell>().ToArray();
                    Parallel.ForEach(cells, (cell) =>
                    {
                        if (cell.CellReference.HasValue)
                        {
                            var index = -1;
                            switch (cell.CellReference.Value[1])
                            {
                                case '0':
                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                case '8':
                                case '9':
                                    {
                                        index = (int)alphabetHash[cell.CellReference.Value[0]];
                                        break;
                                    }
                                default:
                                    {
                                        index =
                                        (((int)alphabetHash[cell.CellReference.Value[0]] + 1) * alphabetHash.Count)
                                        + (int)alphabetHash[cell.CellReference.Value[1]];
                                        break;
                                    }
                            }
                            if (columnIndexes.ContainsKey(index))
                            {
                                var cellValue = GetCellValue(indexedStringTable, cell);
                                newRow[columnIndexes[index]] = cellValue;
                                if (!string.IsNullOrWhiteSpace(cellValue))
                                {
                                    rowHasValue = true;
                                }
                            }
                        }
                    });

                    if (!rowHasValue)
                        break;

                    rowDictionaries.Add(newRow);
                }

            }
            rowDictionaries.RemoveAt(0); //...so i'm taking it out here.
            return rowDictionaries;
        }

        public static string GetCellValue(IDictionary stringTable, Cell cell)
        {
            string value = cell.CellValue?.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return (string)stringTable[int.Parse(value)];
            }
            else
            {
                return value;
            }
        }

        private static IDictionary IndexStringTable(SpreadsheetDocument document)
        {
            var i = 0;
            var stringTablePart = document.WorkbookPart.SharedStringTablePart;
            var dictionary = new Hashtable();
            foreach (var item in stringTablePart.SharedStringTable.ChildElements)
            {
                dictionary.Add(i++, item.InnerText);
            }
            return dictionary;
        }
    }
}
