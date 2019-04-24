using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;
using JosephM.Record.Xrm.Mappers;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.SavedXrmConnections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixPackageSettingsModuleTests : JosephMVsixTests
    {
        /// <summary>
        /// Verifies redirected entry of new connection for package settings when there isnt one yet
        /// </summary>
        [TestMethod]
        public void VsixPackageSettingsModuleTestEnterPackageSettings()
        {
            var recordConnection = new XrmConfigurationMapper().Map(XrmConfiguration as XrmConfiguration);

            InitialiseModuleXrmConnection = false;
            var testApplication = CreateAndLoadTestApplication<XrmPackageSettingsModule>(loadXrmConnection: false);

            //navigate to package settings will initally load connection entry as we dont yet have one
            var dialog = testApplication.NavigateToDialog<XrmPackageSettingsModule, XrmPackageSettingsDialog>();
            var connectionEntry = dialog.Controller.UiItems.First() as ObjectEntryViewModel;

            //verify autocompletes only display for those not reliant on existing values
            Assert.IsNotNull(connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.DiscoveryServiceAddress)).AutocompleteViewModel);
            Assert.IsNull(connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.Domain)).AutocompleteViewModel);
            Assert.IsNull(connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.Username)).AutocompleteViewModel);
            Assert.IsNotNull(connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.OrganizationUniqueName)).AutocompleteViewModel);

            //enter the connection
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

            var packageSettingsEntryForm = testApplication.GetSubObjectEntryViewModel(dialog, index: 1) as ObjectEntryViewModel;
            var connectionsGrid = packageSettingsEntryForm.GetEnumerableFieldViewModel(nameof(XrmPackageSettings.Connections));
            Assert.AreEqual(1, connectionsGrid.GridRecords.Count);

            //go to add another connection and verify username now has autocomplete due to existing value
            connectionsGrid.DynamicGridViewModel.AddRowButton.Invoke();
            connectionEntry = testApplication.GetSubObjectEntryViewModel(packageSettingsEntryForm);
            Assert.IsNotNull(connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.DiscoveryServiceAddress)).AutocompleteViewModel);
            Assert.IsNotNull(connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.Username)).AutocompleteViewModel);
            Assert.IsNotNull(connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.OrganizationUniqueName)).AutocompleteViewModel);

        }
    }
}
