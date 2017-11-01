using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Deployment.CreatePackage;
using JosephM.Deployment.DeployPackage;
using JosephM.Deployment.ExportXml;
using JosephM.Prism.XrmModule.Test;
using JosephM.Record.Extentions;
using JosephM.Xrm.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace JosephM.Deployment.Test
{
    [TestClass]
    public class DeploymentCreateAndDeployPackageModuleTests : XrmModuleTest
    {
        [TestMethod]
        public void DeploymentCreateAndDeployPackageModuleTest()
        {

            var account = CreateAccount();
            var solution = XrmRecordService.GetFirst("solution", XrmRecordService.GetPrimaryField("solution"), "Test Components");
            Assert.IsNotNull(solution);

            FileUtility.DeleteFiles(TestingFolder);

            var createDeploymentPackageRequest = new CreatePackageRequest();
            createDeploymentPackageRequest.FolderPath = new Folder(TestingFolder);
            createDeploymentPackageRequest.Solution = solution.ToLookup();
            createDeploymentPackageRequest.ThisReleaseVersion = "3.0.0.0";
            createDeploymentPackageRequest.SetVersionPostRelease = "4.0.0.0";
            createDeploymentPackageRequest.DataToInclude = new[]
            {
                new ExportRecordType()
                {
                     RecordType = new RecordType(Entities.account, Entities.account), Type = ExportType.AllRecords
                }
            };
            var createApplication = CreateAndLoadTestApplication<CreatePackageModule>();
            createApplication.NavigateAndProcessDialog<CreatePackageModule, CreatePackageDialog>(createDeploymentPackageRequest);

            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).First().EndsWith(".zip"));
            Assert.IsTrue(FileUtility.GetFolders(TestingFolder).First().EndsWith("Data"));
            Assert.IsTrue(FileUtility.GetFiles((FileUtility.GetFolders(TestingFolder).First())).Any());

            solution = XrmRecordService.Get(solution.Type, solution.Id);
            Assert.AreEqual("4.0.0.0", solution.GetStringField(Fields.solution_.version));

            //delete for recreation
            XrmService.Delete(account);

            //Okay now lets deploy it
            var deployRequest = new DeployPackageRequest();
            deployRequest.FolderContainingPackage = new Folder(TestingFolder);
            deployRequest.Connection = GetSavedXrmRecordConfiguration();

            var deployApplication = CreateAndLoadTestApplication<DeployPackageModule>();
            deployApplication.NavigateAndProcessDialog<DeployPackageModule, DeployPackageDialog>(deployRequest);

            solution = XrmRecordService.Get(solution.Type, solution.Id);
            Assert.AreEqual("3.0.0.0", solution.GetStringField(Fields.solution_.version));

            //should be recreated
            account = Refresh(account);

            createDeploymentPackageRequest = new CreatePackageRequest();
            createDeploymentPackageRequest.FolderPath = new Folder(TestingFolder);
            createDeploymentPackageRequest.Solution = solution.ToLookup();
            createDeploymentPackageRequest.ThisReleaseVersion = "3.0.0.0";
            createDeploymentPackageRequest.SetVersionPostRelease = "4.0.0.0";
            createDeploymentPackageRequest.DeployPackageInto = GetSavedXrmRecordConfiguration();
            createDeploymentPackageRequest.DataToInclude = new[]
            {
                new ExportRecordType()
                {
                     RecordType = new RecordType(Entities.account, Entities.account), Type = ExportType.AllRecords
                }
            };
            //error if already .zips on the folder
            FileUtility.WriteToFile(TestingFolder, "Fake.zip", "FakeContent");
            createApplication = CreateAndLoadTestApplication<CreatePackageModule>();
            try
            {
                createApplication.NavigateAndProcessDialog<CreatePackageModule, CreatePackageDialog>(createDeploymentPackageRequest);
                Assert.Fail();
            }
            catch(Exception ex)
            {
                Assert.IsFalse(ex is AssertFailedException);
            }
            FileUtility.DeleteFiles(TestingFolder);
            FileUtility.DeleteSubFolders(TestingFolder);
            createApplication.NavigateAndProcessDialog<CreatePackageModule, CreatePackageDialog>(createDeploymentPackageRequest);

            solution = XrmRecordService.Get(solution.Type, solution.Id);
            Assert.AreEqual("3.0.0.0", solution.GetStringField(Fields.solution_.version));
        }
    }
}
