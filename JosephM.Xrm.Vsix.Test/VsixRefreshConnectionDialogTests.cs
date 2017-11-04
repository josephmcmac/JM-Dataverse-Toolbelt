using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.ObjectMapping;
using JosephM.Record.Xrm.Mappers;
using JosephM.Xrm.Vsix.Module.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixRefreshConnectionDialogTests : JosephMVsixTests
    {
        [TestMethod]
        public void VsixRefreshConnectionDialogTest()
        {
            var xrmConfiguration = new InterfaceMapperFor<IXrmConfiguration,XrmConfiguration>().Map(XrmConfiguration);
            var xrmRecordConfiguration = new XrmConfigurationMapper().Map(xrmConfiguration);
            var dialog = new ConnectionEntryDialog(CreateDialogController(), xrmRecordConfiguration, VisualStudioService, true);
            dialog.Controller.BeginDialog();

            var entryViewModel = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            Assert.IsTrue(entryViewModel.Validate());
            entryViewModel.OnSave();
        }
    }
}
