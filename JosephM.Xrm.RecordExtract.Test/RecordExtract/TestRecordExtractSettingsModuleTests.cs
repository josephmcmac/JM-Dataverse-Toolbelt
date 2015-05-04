using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Utility;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Prism.Infrastructure.Test;
using JosephM.Record.Application.Fakes;
using JosephM.Xrm.RecordExtract.RecordExtract;

namespace JosephM.Xrm.RecordExtract.Test.RecordExtract
{
    [TestClass]
    public class TestRecordExtractSettingsModuleTests : SettingsModuleTest<TestRecordExtractSettingsModule, TestRecordExtractSettingsDialog, IRecordExtractSettings, RecordExtractSettings>
    {
        [TestMethod]
        public void TestRecordExtractSettingsDialogTest()
        {
            ExecuteTest();
        }
    }
}