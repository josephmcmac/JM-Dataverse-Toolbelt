using JosephM.Application.Desktop.Test;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.Xrm.Vsix.Wizards;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using JosephM.Xrm.Vsix.App;
using JosephM.Core.Extentions;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixSolutionWizardTests : JosephMVsixTests
    {
        [TestMethod]
        public void VsixSolutionWizardTest()
        {
            //okay so this wizard contains a dialog spawned different to the others
            //since it is loaded prior to the solution being created
            //this script just verifies the settings are netered into the dialog wihtout a crash with no visual studio solutoion context etc.


            var packageSettings = new XrmPackageSettings();

            var container = new VsixDependencyContainer();

            //purpose of this is to load modules registrations into the container as per done in the actual wizard
            Factory.CreateJosephMXrmVsixApp(new FakeVisualStudioService(), container, isNonSolutionExplorerContext: true);

            //okay spawn the wizards entry method with a fake controller
            var applicationController = new FakeVsixApplicationController(container);
            XrmSolutionWizardBase.RunWizardSettingsEntry(packageSettings, applicationController, "Fake.Name");

            //okay so now we have navigated to the package entry
            //in our application controller via the wizard 

            //here I inject that application conctroller into a fake application
            //so that I can use methods for auto testing the dialog which has spawned
            var fakeVsixApplication = TestApplication.CreateTestApplication(applicationController);
            fakeVsixApplication.AddModule<XrmPackageSettingsModule>();
            var dialog = fakeVsixApplication.GetNavigatedDialog<XrmPackageSettingsDialog>();

            //okay so the package entry dialog should redirect us to the connection entry when started as we dont have a connection yet
            var connectionEntryDialog = dialog.SubDialogs.First();
            var connectionEntry = fakeVsixApplication.GetSubObjectEntryViewModel(connectionEntryDialog);

            var connectionToEnter = GetXrmRecordConfiguration();
            fakeVsixApplication.EnterAndSaveObject(connectionToEnter, connectionEntry);

            //okay now the connection is entered it should navigate to the package settings entry
            var packageSettingsEntry = fakeVsixApplication.GetSubObjectEntryViewModel(dialog, index: 1);
            Assert.AreEqual("Fake", packageSettingsEntry.GetStringFieldFieldViewModel(nameof(XrmPackageSettings.SolutionObjectPrefix)).Value);

            //we want to verify that the object had the settings passed into it as well as the lookup connection works
            packageSettingsEntry.GetBooleanFieldFieldViewModel(nameof(XrmPackageSettings.AddToSolution)).Value = true;
            var solutionPicklistField = packageSettingsEntry.GetLookupFieldFieldViewModel(nameof(XrmPackageSettings.Solution));
            solutionPicklistField.SelectedItem = solutionPicklistField.ItemsSource.ElementAt(1);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(packageSettingsEntry.GetStringFieldFieldViewModel(nameof(XrmPackageSettings.SolutionDynamicsCrmPrefix)).Value));

            var connectionsubGrid = packageSettingsEntry.GetEnumerableFieldViewModel(nameof(XrmPackageSettings.Connections));
            Assert.IsTrue(connectionsubGrid.GridRecords.Any());
            Assert.AreEqual(connectionToEnter.OrganizationUniqueName, connectionsubGrid.GridRecords.First().GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.OrganizationUniqueName)).Value);

            packageSettingsEntry.SaveButtonViewModel.Invoke();

            Assert.IsTrue(packageSettings.Connections.Any());
            Assert.AreEqual(connectionToEnter.OrganizationUniqueName, packageSettings.Connections.First().OrganizationUniqueName);
        }
    }
}
