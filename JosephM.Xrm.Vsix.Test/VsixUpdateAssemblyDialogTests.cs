using JosephM.Application.ViewModel.Fakes;
using JosephM.Xrm.Vsix.Module.UpdateAssembly;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixUpdateAssemblyDialogTests : JosephMVsixTests
    {
        [TestMethod]
        public void VsixUpdateAssemblyDialogTest()
        {
            var packageSettings = GetTestPackageSettings();
            packageSettings.AddToSolution = false;
            DeployAssembly(packageSettings);
            packageSettings.AddToSolution = true;
            var dialog = new UpdateAssemblyDialog(new FakeDialogController(new FakeApplicationController()),
                new FakeVisualStudioService(), XrmRecordService, packageSettings);
            dialog.Controller.BeginDialog();

        }
    }
}
