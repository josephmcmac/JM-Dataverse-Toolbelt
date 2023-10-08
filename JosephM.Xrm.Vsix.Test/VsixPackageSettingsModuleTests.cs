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

            //enter the connection
            connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.Name)).Value = "Script Entry 1";
            connectionEntry.GetPicklistFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.ConnectionType)).Value = connectionEntry.GetPicklistFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.ConnectionType)).ItemsSource.First(p => p == PicklistOption.EnumToPicklistOption(recordConnection.ConnectionType));
            connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.ClientId)).Value = recordConnection.ClientId;
            connectionEntry.GetFieldViewModel(nameof(SavedXrmRecordConfiguration.ClientSecret)).ValueObject = recordConnection.ClientSecret;
            connectionEntry.GetFieldViewModel(nameof(SavedXrmRecordConfiguration.WebUrl)).ValueObject = recordConnection.WebUrl;

            //save the connection and verify added to grid
            Assert.IsTrue(connectionEntry.Validate());
            connectionEntry.SaveButtonViewModel.Invoke();

            var packageSettingsEntryForm = testApplication.GetSubObjectEntryViewModel(dialog, index: 1) as ObjectEntryViewModel;
            var connectionsGrid = packageSettingsEntryForm.GetEnumerableFieldViewModel(nameof(XrmPackageSettings.Connections));
            Assert.AreEqual(1, connectionsGrid.GridRecords.Count);
        }
    }
}
