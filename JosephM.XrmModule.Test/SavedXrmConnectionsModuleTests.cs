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

            //enter the connection
            connectionEntry.GetBooleanFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.Active)).Value = true;
            connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.Name)).Value = "Script Entry 1";
            connectionEntry.GetPicklistFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.ConnectionType)).Value = connectionEntry.GetPicklistFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.ConnectionType)).ItemsSource.First(p => p == PicklistOption.EnumToPicklistOption(recordConnection.ConnectionType));
            connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.ClientId)).Value = recordConnection.ClientId; connectionEntry.GetFieldViewModel(nameof(SavedXrmRecordConfiguration.ClientSecret)).ValueObject = recordConnection.ClientSecret;
            connectionEntry.GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.WebUrl)).Value = recordConnection.WebUrl;

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
        }
    }
}