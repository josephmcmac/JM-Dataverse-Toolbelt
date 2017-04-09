using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.ObjectMapping;
using JosephM.Record.Query;
using JosephM.Record.Xrm.Mappers;
using JosephM.XRM.VSIX.Commands.DeployAssembly;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Record.Xrm.Test;
using JosephM.Xrm.Test;
using JosephM.XRM.VSIX;
using JosephM.XRM.VSIX.Commands.PackageSettings;
using JosephM.XRM.VSIX.Commands.RefreshConnection;
using JosephM.XRM.VSIX.Commands.UpdateAssembly;
using Fields = JosephM.Xrm.Schema.Fields;
using Entities = JosephM.Xrm.Schema.Entities;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixRefreshSettingsDialogTests : JosephMVsixTests
    {
        [TestMethod]
        public void VsixRefreshSettingsDialogTest()
        {
            var fakeVisualStudioService = CreateVisualStudioService();

            var packageSettinns = new XrmPackageSettings();
            PopulateObject(packageSettinns);

            var dialog = new XrmPackageSettingDialog(CreateDialogController(), packageSettinns, fakeVisualStudioService, true, null);
            dialog.Controller.BeginDialog();

            var entryViewModel = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.OnSave();

            packageSettinns = new XrmPackageSettings();
            PopulateObject(packageSettinns);

            dialog = new XrmPackageSettingDialog(CreateDialogController(), packageSettinns, fakeVisualStudioService, true, XrmRecordService);
            dialog.Controller.BeginDialog();

            entryViewModel = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.OnSave();
        }
    }
}
