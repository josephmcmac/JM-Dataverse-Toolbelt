using JosephM.Application.Desktop.Application;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Core.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.Application.Desktop.Test
{
    [TestClass]
    public class DesktopSettingsManagerTests
    {
        [TestMethod]
        public void DesktopSettingsManagerTest()
        {
            var fakeApplicationController = new FakeApplicationController();
            FileUtility.DeleteFiles(fakeApplicationController.SettingsPath);
            FileUtility.DeleteSubFolders(fakeApplicationController.SettingsPath);

            var settingsManager = new DesktopSettingsManager(fakeApplicationController);

            var resolveNotYetCreated = settingsManager.Resolve<TestResolveType>();
            Assert.IsNotNull(resolveNotYetCreated);
            Assert.AreEqual(0, resolveNotYetCreated.Int);

            resolveNotYetCreated.Int = 50;
            settingsManager.SaveSettingsObject(resolveNotYetCreated);

            var resolveAfterCreation = settingsManager.Resolve<TestResolveType>();
            Assert.AreEqual(resolveNotYetCreated.Int, resolveAfterCreation.Int);
        }

        public class TestResolveType
        {
            public int Int { get; set; }
        }
    }
}