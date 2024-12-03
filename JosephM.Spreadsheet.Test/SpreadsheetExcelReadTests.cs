using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Spreadsheet.Test
{
    [TestClass]
    public class SpreadsheetExcelReadTests
    {
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
