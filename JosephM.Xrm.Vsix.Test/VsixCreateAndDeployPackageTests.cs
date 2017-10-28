using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Deployment.CreatePackage;
using JosephM.Deployment.DeployPackage;
using JosephM.Deployment.ExportXml;
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
            var account = CreateAccount();

            var tempFolder = Path.Combine(TestingFolder, "TEMP");
            if (Directory.Exists(tempFolder))
            {
                FileUtility.DeleteFiles(tempFolder);
                FileUtility.DeleteSubFolders(tempFolder);
            }
            var packageSettings = GetTestPackageSettings();
            XrmService.SetField(Entities.solution, new Guid(packageSettings.Solution.Id), Fields.solution_.version, "2.0.0.0");

            var request = CreatePackageRequest.CreateForCreatePackage(tempFolder, packageSettings.Solution);
            request.DataToInclude = new[]
            {
                new ExportRecordType()
                {
                     RecordType = new RecordType(Entities.account, Entities.account), Type = ExportType.AllRecords
                }
            };

            var service = new CreatePackageService(XrmRecordService);

            var dialogCreate = new XRM.VSIX.Commands.CreateDeploymentPackage.CreateDeploymentPackageDialog(service, request, CreateDialogController(), packageSettings, VisualStudioService);
            dialogCreate.Controller.BeginDialog();

            var entryForm = GetEntryForm(dialogCreate);
            var thisVersionField = entryForm.GetStringFieldFieldViewModel(nameof(CreatePackageRequest.ThisReleaseVersion));
            var nextVersionField = entryForm.GetStringFieldFieldViewModel(nameof(CreatePackageRequest.SetVersionPostRelease));
            Assert.AreEqual("2.0.0.0", nextVersionField.Value);
            Assert.AreEqual("2.0.0.0", thisVersionField.Value);
            thisVersionField.Value = "3.0.0.0";
            Assert.AreEqual("3.0.0.0", nextVersionField.Value);
            nextVersionField.Value = "4.0.0.0";
            var dataToExportField = entryForm.GetSubGridViewModel(nameof(CreatePackageRequest.DataToInclude));
            dataToExportField.AddRow();
            var dataToExportRow = dataToExportField.GridRecords.First();
            dataToExportRow.GetRecordTypeFieldViewModel(nameof(ExportRecordType.RecordType)).Value = new RecordType(Entities.account, Entities.account);
            dataToExportRow.GetPicklistFieldFieldViewModel(nameof(ExportRecordType.Type)).Value = new PicklistOption(ExportType.AllRecords.ToString(), ExportType.AllRecords.ToString());
            SubmitEntryForm(dialogCreate);

            var folder = Directory.GetDirectories(Path.Combine(VisualStudioService.SolutionDirectory, "Releases")).First();
            Assert.IsTrue(FileUtility.GetFiles(folder).First().EndsWith(".zip"));
            Assert.IsTrue(FileUtility.GetFolders(folder).First().EndsWith("Data"));
            Assert.IsTrue(FileUtility.GetFiles((FileUtility.GetFolders(folder).First())).Any());

            var solution = XrmRecordService.Get(packageSettings.Solution.RecordType, packageSettings.Solution.Id);
            Assert.AreEqual("4.0.0.0", solution.GetStringField(Fields.solution_.version));

            //delete for recreation
            XrmService.Delete(account);

            //Okay now lets deploy it
            var deployRequest = DeployPackageRequest.CreateForDeployPackage(folder);
            deployRequest.Connection = packageSettings.Connections.First();

            var deployService = new DeployPackageService(XrmRecordService);
            var dialogDeploy = new XRM.VSIX.Commands.DeployPackage.DeployPackageDialog(deployService, deployRequest, CreateDialogController(), packageSettings, VisualStudioService);
            dialogDeploy.Controller.BeginDialog();

            SubmitEntryForm(dialogDeploy);

            solution = XrmRecordService.Get(packageSettings.Solution.RecordType, packageSettings.Solution.Id);
            Assert.AreEqual("3.0.0.0", solution.GetStringField(Fields.solution_.version));

            //should be recreated
            account = Refresh(account);
        }
    }
}
