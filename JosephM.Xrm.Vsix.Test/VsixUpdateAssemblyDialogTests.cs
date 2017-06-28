using JosephM.Application.ViewModel.Fakes;
using JosephM.XRM.VSIX.Commands.UpdateAssembly;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixUpdateAssemblyDialogTests : JosephMVsixTests
    {
        [TestMethod]
        public void VsixUpdateAssemblyDialogTest()
        {
            var packageSettings = GetPackageSettingsAddToSolution();
            packageSettings.AddToSolution = false;
            DeployAssembly(packageSettings);
            packageSettings.AddToSolution = true;
            var dialog = new UpdateAssemblyDialog(new FakeDialogController(new FakeApplicationController()),
                GetTestPluginAssemblyFile(), XrmRecordService, packageSettings);
            dialog.Controller.BeginDialog();

        }
    }
}
