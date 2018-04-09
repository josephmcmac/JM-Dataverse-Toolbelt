using JosephM.Application.Application;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Core.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.Prism.Infrastructure.Test
{
    [TestClass]
    public class PrismSettingsManagerTests
    {
        [TestMethod]
        public void PrismSettingsManagerTest()
        {
            var fakeApplicationController = new FakeApplicationController();
            FileUtility.DeleteFiles(fakeApplicationController.SettingsPath);
            FileUtility.DeleteSubFolders(fakeApplicationController.SettingsPath);

            var prismSettingsManager = new PrismSettingsManager(fakeApplicationController);

            var resolveNotYetCreated = prismSettingsManager.Resolve<TestResolveType>();
            Assert.IsNotNull(resolveNotYetCreated);
            Assert.AreEqual(0, resolveNotYetCreated.Int);

            resolveNotYetCreated.Int = 50;
            prismSettingsManager.SaveSettingsObject(resolveNotYetCreated);

            var resolveAfterCreation = prismSettingsManager.Resolve<TestResolveType>();
            Assert.AreEqual(resolveNotYetCreated.Int, resolveAfterCreation.Int);
        }

        public class TestResolveType
        {
            public int Int { get; set; }
        }
    }
}