using JosephM.Application.ViewModel.Query;
using JosephM.XrmModule.Crud;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixXrmCrudDialogTests : JosephMVsixTests
    {
        [TestMethod]
        public void VsixXrmCrudDialogTest()
        {
            //eh so lets run a browse dialog and veirfy it redirect to the connection entry

            var app = CreateAndLoadTestApplication<XrmCrudModule>();

            //okay adding this here because I added a redirect to connection entry if none is entered
            var originalConnection = HijackForPackageEntryRedirect(app);
            //run the dialog
            var dialog = app.NavigateToDialog<XrmCrudModule, XrmCrudDialog>();

            VerifyPackageEntryRedirect(originalConnection, dialog);

            //cool if has worked then now we will be at the query view model with the connection
            var queryViewModel = dialog.Controller.UiItems[0] as QueryViewModel;
            Assert.IsNotNull(queryViewModel);
        }
    }
}
