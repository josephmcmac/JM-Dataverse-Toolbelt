using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Module.Connection;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixRefreshSettingsDialogTests : JosephMVsixTests
    {
        [TestMethod]
        public void VsixRefreshSettingsDialogTest()
        {
            var testApplication = CreateAndLoadTestApplication<XrmPackageSettingsModule>();
            var dialog = testApplication.NavigateToDialog<XrmPackageSettingsModule, XrmPackageSettingsDialog>();

            //okay this one doesn't just auto enter an object
            //I am going to create a new publisher and solution
            //in the lookup fields

            //get the settings entry form
            var entryViewModel = GetEntryForm(dialog);
            entryViewModel.LoadFormSections();

            //set dummy prefix values
            entryViewModel.GetStringFieldFieldViewModel(nameof(XrmPackageSettings.SolutionDynamicsCrmPrefix)).Value = "Foo";
            entryViewModel.GetStringFieldFieldViewModel(nameof(XrmPackageSettings.SolutionObjectPrefix)).Value = "Foo";

            DeleteTestNewLookupSolution();
            //invoke new on the solution lookup field
            var solutionField = entryViewModel.GetLookupFieldFieldViewModel(nameof(XrmPackageSettings.Solution));
            var Loo = solutionField.LookupService;
            var For = solutionField.LookupFormService;
            Assert.IsTrue(solutionField.AllowNew);
            solutionField.Value = null;
            solutionField.NewButton.Invoke();

            //get the new solution entry form
            var solutionEntryForm = entryViewModel.ChildForms.First() as RecordEntryFormViewModel;
            Assert.IsNotNull(solutionEntryForm);
            solutionEntryForm.LoadFormSections();
            solutionEntryForm.GetStringFieldFieldViewModel(Fields.solution_.uniquename).Value = "TESTNEWLOOKUPSOLUTION";
            solutionEntryForm.GetStringFieldFieldViewModel(Fields.solution_.friendlyname).Value = "TESTNEWLOOKUPSOLUTION";
            solutionEntryForm.GetStringFieldFieldViewModel(Fields.solution_.version).Value = "1.0.0.0";

            DeleteTestNewLookupPublisher();
            //invoke new on the publisher field
            var publisherField = solutionEntryForm.GetLookupFieldFieldViewModel(Fields.solution_.publisherid);
            Assert.IsTrue(publisherField.AllowNew);
            publisherField.NewButton.Invoke();

            //get the new publisher entry form enter some details and save
            var publisherEntryForm = solutionEntryForm.ChildForms.First() as RecordEntryFormViewModel;
            Assert.IsNotNull(publisherEntryForm);
            publisherEntryForm.LoadFormSections();
            publisherEntryForm.GetStringFieldFieldViewModel(Fields.publisher_.uniquename).Value = "TESTNEWLOOKUPPUBLISHER";
            publisherEntryForm.GetStringFieldFieldViewModel(Fields.publisher_.friendlyname).Value = "TESTNEWLOOKUPPUBLISHER";
            publisherEntryForm.GetStringFieldFieldViewModel(Fields.publisher_.customizationprefix).Value = "abc";
            publisherEntryForm.GetIntegerFieldFieldViewModel(Fields.publisher_.customizationoptionvalueprefix).Value = 12345;
            publisherEntryForm.SaveButtonViewModel.Invoke();
            //verify publisher form closed
            Assert.IsFalse(solutionEntryForm.ChildForms.Any(), publisherEntryForm.GetValidationSummary());

            //verify the solution entry form is populated with the created publisher
            Assert.IsNotNull(publisherField.Value);
            Assert.IsNotNull(XrmRecordService.Get(publisherField.Value.RecordType, publisherField.Value.Id));

            //save the solution and verify form closed
            solutionEntryForm.SaveButtonViewModel.Invoke();
            Assert.IsFalse(entryViewModel.ChildForms.Any(), solutionEntryForm.GetValidationSummary());

            //verify the package entry form is populated with the created solution
            Assert.IsNotNull(solutionField.Value);
            Assert.IsNotNull(XrmRecordService.Get(solutionField.Value.RecordType, solutionField.Value.Id));
            //save package entry form
            SubmitEntryForm(dialog);

            var completionScreen = dialog.Controller.UiItems.First() as CompletionScreenViewModel;

            completionScreen.CloseButton.Invoke();
            Assert.IsNull(dialog.FatalException);

            //verify the package settings now have the solution we created when resolved
            var settingsManager = testApplication.Controller.ResolveType(typeof(ISettingsManager)) as ISettingsManager;
            Assert.IsNotNull(settingsManager);
            Assert.AreEqual(solutionField.Value.Id, settingsManager.Resolve<XrmPackageSettings>().Solution.Id);
        }

        /// <summary>
        /// This script veirfies that when the package settings dialog is triggered and no connection is yet
        /// configured/added in the solution then the user is redirected to enter the connection first
        /// </summary>
        [TestMethod]
        public void VsixRefreshSettingsDialogAddConnectionFirstIfNotYetInSolutionTest()
        {
            InitialiseModuleXrmConnection = false;

            var testApplication = CreateAndLoadTestApplication<XrmPackageSettingsModule>();

            var dialog = testApplication.NavigateToDialog<XrmPackageSettingsModule, XrmPackageSettingsDialog>();
            var connectionSubDialog = testApplication.GetSubDialog(dialog);
            Assert.IsTrue(connectionSubDialog is ConnectionEntryDialog);
            var connectionEntryDialog = testApplication.GetSubObjectEntryViewModel(connectionSubDialog);
            Assert.IsTrue(connectionEntryDialog is ObjectEntryViewModel);
            testApplication.EnterAndSaveObject(GetXrmRecordConfiguration(), connectionEntryDialog);

            var packageentryForm = testApplication.GetSubObjectEntryViewModel(dialog, 1);
            //verify that the connection was passed into the cponnecitons grid
            var subgrid = packageentryForm.GetEnumerableFieldViewModel(nameof(XrmPackageSettings.Connections));
            Assert.IsNotNull(subgrid.GridRecords.First().GetStringFieldFieldViewModel(nameof(SavedXrmRecordConfiguration.OrganizationUniqueName)).Value);
            //verify that the solution picklist was populated (uses the entered conneciton to populate)
            var solutionPicklist = packageentryForm.GetLookupFieldFieldViewModel(nameof(XrmPackageSettings.Solution));
            Assert.IsTrue(solutionPicklist.ItemsSource.Any());

            Assert.IsTrue((packageentryForm.GetRecord() as ObjectRecord).Instance is XrmPackageSettings);
            testApplication.EnterAndSaveObject(GetTestPackageSettings(), packageentryForm);

            var settingsManager = testApplication.Controller.ResolveType(typeof(ISettingsManager)) as ISettingsManager;
            Assert.IsNotNull(settingsManager);
            Assert.IsNotNull(settingsManager.Resolve<XrmRecordConfiguration>().OrganizationUniqueName);
            Assert.IsTrue(settingsManager.Resolve<XrmPackageSettings>().Connections.Any());
        }
    }
}
