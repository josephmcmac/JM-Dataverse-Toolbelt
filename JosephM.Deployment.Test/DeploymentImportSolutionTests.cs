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
    public class DeploymentImportSolutionTests : XrmModuleTest
    {
        [DeploymentItem(@"Files\TestImportScript_1_0_0_0_managed.zip")]
        [DeploymentItem(@"Files\TestImportScript_1_0_0_1_managed.zip")]
        [TestMethod]
        public void DeploymentImportSolutionTest()
        {
            var installedSolution = XrmRecordService.GetFirst(Entities.solution, Fields.solution_.friendlyname, "TestImportScript");
            if(installedSolution != null)
            {
                XrmRecordService.Delete(installedSolution);
            }
            var solutionFile1 = new FileInfo(@"TestImportScript_1_0_0_0_managed.zip");
            var createApplication = CreateAndLoadTestApplication<ImportSolutionModule>();
            var entryForm = createApplication.NavigateToDialogModuleEntryForm<ImportSolutionModule, ImportSolutionDialog>();
            entryForm.GetFieldViewModel<FileRefFieldViewModel>(nameof(ImportSolutionRequest.SolutionZip)).Value = new FileReference(solutionFile1.FullName);
            entryForm.GetObjectFieldFieldViewModel(nameof(ImportSolutionRequest.TargetConnection)).Value = GetSavedXrmRecordConfiguration();
            Assert.AreEqual("TestImportScript", entryForm.GetStringFieldFieldViewModel(nameof(ImportSolutionRequest.UniqueName)).Value);
            Assert.AreEqual("1.0.0.0", entryForm.GetStringFieldFieldViewModel(nameof(ImportSolutionRequest.Version)).Value);
            Assert.IsTrue(entryForm.GetBooleanFieldFieldViewModel(nameof(ImportSolutionRequest.IsManaged)).Value);
            Assert.IsFalse(entryForm.GetBooleanFieldFieldViewModel(nameof(ImportSolutionRequest.IsCurrentlyInstalledInTarget)).Value);
            Assert.IsFalse(entryForm.GetBooleanFieldFieldViewModel(nameof(ImportSolutionRequest.InstallAsUpgrade)).Value);

            if(!entryForm.Validate())
            {
                Assert.Fail(entryForm.GetValidationSummary());
            }
            entryForm.SaveButtonViewModel.Invoke();

            installedSolution = XrmRecordService.GetFirst(Entities.solution, Fields.solution_.friendlyname, "TestImportScript");
            Assert.IsNotNull(installedSolution);
            Assert.AreEqual("1.0.0.0", installedSolution.GetStringField(Fields.solution_.version));


            var solutionFile2 = new FileInfo(@"TestImportScript_1_0_0_1_managed.zip");
            createApplication = CreateAndLoadTestApplication<ImportSolutionModule>();
            entryForm = createApplication.NavigateToDialogModuleEntryForm<ImportSolutionModule, ImportSolutionDialog>();
            entryForm.GetFieldViewModel<FileRefFieldViewModel>(nameof(ImportSolutionRequest.SolutionZip)).Value = new FileReference(solutionFile2.FullName);
            entryForm.GetObjectFieldFieldViewModel(nameof(ImportSolutionRequest.TargetConnection)).Value = GetSavedXrmRecordConfiguration();
            Assert.AreEqual("TestImportScript", entryForm.GetStringFieldFieldViewModel(nameof(ImportSolutionRequest.UniqueName)).Value);
            Assert.AreEqual("1.0.0.1", entryForm.GetStringFieldFieldViewModel(nameof(ImportSolutionRequest.Version)).Value);
            Assert.IsTrue(entryForm.GetBooleanFieldFieldViewModel(nameof(ImportSolutionRequest.IsManaged)).Value);
            Assert.IsTrue(entryForm.GetBooleanFieldFieldViewModel(nameof(ImportSolutionRequest.IsCurrentlyInstalledInTarget)).Value);
            Assert.IsTrue(entryForm.GetBooleanFieldFieldViewModel(nameof(ImportSolutionRequest.CurrentTargetVersionManaged)).Value);
            Assert.AreEqual("1.0.0.0", entryForm.GetStringFieldFieldViewModel(nameof(ImportSolutionRequest.CurrentTargetVersion)).Value);
            Assert.IsTrue(entryForm.GetBooleanFieldFieldViewModel(nameof(ImportSolutionRequest.InstallAsUpgrade)).Value);

            if (!entryForm.Validate())
            {
                Assert.Fail(entryForm.GetValidationSummary());
            }
            entryForm.SaveButtonViewModel.Invoke();

            installedSolution = XrmRecordService.GetFirst(Entities.solution, Fields.solution_.friendlyname, "TestImportScript");
            Assert.IsNotNull(installedSolution);
            Assert.AreEqual("1.0.0.1", installedSolution.GetStringField(Fields.solution_.version));
        }
    }
}
