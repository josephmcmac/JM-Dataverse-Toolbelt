using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.IO;

namespace JosephM.Spreadsheet.Test
{
    [TestClass]
    public class SpreadsheetExcelReadTests
    {
        [TestMethod]
        public void SpreadsheetExcelReadXlsDebug()
        {
            var rows = ExcelUtility.SelectPropertyBagsFromExcelTab("C:\\Temp\\System Forms - TEST.xlsx", "Records");

            foreach(var row in rows)
            {
                File.WriteAllText($"C:\\Temp\\FormXmlFiles\\{row.GetFieldAsString("Entity Name")} - {row.GetFieldAsString("Name")}.xml", row.GetFieldAsString("formxml"));
            }
        }

        [DeploymentItem("TestXlsx.xlsx")]
        [TestMethod]
        public void SpreadsheetExcelReadXlsxTest()
        {
            var fileName = "TestXlsx.xlsx";
            var tabs = ExcelUtility.GetExcelTabNames(fileName);
            var rows = ExcelUtility.SelectPropertyBagsFromExcelTab(fileName, "Fields");
            var rows2 = ExcelUtility.SelectPropertyBagsFromExcelTab(fileName, "Option Sets");

            var fieldName = rows.First().GetFieldAsString("Schema Name");
        }
    }
}
