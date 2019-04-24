using JosephM.Application.Desktop.Test;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Record.Xrm.Mappers;
using JosephM.Record.Xrm.Test;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.XrmModule.XrmConnection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.XrmModule.Test
{
    [TestClass]
    public class XrmConnectionModuleTests : XrmRecordTest
    {
        /// <summary>
        /// runs through 'connect to cr
        /// </summary>
        [TestMethod]
        public void XrmConnectionModuleTestEnterConnection()
        {
            var recordConnection = new XrmConfigurationMapper().Map(XrmConfiguration as XrmConfiguration);

            //clear all saved settings for test script app
            var testApplication = TestApplication.CreateTestApplication();
            var settingsFolder = testApplication.Controller.SettingsPath;
            FileUtility.DeleteFiles(settingsFolder);

            //create app with saved connections module
            testApplication = TestApplication.CreateTestApplication();
            testApplication.AddModule<SavedXrmConnectionsModule>();

            //navgiate to adding saved conneciton
            var module = testApplication.GetModule<XrmConnectionModule>();
            module.DialogCommand();
            var dialog = testApplication.GetNavigatedDialog<XrmConnectionDialog>();
            var connectionEntry = testApplication.GetSubObjectEntryViewModel(dialog);

            //verify autocompletes only display for those not reliant on existing values
            Assert.IsNotNull(connectionEntry.GetStringFieldFieldViewModel(nameof(XrmRecordConfiguration.DiscoveryServiceAddress)).AutocompleteViewModel);
            Assert.IsNull(connectionEntry.GetStringFieldFieldViewModel(nameof(XrmRecordConfiguration.Domain)).AutocompleteViewModel);
            Assert.IsNull(connectionEntry.GetStringFieldFieldViewModel(nameof(XrmRecordConfiguration.Username)).AutocompleteViewModel);
            Assert.IsNotNull(connectionEntry.GetStringFieldFieldViewModel(nameof(XrmRecordConfiguration.OrganizationUniqueName)).AutocompleteViewModel);

            //enter the connection
            connectionEntry.GetStringFieldFieldViewModel(nameof(XrmRecordConfiguration.Name)).Value = "Script Entry 1";
            connectionEntry.GetStringFieldFieldViewModel(nameof(XrmRecordConfiguration.DiscoveryServiceAddress)).SearchButton.Invoke();
            Assert.IsTrue(connectionEntry.GetStringFieldFieldViewModel(nameof(XrmRecordConfiguration.DiscoveryServiceAddress)).AutocompleteViewModel.DynamicGridViewModel.GridRecords.Any());
            connectionEntry.GetPicklistFieldFieldViewModel(nameof(XrmRecordConfiguration.AuthenticationProviderType)).Value = connectionEntry.GetPicklistFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.AuthenticationProviderType)).ItemsSource.First(p => p == PicklistOption.EnumToPicklistOption(recordConnection.AuthenticationProviderType));
            connectionEntry.GetStringFieldFieldViewModel(nameof(XrmRecordConfiguration.DiscoveryServiceAddress)).Value = recordConnection.DiscoveryServiceAddress;
            connectionEntry.GetStringFieldFieldViewModel(nameof(XrmRecordConfiguration.Domain)).Value = recordConnection.Domain;
            connectionEntry.GetStringFieldFieldViewModel(nameof(XrmRecordConfiguration.Username)).Value = recordConnection.Username;
            connectionEntry.GetFieldViewModel(nameof(XrmRecordConfiguration.Password)).ValueObject = recordConnection.Password;

            //including using autocomplete for the organisation unique name
            connectionEntry.GetStringFieldFieldViewModel(nameof(XrmRecordConfiguration.OrganizationUniqueName)).SearchButton.Invoke();
            Assert.IsTrue(connectionEntry.GetStringFieldFieldViewModel(nameof(XrmRecordConfiguration.OrganizationUniqueName)).AutocompleteViewModel.DynamicGridViewModel.GridRecords.Any());
            connectionEntry.GetStringFieldFieldViewModel(nameof(XrmRecordConfiguration.OrganizationUniqueName)).AutocompleteViewModel.DynamicGridViewModel.SelectedRow = connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.OrganizationUniqueName)).AutocompleteViewModel.DynamicGridViewModel.GridRecords.First(r => r.GetStringFieldFieldViewModel(nameof(Xrm.Organisation.UniqueName)).Value == recordConnection.OrganizationUniqueName);
            connectionEntry.GetStringFieldFieldViewModel(nameof(XrmRecordConfiguration.OrganizationUniqueName)).AutocompleteViewModel.SetToSelectedRow();
            Assert.AreEqual(recordConnection.OrganizationUniqueName, connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.OrganizationUniqueName)).Value);

            //save the connection and verify added to grid
            Assert.IsTrue(connectionEntry.Validate());
            connectionEntry.SaveButtonViewModel.Invoke();
        }
    }
}