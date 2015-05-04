using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Test;
using JosephM.Core.Utility;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.Application.Fakes;

namespace JosephM.Prism.Infrastructure.Test
{
    public class SettingsModuleTest<TSettingsModule, TDialog, TInterface, TClass> : PrismModuleTest<TSettingsModule>
        where TSettingsModule : SettingsModule<TDialog, TInterface, TClass>, new()
        where TDialog : AppSettingsDialog<TInterface, TClass>
        where TClass : TInterface, new()
    {
        public void ExecuteTest()
        {
            var fakeApplicationController = new FakeApplicationController();
            FileUtility.DeleteFiles(fakeApplicationController.SettingsPath);

            var dialog = Container.Resolve<TDialog>();
            var dialogController = dialog.Controller;
            dialogController.BeginDialog();

            var settingsFiles = FileUtility.GetFiles(fakeApplicationController.SettingsPath);
            Assert.AreEqual(1, settingsFiles.Count());
            var prismSettingsManager = new PrismSettingsManager(fakeApplicationController);
            var settings = prismSettingsManager.Resolve<TClass>();
            Assert.IsNotNull(settings);

            dialog = Container.Resolve<TDialog>();
            dialogController = dialog.Controller;
            dialogController.BeginDialog();

            settingsFiles = FileUtility.GetFiles(fakeApplicationController.SettingsPath);
            Assert.AreEqual(1, settingsFiles.Count());
            prismSettingsManager = new PrismSettingsManager(fakeApplicationController);
            settings = prismSettingsManager.Resolve<TClass>();
            Assert.IsNotNull(settings);
        }
    }
}