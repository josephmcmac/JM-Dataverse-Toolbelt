using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Prism.Infrastructure.Test;
using JosephM.Xrm.RecordExtract.Test.RecordExtract;
using JosephM.Xrm.RecordExtract.TextSearch;

namespace JosephM.Xrm.RecordExtract.Test.TextSearch
{
    [TestClass]
    public class TestTextSearchSettingsModuleTests : SettingsModuleTest<TestTextSearchSettingsModule, TestTextSearchSettingsDialog, ITextSearchSettings, TextSearchSettings>
    {
        [TestMethod]
        public void TestTextSearchSettingsDialogTest()
        {
            ExecuteTest();
        }
    }
}