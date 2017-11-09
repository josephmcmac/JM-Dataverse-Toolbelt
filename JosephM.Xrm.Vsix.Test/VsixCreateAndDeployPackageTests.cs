using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment;
using JosephM.Deployment.CreatePackage;
using JosephM.Deployment.DeployPackage;
using JosephM.Deployment.ExportXml;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Module.CreatePackage;
using JosephM.Xrm.Vsix.Module.DeployPackage;
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
            DeleteAll(Entities.contact);
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
            request.ThisReleaseVersion = "3.0.0.0";
            request.SetVersionPostRelease = "4.0.0.0";
            request.DataToInclude = new[]
            {
                new ExportRecordType()
                {
                     RecordType = new RecordType(Entities.account, Entities.account), Type = ExportType.AllRecords
                },
                new ExportRecordType()
                {
                     RecordType = new RecordType(Entities.contact, Entities.contact), Type = ExportType.AllRecords
                }
            };

            var createTestApplication = CreateAndLoadTestApplication<VsixCreatePackageModule>();
            var createResponse = createTestApplication.NavigateAndProcessDialog<VsixCreatePackageModule, VsixCreatePackageDialog, ServiceResponseBase<DataImportResponseItem>>(request);
            Assert.IsFalse(createResponse.HasError);

            var folder = Directory.GetDirectories(Path.Combine(VisualStudioService.SolutionDirectory, "Releases")).First();
            Assert.IsTrue(FileUtility.GetFiles(folder).First().EndsWith(".zip"));
            Assert.IsTrue(FileUtility.GetFolders(folder).First().EndsWith("Data"));
            Assert.IsTrue(FileUtility.GetFiles((FileUtility.GetFolders(folder).First())).Any());

            var solution = XrmRecordService.Get(packageSettings.Solution.RecordType, packageSettings.Solution.Id);
            Assert.AreEqual("4.0.0.0", solution.GetStringField(Fields.solution_.version));
            //delete for recreation
            XrmService.Delete(account);

            //Okay now lets deploy it
            var deployRequest = new DeployPackageRequest();
            deployRequest.Connection = packageSettings.Connections.First();

            VisualStudioService.SetSelectedItem(new FakeVisualStudioSolutionFolder(folder));

            var deployTestApplication = CreateAndLoadTestApplication<VsixDeployPackageModule>();
            var deployResponse = deployTestApplication.NavigateAndProcessDialog<VsixDeployPackageModule, DeployPackageDialog, ServiceResponseBase<DataImportResponseItem>>(deployRequest);
            Assert.IsFalse(deployResponse.HasError);

            solution = XrmRecordService.Get(packageSettings.Solution.RecordType, packageSettings.Solution.Id);
            Assert.AreEqual("3.0.0.0", solution.GetStringField(Fields.solution_.version));

            //should be recreated
            account = Refresh(account);
        }
    }
}
