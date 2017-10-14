using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Xrm.ImportExporter.Service;
using JosephM.Xrm.Schema;
using JosephM.XRM.VSIX.Commands.CreateDeploymentPackage;
using JosephM.XRM.VSIX.Commands.DeployPackage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixCreateAndDeployPackageTests : JosephMVsixTests
    {
        [TestMethod]
        public void VsixCreateAndDeployPackageTest()
        {
            DeleteAll(Entities.account);

            var account = XrmRecordService.NewRecord(Entities.account);
            account.SetField(Fields.account_.name, "TEST", XrmRecordService);
            account.Id = XrmRecordService.Create(account, null);

            var tempFolder = Path.Combine(TestingFolder, "TEMP");
            if (Directory.Exists(tempFolder))
            {
                FileUtility.DeleteFiles(tempFolder);
                FileUtility.DeleteSubFolders(tempFolder);
            }
            var packageSettings = GetTestPackageSettings();
            XrmService.SetField(Entities.solution, new Guid(packageSettings.Solution.Id), Fields.solution_.version, "2.0.0.0");

            var request = XrmSolutionImporterExporterRequest.CreateForCreatePackage(tempFolder, packageSettings.Solution);
            request.DataToInclude = new[]
            {
                new ImportExportRecordType()
                {
                     RecordType = new RecordType(Entities.account, Entities.account), Type = ExportType.AllRecords
                }
            };

            var service = new XrmSolutionImporterExporterService(XrmRecordService);

            var dialogCreate = new CreateDeploymentPackageDialog(service, request, CreateDialogController(), packageSettings, VisualStudioService);
            dialogCreate.Controller.BeginDialog();

            var entryForm = GetEntryForm(dialogCreate);
            var thisVersionField = entryForm.GetStringFieldFieldViewModel(nameof(XrmSolutionImporterExporterRequest.ThisReleaseVersion));
            var nextVersionField = entryForm.GetStringFieldFieldViewModel(nameof(XrmSolutionImporterExporterRequest.SetVersionPostRelease));
            Assert.AreEqual("2.0.0.0", nextVersionField.Value);
            Assert.AreEqual("2.0.0.0", thisVersionField.Value);
            thisVersionField.Value = "3.0.0.0";
            Assert.AreEqual("3.0.0.0", nextVersionField.Value);
            nextVersionField.Value = "4.0.0.0";
            var dataToExportField = entryForm.GetSubGridViewModel(nameof(XrmSolutionImporterExporterRequest.DataToInclude));
            dataToExportField.AddRow();
            var dataToExportRow = dataToExportField.GridRecords.First();
            dataToExportRow.GetRecordTypeFieldViewModel(nameof(ImportExportRecordType.RecordType)).Value = new RecordType(Entities.account, Entities.account);
            dataToExportRow.GetPicklistFieldFieldViewModel(nameof(ImportExportRecordType.Type)).Value = new PicklistOption(ExportType.AllRecords.ToString(), ExportType.AllRecords.ToString());
            SubmitEntryForm(dialogCreate);

            var folder = Directory.GetDirectories(Path.Combine(VisualStudioService.SolutionDirectory, "Releases")).First();
            Assert.IsTrue(FileUtility.GetFiles(folder).First().EndsWith(".zip"));
            Assert.IsTrue(FileUtility.GetFolders(folder).First().EndsWith("Data"));
            Assert.IsTrue(FileUtility.GetFiles((FileUtility.GetFolders(folder).First())).Any());

            var solution = XrmRecordService.Get(packageSettings.Solution.RecordType, packageSettings.Solution.Id);
            Assert.AreEqual("4.0.0.0", solution.GetStringField(Fields.solution_.version));

            //delete for recreation
            XrmRecordService.Delete(account);

            //Okay now lets deploy it
            request = XrmSolutionImporterExporterRequest.CreateForDeployPackage(folder);
            request.Connection = packageSettings.Connections.First();

            service = new XrmSolutionImporterExporterService(XrmRecordService);
            var dialogDeploy = new DeployPackageDialog(service, request, CreateDialogController(), packageSettings, VisualStudioService);
            dialogDeploy.Controller.BeginDialog();

            SubmitEntryForm(dialogDeploy);

            solution = XrmRecordService.Get(packageSettings.Solution.RecordType, packageSettings.Solution.Id);
            Assert.AreEqual("3.0.0.0", solution.GetStringField(Fields.solution_.version));

            //should be recreated
            account = XrmRecordService.Get(account.Type, account.Id);
        }
    }
}
