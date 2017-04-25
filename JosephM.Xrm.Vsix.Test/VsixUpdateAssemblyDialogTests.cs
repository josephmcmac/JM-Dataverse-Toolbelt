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
            DeployAssembly(packageSettings);

            var dialog = new UpdateAssemblyDialog(new FakeDialogController(new FakeApplicationController()),
                GetTestPluginAssemblyFile(), XrmRecordService);
            dialog.Controller.BeginDialog();

        }
    }
}
