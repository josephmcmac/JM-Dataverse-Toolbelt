using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment.CreatePackage;
using JosephM.Record.Extentions;
using JosephM.Xrm.DataImportExport.Import;
using JosephM.Xrm.DataImportExport.XmlImport;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Deployment.Test
{
    [TestClass]
    public class DeploymentCreateAndDeployPackageModuleTests : XrmModuleTest
    {
        [TestMethod]
        public void DeploymentCreateAndDeployPackageModuleTest()
        {
            DeleteAll(Entities.account);
            DeleteAll(Entities.contact);

            var account = CreateAccount();
            var solution = XrmRecordService.GetFirst(Entities.solution, Fields.solution_.uniquename, "TestComponents");
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
                     RecordType = new RecordType(Entities.account, Entities.account), Type = ExportType.AllRecords,
                },
                new ExportRecordType()
                {
                     RecordType = new RecordType(Entities.contact, Entities.contact), Type = ExportType.AllRecords
                }
            };
            var createApplication = CreateAndLoadTestApplication<CreatePackageModule>();
            var response = createApplication.NavigateAndProcessDialog<CreatePackageModule, CreatePackageDialog, ServiceResponseBase<DataImportResponseItem>>(createDeploymentPackageRequest);
            Assert.IsFalse(response.HasError);

            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).First().EndsWith(".zip"));
            Assert.IsTrue(FileUtility.GetFolders(TestingFolder).First().EndsWith("Data"));
            Assert.IsTrue(FileUtility.GetFiles((FileUtility.GetFolders(TestingFolder).First())).Any());

            solution = XrmRecordService.Get(solution.Type, solution.Id);
            Assert.AreEqual("4.0.0.0", solution.GetStringField(Fields.solution_.version));
        }
    }
}
