using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Utility;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.XRM.VSIX;
using JosephM.XRM.VSIX.Commands.PackageSettings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixRefreshSettingsDialogTests : JosephMVsixTests
    {
        [TestMethod]
        public void VsixRefreshSettingsDialogTest()
        {
            var fakeVisualStudioService = CreateVisualStudioService();

            var packageSettinns = GetTestPackageSettings();

            var dialog = new XrmPackageSettingDialog(CreateDialogController(), packageSettinns, fakeVisualStudioService, true, null);
            dialog.Controller.BeginDialog();

            SubmitEntryForm(dialog);

            packageSettinns = GetTestPackageSettings();

            dialog = new XrmPackageSettingDialog(CreateDialogController(), packageSettinns, fakeVisualStudioService, true, XrmRecordService);
            dialog.Controller.BeginDialog();

            SubmitEntryForm(dialog);
        }
    }
}
