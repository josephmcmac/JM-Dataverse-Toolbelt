using JosephM.Application.Desktop.Test;
using JosephM.Core.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Xrm.RecordExtract.Test.TextSearch
{
    [TestClass]
    public class TestTextSearchModuleTests : DialogModuleTest<TestTextSearchModule, TestTextSearchDialog>
    {
        protected override void PrepareTest()
        {
            FileUtility.DeleteFiles(TestingFolder);
        }

        protected override void CompleteTest()
        {
            Assert.AreEqual(1, FileUtility.GetFiles(TestingFolder).Count());
        }

        [TestMethod]
        public void TestTextSearchModuleTest()
        {
            ExecuteAutoEntryTest();
        }
    }
}