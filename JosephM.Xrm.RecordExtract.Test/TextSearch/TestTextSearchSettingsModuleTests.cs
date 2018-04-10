using JosephM.Application.Prism.Test;
using JosephM.Xrm.RecordExtract.TextSearch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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