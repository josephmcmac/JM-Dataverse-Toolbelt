using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Utility;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Xrm.Schema;
using JosephM.XRM.VSIX;
using JosephM.XRM.VSIX.Commands.PackageSettings;
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

            var dialog = new XrmPackageSettingDialog(CreateDialogController(), packageSettinns, VisualStudioService, true, XrmRecordService);
            dialog.Controller.BeginDialog();

            //okay I am going to add script in here to create a new publisher and solution
            var entryViewModel = GetEntryForm(dialog);
            entryViewModel.LoadFormSections();

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

            var completionScreen = dialog.Controller.UiItems.First() as CompletionScreenViewModel;
            Assert.IsFalse(completionScreen.CompletionDetails.Items.Any());
            completionScreen.CloseButton.Invoke();
        }
    }
}
