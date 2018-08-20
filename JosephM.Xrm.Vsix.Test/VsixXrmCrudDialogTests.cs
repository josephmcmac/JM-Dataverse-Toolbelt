using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.XrmModule.Crud;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.AppConfig;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.Application.ViewModel.Query;
using System.Linq;
using JosephM.Application.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;

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
            var xrmRecordService = app.Controller.ResolveType<XrmRecordService>();
            //okay this is the service which will get resolve by the dialog - so lets clear out its connection details
            //then the dialog should redirect to entry
            var originalConnection = xrmRecordService.XrmRecordConfiguration;
            xrmRecordService.XrmRecordConfiguration = new XrmRecordConfiguration();

            var dialog = app.NavigateToDialog<XrmCrudModule, XrmCrudDialog>();

            //okay we should have been directed to a connection entry

            var connectionEntryViewModel = dialog.Controller.UiItems[0] as ObjectEntryViewModel;
            var newConnection = connectionEntryViewModel.GetObject() as SavedXrmRecordConfiguration;
            newConnection.AuthenticationProviderType = originalConnection.AuthenticationProviderType;
            newConnection.DiscoveryServiceAddress = originalConnection.DiscoveryServiceAddress;
            newConnection.OrganizationUniqueName = originalConnection.OrganizationUniqueName;
            newConnection.Domain = originalConnection.Domain;
            newConnection.Username = originalConnection.Username;
            newConnection.Password = originalConnection.Password;
            newConnection.Name = "RedirectScriptEntered";
            connectionEntryViewModel.SaveButtonViewModel.Invoke();

            //cool if has worked then now we will be at the query view model with the connection
            var queryViewModel = dialog.Controller.UiItems[0] as QueryViewModel;
            Assert.IsNotNull(queryViewModel);
            //lets just verify the connections were saved as well
            var savedConnections = app.Controller.ResolveType<ISavedXrmConnections>();
            Assert.IsTrue(savedConnections.Connections.Any(c => c.Name == "RedirectScriptEntered"));
            var appXrmRecordService = app.Controller.ResolveType<XrmRecordService>();
            Assert.IsTrue(appXrmRecordService.XrmRecordConfiguration.ToString() == "RedirectScriptEntered");
            var appXrmRecordConnection = app.Controller.ResolveType<IXrmRecordConfiguration>();
            Assert.IsTrue(appXrmRecordConnection.ToString() == "RedirectScriptEntered");
            var savedSetingsManager = app.Controller.ResolveType<ISettingsManager>();
            var savedXrmRecordService = savedSetingsManager.Resolve<XrmPackageSettings>();
            Assert.IsTrue(appXrmRecordService.XrmRecordConfiguration.ToString() == "RedirectScriptEntered");
            var savedXrmRecordConnection = savedSetingsManager.Resolve<XrmRecordConfiguration>();
        }
    }
}
