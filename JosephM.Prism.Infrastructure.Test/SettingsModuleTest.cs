using System.Linq;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Core.AppConfig;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Test;
using JosephM.Core.Utility;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.Application.Fakes;

namespace JosephM.Prism.Infrastructure.Test
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

            var testApplication = new TestApplication();
            testApplication.Controller.RegisterType<IDialogController, AutoDialogController>();
            testApplication.AddModule<TSettingsModule>();
            var module = testApplication.GetModule<TSettingsModule>();
            module.DialogCommand();
            var dialog = testApplication.GetNavigatedDialog<TDialog>();

            var settingsFiles = FileUtility.GetFiles(fakeApplicationController.SettingsPath);
            Assert.AreEqual(1, settingsFiles.Count());
            var prismSettingsManager = new PrismSettingsManager(fakeApplicationController);
            var settings = prismSettingsManager.Resolve<TClass>();
            Assert.IsNotNull(settings);

            module.DialogCommand();
            dialog = testApplication.GetNavigatedDialog<TDialog>();

            settingsFiles = FileUtility.GetFiles(fakeApplicationController.SettingsPath);
            Assert.AreEqual(1, settingsFiles.Count());
            prismSettingsManager = new PrismSettingsManager(fakeApplicationController);
            settings = prismSettingsManager.Resolve<TClass>();
            Assert.IsNotNull(settings);
        }
    }
}