using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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

        public static DataTable Read(string fileName, string tabName)
        {
            var dt = new DataTable();

            using (var spreadSheetDocument = SpreadsheetDocument.Open(fileName, false))
            {
                var indexedStringTable = IndexStringTable(spreadSheetDocument);
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
                var rows = sheetData.Descendants<Row>();

                foreach (Cell cell in rows.ElementAt(0))
                {
                    var cellValue = GetCellValue(indexedStringTable, cell);
                    if (!string.IsNullOrWhiteSpace(cellValue))
                    {
                        dt.Columns.Add(cellValue);
                    }
                }

                foreach (var row in rows.ToArray()) //this will also include your header row...
                {
                    var newRow = dt.NewRow();

                    var cells = row.Descendants<Cell>().ToArray();
                    var cellCount = cells.Count();
                    var i = 0;
                    foreach (var cell in cells)
                    {
                        if (cell.CellReference.HasValue)
                        {
                            var column = new string(cell.CellReference.Value.Where(c => !char.IsDigit(c)).ToArray()).ToUpper();
                            var index = -1;
                            if(column.Length == 1)
                            {
                                index = _alphabet.IndexOf(column[0]);
                            }
                            else
                            {
                                index =
                                    ((_alphabet.IndexOf(column[0]) + 1) * _alphabet.Length)
                                    + _alphabet.IndexOf(column[1]);
                            }
                            if (index > -1 && index < newRow.ItemArray.Count())
                            {
                                newRow[index] = GetCellValue(indexedStringTable, cell);
                            }
                        }
                    }

                    if (newRow.ItemArray.All(c => c is null || c is DBNull))
                        break;

                    dt.Rows.Add(newRow);
                }

            }
            dt.Rows.RemoveAt(0); //...so i'm taking it out here.
            return dt;
        }

        private static string _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string GetCellValue(IDictionary<int, string> stringTable, Cell cell)
        {

            string value =  cell.CellValue?.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTable[int.Parse(value)];
            }
            else
            {
                return value;
            }
        }

        private static IDictionary<int, string> IndexStringTable(SpreadsheetDocument document)
        {
            var i = 0;
            var stringTablePart = document.WorkbookPart.SharedStringTablePart;
            var dictionary = new SortedDictionary<int, string>();
            foreach (var item in stringTablePart.SharedStringTable.ChildElements)
            {
                dictionary.Add(i++, item.InnerText);
            }
            return dictionary;
        }
    }
}
