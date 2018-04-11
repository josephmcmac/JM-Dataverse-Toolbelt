using JosephM.Application.Desktop.Test;
using JosephM.Core.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Xrm.RecordExtract.Test.RecordExtract
{
    [TestClass]
    public class TestRecordExtractModuleTests : DialogModuleTest<TestRecordExtractModule, TestRecordExtractDialog>
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
        public void TestRecordExtractModuleTest()
        {
            ExecuteAutoEntryTest();
        }
    }
}