using JosephM.Application.Application;
using JosephM.Application.Desktop.Application;
using JosephM.Application.Desktop.Module.Settings;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Core.Test;
using JosephM.Core.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Application.Desktop.Test
{
    public class SettingsModuleTest<TSettingsModule, TDialog, TInterface, TClass> : CoreTest
        where TSettingsModule : SettingsModule<TDialog, TInterface, TClass>, new()
        where TDialog : AppSettingsDialog<TInterface, TClass>
        where TClass : TInterface, new()
    {
        public void ExecuteTest()
        {
            var fakeApplicationController = new FakeApplicationController();
            FileUtility.DeleteFiles(fakeApplicationController.SettingsPath);

            var testApplication = TestApplication.CreateTestApplication();
            testApplication.Controller.RegisterType<IDialogController, AutoDialogController>();
            testApplication.AddModule<TSettingsModule>();
            var module = testApplication.GetModule<TSettingsModule>();
            module.DialogCommand();
            var dialog = testApplication.GetNavigatedDialog<TDialog>();

            var settingsFiles = FileUtility.GetFiles(fakeApplicationController.SettingsPath);
            Assert.AreEqual(1, settingsFiles.Count());
            var settingsManager = new DesktopSettingsManager(fakeApplicationController);
            var settings = settingsManager.Resolve<TClass>();
            Assert.IsNotNull(settings);

            module.DialogCommand();
            dialog = testApplication.GetNavigatedDialog<TDialog>();

            settingsFiles = FileUtility.GetFiles(fakeApplicationController.SettingsPath);
            Assert.AreEqual(1, settingsFiles.Count());
            settingsManager = new DesktopSettingsManager(fakeApplicationController);
            settings = settingsManager.Resolve<TClass>();
            Assert.IsNotNull(settings);
        }
    }
}