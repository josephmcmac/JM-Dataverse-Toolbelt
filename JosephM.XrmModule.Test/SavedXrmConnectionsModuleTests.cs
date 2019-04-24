using JosephM.Application.Desktop.Test;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Record.Xrm.Mappers;
using JosephM.Record.Xrm.Test;
using JosephM.Xrm;
using JosephM.XrmModule.SavedXrmConnections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.XrmModule.Test
{
    [TestClass]
    public class SavedXrmConnectionsModuleTests : XrmRecordTest
    {
        /// <summary>
        /// runs through adding a saved connection
        /// </summary>
        [TestMethod]
        public void SavedXrmConnectionsModuleTestEnterSavedConnections()
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
            var dialog = testApplication.NavigateToDialog<SavedXrmConnectionsModule, SavedXrmConnectionsDialog>();
            var savedConnectionsEntryForm = testApplication.GetSubObjectEntryViewModel(dialog);

            //click to add a connection to the grid
            var connectionsGrid = savedConnectionsEntryForm.GetEnumerableFieldViewModel(nameof(SavedXrmConnections.SavedXrmConnections.Connections));
            connectionsGrid.DynamicGridViewModel.AddRowButton.Invoke();
            var connectionEntry = testApplication.GetSubObjectEntryViewModel(savedConnectionsEntryForm);

            //verify autocompletes only display for those not reliant on existing values
            Assert.IsNotNull(connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.DiscoveryServiceAddress)).AutocompleteViewModel);
            Assert.IsNull(connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.Domain)).AutocompleteViewModel);
            Assert.IsNull(connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.Username)).AutocompleteViewModel);
            Assert.IsNotNull(connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.OrganizationUniqueName)).AutocompleteViewModel);

            //enter the connection
            connectionEntry.GetBooleanFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.Active)).Value = true;
            connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.Name)).Value = "Script Entry 1";
            connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.DiscoveryServiceAddress)).SearchButton.Invoke();
            Assert.IsTrue(connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.DiscoveryServiceAddress)).AutocompleteViewModel.DynamicGridViewModel.GridRecords.Any());
            connectionEntry.GetPicklistFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.AuthenticationProviderType)).Value = connectionEntry.GetPicklistFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.AuthenticationProviderType)).ItemsSource.First(p => p == PicklistOption.EnumToPicklistOption(recordConnection.AuthenticationProviderType));
            connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.DiscoveryServiceAddress)).Value = recordConnection.DiscoveryServiceAddress;
            connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.Domain)).Value = recordConnection.Domain;
            connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.Username)).Value = recordConnection.Username;
            connectionEntry.GetFieldViewModel(nameof(SavedXrmRecordConfiguration.Password)).ValueObject = recordConnection.Password;

            //including using autocomplete for the organisation unique name
            connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.OrganizationUniqueName)).SearchButton.Invoke();
            Assert.IsTrue(connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.OrganizationUniqueName)).AutocompleteViewModel.DynamicGridViewModel.GridRecords.Any());
            connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.OrganizationUniqueName)).AutocompleteViewModel.DynamicGridViewModel.SelectedRow = connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.OrganizationUniqueName)).AutocompleteViewModel.DynamicGridViewModel.GridRecords.First(r => r.GetStringFieldFieldViewModel(nameof(Xrm.Organisation.UniqueName)).Value == recordConnection.OrganizationUniqueName);
            connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.OrganizationUniqueName)).AutocompleteViewModel.SetToSelectedRow();
            Assert.AreEqual(recordConnection.OrganizationUniqueName, connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.OrganizationUniqueName)).Value);

            //save the connection and verify added to grid
            Assert.IsTrue(connectionEntry.Validate());
            connectionEntry.SaveButtonViewModel.Invoke();
            Assert.IsFalse(savedConnectionsEntryForm.ChildForms.Any());
            connectionsGrid = savedConnectionsEntryForm.GetEnumerableFieldViewModel(nameof(SavedXrmConnections.SavedXrmConnections.Connections));
            Assert.AreEqual(1, connectionsGrid.GridRecords.Count);

            //save the connections and verify reloads same form with the connection
            Assert.IsTrue(savedConnectionsEntryForm.Validate());
            savedConnectionsEntryForm.SaveButtonViewModel.Invoke();

            savedConnectionsEntryForm = savedConnectionsEntryForm = testApplication.GetSubObjectEntryViewModel(dialog, index: 1);
            connectionsGrid = savedConnectionsEntryForm.GetEnumerableFieldViewModel(nameof(SavedXrmConnections.SavedXrmConnections.Connections));
            Assert.AreEqual(1, connectionsGrid.GridRecords.Count);

            //go to add another connection and verify username now has autocomplete due to existing value
            connectionsGrid.DynamicGridViewModel.AddRowButton.Invoke();
            connectionEntry = testApplication.GetSubObjectEntryViewModel(savedConnectionsEntryForm);
            Assert.IsNotNull(connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.DiscoveryServiceAddress)).AutocompleteViewModel);
            Assert.IsNotNull(connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.Username)).AutocompleteViewModel);
            Assert.IsNotNull(connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.OrganizationUniqueName)).AutocompleteViewModel);

        }
    }
}