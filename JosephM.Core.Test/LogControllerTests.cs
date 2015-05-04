using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Log;
using JosephM.Core.Utility;

namespace JosephM.Core.Test
{
    [TestClass]
    public class LogControllerTests : CoreTest
    {
        [TestMethod]
        public void LogControllerTest()
        {
            const int logsPerFile = 5;
            var testFolder = TestingFolder + @"\TextLogTests";
            FileUtility.DeleteFiles(testFolder);
            var logConfiguration = new LogConfiguration()
            {
                Log = true,
                LogDetail = true,
                LogFilePath = testFolder,
                LogsPerFile = logsPerFile
            };

            var controller = new LogController(logConfiguration, "LogTest");

            const int todo = 5;
            for (var i = 0; i < 5; i++)
            {
                controller.LogDetail("Detail " + i);
                controller.LogLiteral("Message " + i);
                controller.UpdateProgress(i, todo, "Progress");
            }
            const int expectedCount = ((todo*3)/logsPerFile) + (((todo*3)%logsPerFile) > 0 ? 1 : 0);
            var files = FileUtility.GetFiles(testFolder);
            Assert.AreEqual(expectedCount, files.Count());
        }
    }
}