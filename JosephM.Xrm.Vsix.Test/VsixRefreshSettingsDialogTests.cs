using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XRM.VSIX;
using JosephM.XRM.VSIX.Commands.RefreshConnection;
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
            var packageSettinns = GetTestPackageSettings();

            var dialog = new XrmPackageSettingsDialog(CreateDialogController(), packageSettinns, VisualStudioService, XrmRecordService);
            dialog.Controller.BeginDialog();

            //okay I am going to add script in here to create a new publisher and solution
            var entryViewModel = GetEntryForm(dialog);
            entryViewModel.LoadFormSections();

            entryViewModel.GetStringFieldFieldViewModel(nameof(XrmPackageSettings.SolutionDynamicsCrmPrefix)).Value = "Foo";
            entryViewModel.GetStringFieldFieldViewModel(nameof(XrmPackageSettings.SolutionObjectPrefix)).Value = "Foo";

            var solutionField = entryViewModel.GetLookupFieldFieldViewModel(nameof(XrmPackageSettings.Solution));
            var Loo = solutionField.LookupService;
            var For = solutionField.LookupFormService;
            Assert.IsTrue(solutionField.AllowNew);
            solutionField.Value = null;
            DeleteTestNewLookupSolution();
            solutionField.NewButton.Invoke();

            var solutionEntryForm = entryViewModel.ChildForms.First() as RecordEntryFormViewModel;
            Assert.IsNotNull(solutionEntryForm);
            solutionEntryForm.LoadFormSections();
            solutionEntryForm.GetStringFieldFieldViewModel(Fields.solution_.uniquename).Value = "TESTNEWLOOKUPSOLUTION";
            solutionEntryForm.GetStringFieldFieldViewModel(Fields.solution_.friendlyname).Value = "TESTNEWLOOKUPSOLUTION";
            solutionEntryForm.GetStringFieldFieldViewModel(Fields.solution_.version).Value = "1.0.0.0";
            var publisherField = solutionEntryForm.GetLookupFieldFieldViewModel(Fields.solution_.publisherid);
            Assert.IsTrue(publisherField.AllowNew);
            DeleteTestNewLookupPublisher();
            publisherField.NewButton.Invoke();

            var publisherEntryForm = solutionEntryForm.ChildForms.First() as RecordEntryFormViewModel;
            Assert.IsNotNull(publisherEntryForm);
            publisherEntryForm.LoadFormSections();
            publisherEntryForm.GetStringFieldFieldViewModel(Fields.publisher_.uniquename).Value = "TESTNEWLOOKUPPUBLISHER";
            publisherEntryForm.GetStringFieldFieldViewModel(Fields.publisher_.friendlyname).Value = "TESTNEWLOOKUPPUBLISHER";
            publisherEntryForm.GetStringFieldFieldViewModel(Fields.publisher_.customizationprefix).Value = "abc";
            publisherEntryForm.GetIntegerFieldFieldViewModel(Fields.publisher_.customizationoptionvalueprefix).Value = 12345;

            publisherEntryForm.SaveButtonViewModel.Invoke();
            Assert.IsFalse(solutionEntryForm.ChildForms.Any(), publisherEntryForm.GetValidationSummary());

            Assert.IsNotNull(publisherField.Value);
            solutionEntryForm.SaveButtonViewModel.Invoke();
            Assert.IsFalse(entryViewModel.ChildForms.Any(), solutionEntryForm.GetValidationSummary());

            Assert.IsNotNull(solutionField.Value);
            SubmitEntryForm(dialog);

            //todo refactor these to service requests
            var completionScreen = dialog.Controller.UiItems.First() as CompletionScreenViewModel;
            //todo add an assert no error in 
            completionScreen.CloseButton.Invoke();
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
            var subgrid = packageentryForm.GetSubGridViewModel(nameof(XrmPackageSettings.Connections));
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
