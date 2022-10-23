using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment.CreatePackage;
using JosephM.Deployment.DeployPackage;
using JosephM.Deployment.ImportSolution;
using JosephM.Xrm.DataImportExport.Import;
using JosephM.Xrm.DataImportExport.XmlImport;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Module.CreatePackage;
using JosephM.Xrm.Vsix.Module.DeployPackage;
using JosephM.Xrm.Vsix.Module.ImportRecords;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixCreateAndDeployPackageTests : JosephMVsixTests
    {
        /// <summary>
        /// Runs through the create package, deploy package, import solution and import records dialogs
        /// </summary>

        [TestMethod]
        public void VsixCreateAndDeployPackageTest()
        {
            //clear some stuff
            DeleteAll(Entities.account);
            DeleteAll(Entities.contact);
            var tempFolder = Path.Combine(TestingFolder, "TEMP");
            if (Directory.Exists(tempFolder))
            {
                FileUtility.DeleteFiles(tempFolder);
                FileUtility.DeleteSubFolders(tempFolder);
            }

            //create and account for the deployment package
            var account = CreateAccount();
            var packageSettings = GetTestPackageSettings();

            //set solution v2 prior to package create
            XrmService.SetField(Entities.solution, new Guid(packageSettings.Solution.Id), Fields.solution_.version, "2.0.0.0");

            var app = CreateAndLoadTestApplication<VsixCreatePackageModule>();
            //run the dialog - including a redirect to enter the package settings first
            var originalConnection = HijackForPackageEntryRedirect(app);
            var dialog = app.NavigateToDialog<VsixCreatePackageModule, VsixCreatePackageDialog>();
            VerifyPackageEntryRedirect(originalConnection, dialog);
            //okay no should be at the entry the create package details
            var packageEntry = dialog.Controller.UiItems.First() as ObjectEntryViewModel;
            //create package request
            var request = new CreatePackageRequest();
            request.HideTypeAndFolder = true;
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
            app.EnterObject(request, packageEntry);
            //lets set explicit versions
            packageEntry.GetStringFieldFieldViewModel(nameof(CreatePackageRequest.ThisReleaseVersion)).Value = "3.0.0.0";
            packageEntry.GetStringFieldFieldViewModel(nameof(CreatePackageRequest.SetVersionPostRelease)).Value = "4.0.0.0";

            if (!packageEntry.Validate())
                throw new Exception(packageEntry.GetValidationSummary());
            packageEntry.SaveButtonViewModel.Invoke();

            var createResponse = dialog.CompletionItem as ServiceResponseBase<DataImportResponseItem>;

            Assert.IsFalse(createResponse.HasError);

            //verify the files created in the solution package folder
            var folder = Directory.GetDirectories(Path.Combine(VisualStudioService.SolutionDirectory, "Releases")).First();
            Assert.IsTrue(FileUtility.GetFiles(folder).First().EndsWith(".zip"));
            Assert.IsTrue(FileUtility.GetFolders(folder).First().EndsWith("Data"));
            Assert.IsTrue(FileUtility.GetFiles((FileUtility.GetFolders(folder).First())).Any());
            //+ the solution version changed
            var solution = XrmRecordService.Get(packageSettings.Solution.RecordType, packageSettings.Solution.Id);
            Assert.AreEqual("4.0.0.0", solution.GetStringField(Fields.solution_.version));
            
            //delete for account for recreation during import
            XrmService.Delete(account);

            //Okay now lets deploy it
            var deployRequest = new DeployPackageRequest();
            deployRequest.Connection = packageSettings.Connections.First();
            //set the package folder selected in vidsual studio
            VisualStudioService.SetSelectedItem(new FakeVisualStudioSolutionFolder(folder));

            //run the deployment dialog
            var deployTestApplication = CreateAndLoadTestApplication<VsixDeployPackageModule>();
            var deployResponse = deployTestApplication.NavigateAndProcessDialog<VsixDeployPackageModule, DeployPackageDialog, ServiceResponseBase<DataImportResponseItem>>(deployRequest);
            Assert.IsFalse(deployResponse.HasError);

            //verify the solution dpeloyed with updated version
            solution = XrmRecordService.Get(packageSettings.Solution.RecordType, packageSettings.Solution.Id);
            Assert.AreEqual("3.0.0.0", solution.GetStringField(Fields.solution_.version));

            //account should be recreated
            account = Refresh(account);

            //Okay now lets just do the solution import it

            //set the solutoon version something
            XrmService.SetField(Entities.solution, new Guid(packageSettings.Solution.Id), Fields.solution_.version, "2.0.0.0");

            //set the solution zip selected in visual studio
            VisualStudioService.SetSelectedItem(new FakeVisualStudioProjectItem(FileUtility.GetFiles(folder).First()));

            //run the dialog
            var importSolutionRequest = new ImportSolutionRequest();
            importSolutionRequest.Connection = packageSettings.Connections.First();

            var importSolutionApplication = CreateAndLoadTestApplication<ImportSolutionModule>();
            var importSolutionResponse = importSolutionApplication.NavigateAndProcessDialog<ImportSolutionModule, ImportSolutionDialog, ImportSolutionResponse>(importSolutionRequest);
            Assert.IsFalse(importSolutionResponse.HasError);

            //verify the solution dpeloyed with updated version
            Assert.AreEqual("3.0.0.0", solution.GetStringField(Fields.solution_.version));

            //Okay now lets just do the records import

            //delete for account for recreation during import
            XrmService.Delete(account);

            //set the xml files selected
            VisualStudioService.SetSelectedItems(FileUtility.GetFiles(FileUtility.GetFolders(folder).First()).Select(f => new FakeVisualStudioProjectItem(f)));

            //run the import records dialog
            var importRecordsRequest = new ImportRecordsRequest();
            importRecordsRequest.Connection = packageSettings.Connections.First();

            var importRecordsApplication = CreateAndLoadTestApplication<ImportRecordsModule>();
            var importRecordsResponse = importRecordsApplication.NavigateAndProcessDialog<ImportRecordsModule, ImportRecordsDialog, ImportRecordsResponse>(importRecordsRequest);
            Assert.IsFalse(importRecordsResponse.HasError);

            //should be recreated
            account = Refresh(account);
        }
    }
}
