using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Utility;
using JosephM.Prism.Infrastructure.Test;
using JosephM.Xrm.RecordExtract.RecordExtract;
using JosephM.Xrm.RecordExtract.Test.TextSearch;
using JosephM.Xrm.RecordExtract.TextSearch;

namespace JosephM.Xrm.RecordExtract.Test.RecordExtract
{
    [TestClass]
    public class TestRecordExtractModuleTests : ServiceRequestModuleTest<TestRecordExtractModule, TestRecordExtractDialog, TestRecordExtractService, RecordExtractRequest, RecordExtractResponse, RecordExtractResponseItem>
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
            ExecuteTest();
        }
    }
}