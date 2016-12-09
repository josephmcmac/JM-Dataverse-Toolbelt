using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Spreadsheet.Test
{
    [TestClass]
    public class SpreadsheetExcelReadTests
    {
        [DeploymentItem("TestXls.xls")]
        [TestMethod]
        public void SpreadsheetExcelReadXlsTest()
        {
            var fileName = "TestXls.xls";
            var tabs = ExcelUtility.GetExcelTabNames(fileName);
            var rows = ExcelUtility.SelectPropertyBagsFromExcelTabName(fileName, "Fields");
            var rows2 = ExcelUtility.SelectPropertyBagsFromExcelTabName(fileName, "Option Sets");

            var fieldName = rows.First().GetFieldAsString("Schema Name");
        }

        [DeploymentItem("TestXlsx.xlsx")]
        [TestMethod]
        public void SpreadsheetExcelReadXlsxTest()
        {
            var fileName = "TestXlsx.xlsx";
            var tabs = ExcelUtility.GetExcelTabNames(fileName);
            var rows = ExcelUtility.SelectPropertyBagsFromExcelTabName(fileName, "Fields");
            var rows2 = ExcelUtility.SelectPropertyBagsFromExcelTabName(fileName, "Option Sets");

            var fieldName = rows.First().GetFieldAsString("Schema Name");
        }
    }
}
