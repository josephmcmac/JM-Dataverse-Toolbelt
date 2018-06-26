using JosephM.Application.ViewModel.Fakes;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Module.UpdateAssembly;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixUpdateAssemblyDialogTests : JosephMVsixTests
    {
        [TestMethod]
        public void VsixUpdateAssemblyDialogTest()
        {
            //lets run the update assembly dialog
            //does not actually update the assembly because the assembly is unchanged

            var packageSettings = GetTestPackageSettings();
            packageSettings.AddToSolution = false;

            //first deploy the assembly
            DeployAssembly(packageSettings);

            //update the assembly
            packageSettings.AddToSolution = true;
            var dialog = new UpdateAssemblyDialog(new FakeDialogController(new FakeApplicationController()),
                new FakeVisualStudioService(), XrmRecordService, packageSettings);
            dialog.Controller.BeginDialog();
        }
    }
}
