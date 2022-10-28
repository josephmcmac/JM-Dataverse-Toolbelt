using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Core.FieldType;
using JosephM.Deployment.ImportSolution;
using JosephM.Deployment.SolutionTransfer;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace JosephM.Deployment.Test
{
    [TestClass]
    public class DeploymentSolutionTransferTests : XrmModuleTest
    {
        [DeploymentItem(@"Files\TestImportScript_1_0_0_0.zip")]
        [TestMethod]
        public void DeploymentSolutionTransferTest()
        {
            var installedSolution = XrmRecordService.GetFirst(Entities.solution, Fields.solution_.friendlyname, "TestImportScript");
            if (installedSolution != null)
            {
                XrmRecordService.Delete(installedSolution);
            }
            var solutionFile1 = new FileInfo(@"TestImportScript_1_0_0_0.zip");
            var createApplicationImport = CreateAndLoadTestApplication<ImportSolutionModule>();
            var entryFormImport = createApplicationImport.NavigateToDialogModuleEntryForm<ImportSolutionModule, ImportSolutionDialog>();
            entryFormImport.GetFieldViewModel<FileRefFieldViewModel>(nameof(ImportSolutionRequest.SolutionZip)).Value = new FileReference(solutionFile1.FullName);
            entryFormImport.GetObjectFieldFieldViewModel(nameof(ImportSolutionRequest.TargetConnection)).Value = GetSavedXrmRecordConfiguration();
            Assert.AreEqual("TestImportScript", entryFormImport.GetStringFieldFieldViewModel(nameof(ImportSolutionRequest.UniqueName)).Value);
            Assert.AreEqual("1.0.0.0", entryFormImport.GetStringFieldFieldViewModel(nameof(ImportSolutionRequest.Version)).Value);
            Assert.IsFalse(entryFormImport.GetBooleanFieldFieldViewModel(nameof(ImportSolutionRequest.IsManaged)).Value);
            Assert.IsFalse(entryFormImport.GetBooleanFieldFieldViewModel(nameof(ImportSolutionRequest.IsCurrentlyInstalledInTarget)).Value);
            Assert.IsFalse(entryFormImport.GetBooleanFieldFieldViewModel(nameof(ImportSolutionRequest.InstallAsUpgrade)).Value);

            if (!entryFormImport.Validate())
            {
                Assert.Fail(entryFormImport.GetValidationSummary());
            }
            entryFormImport.SaveButtonViewModel.Invoke();

            installedSolution = XrmRecordService.GetFirst(Entities.solution, Fields.solution_.friendlyname, "TestImportScript");
            Assert.IsNotNull(installedSolution);
            Assert.AreEqual("1.0.0.0", installedSolution.GetStringField(Fields.solution_.version));

            var createApplication = CreateAndLoadTestApplication<SolutionTransferModule>();
            var entryForm = createApplication.NavigateToDialogModuleEntryForm<SolutionTransferModule, SolutionTransferDialog>();
            entryForm.GetObjectFieldFieldViewModel(nameof(SolutionTransferRequest.SourceConnection)).Value = GetSavedXrmRecordConfiguration();
            entryForm.GetObjectFieldFieldViewModel(nameof(SolutionTransferRequest.TargetConnection)).Value = GetSavedXrmRecordConfiguration();
            entryForm.GetFieldViewModel<LookupFieldViewModel>(nameof(SolutionTransferRequest.Solution)).Value = entryForm.GetFieldViewModel<LookupFieldViewModel>(nameof(SolutionTransferRequest.Solution)).ItemsSource.First(po => po.Record?.Id == installedSolution.Id).Record.ToLookup();
            Assert.AreEqual("TestImportScript", entryForm.GetStringFieldFieldViewModel(nameof(SolutionTransferRequest.UniqueName)).Value);
            Assert.AreEqual("1.0.0.0", entryForm.GetStringFieldFieldViewModel(nameof(SolutionTransferRequest.Version)).Value);
            Assert.IsFalse(entryForm.GetBooleanFieldFieldViewModel(nameof(SolutionTransferRequest.IsManaged)).Value);
            Assert.IsTrue(entryForm.GetBooleanFieldFieldViewModel(nameof(SolutionTransferRequest.IsCurrentlyInstalledInTarget)).Value);
            Assert.IsFalse(entryForm.GetBooleanFieldFieldViewModel(nameof(SolutionTransferRequest.InstallAsUpgrade)).Value);
            Assert.AreEqual("1.0.0.0", entryForm.GetStringFieldFieldViewModel(nameof(SolutionTransferRequest.CurrentTargetVersion)).Value);
            Assert.IsFalse(entryForm.GetBooleanFieldFieldViewModel(nameof(SolutionTransferRequest.CurrentTargetVersionManaged)).Value);
            entryForm.GetStringFieldFieldViewModel(nameof(SolutionTransferRequest.SourceVersionForRelease)).Value = "1.0.0.1";
            entryForm.GetStringFieldFieldViewModel(nameof(SolutionTransferRequest.SetSourceVersionPostRelease)).Value = "1.0.0.2";

            if (!entryForm.Validate())
            {
                Assert.Fail(entryForm.GetValidationSummary());
            }
            entryForm.SaveButtonViewModel.Invoke();

            installedSolution = XrmRecordService.GetFirst(Entities.solution, Fields.solution_.friendlyname, "TestImportScript");
            Assert.IsNotNull(installedSolution);
            Assert.AreEqual("1.0.0.2", installedSolution.GetStringField(Fields.solution_.version));
        }
    }
}
