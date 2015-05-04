using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Utility;
using JosephM.Prism.Infrastructure.Test;
using JosephM.Xrm.RecordExtract.TextSearch;

namespace JosephM.Xrm.RecordExtract.Test.TextSearch
{
    [TestClass]
    public class TestTextSearchModuleTests : ServiceRequestModuleTest<TestTextSearchModule, TestTextSearchDialog, TestTextSearchService, TextSearchRequest, TextSearchResponse, TextSearchResponseItem>
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
            ExecuteTest();
        }
    }
}