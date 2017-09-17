using System;
using System.Linq;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Prism.XrmModule.Test;
using JosephM.Record.Extentions;
using JosephM.Xrm.ImportExporter.Prism;
using JosephM.Xrm.ImportExporter.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Xrm.Schema;

namespace JosephM.Deployment.Test
{
    [TestClass]
    public class DeploymentSolutionImporterExporterModuleTests : XrmModuleTest
    {
        [TestMethod]
        public void DeploymentSolutionImporterExporterModuleTest()
        {
            var account = CreateAccount();
            var solution = XrmRecordService.GetFirst("solution", XrmRecordService.GetPrimaryField("solution"), "Test Components");
            Assert.IsNotNull(solution);

            FileUtility.DeleteFiles(TestingFolder);

            var migrationRequest = new XrmSolutionImporterExporterRequest();
            migrationRequest.ImportExportTask = SolutionImportExportTask.MigrateSolutions;
            migrationRequest.Connection = GetSavedXrmRecordConfiguration();
            migrationRequest.FolderPath = new Folder(TestingFolder);
            migrationRequest.SolutionMigrations = new[]
            {
                new SolutionMigration()
                {
                    Connection = GetSavedXrmRecordConfiguration(),
                    Solution = solution.ToLookup(),
                    ExportAsManaged = false,
                    DataToExport = new[]
                    {
                        new ImportExportRecordType()
                        {
                            RecordType = new RecordType(Entities.account, Entities.account),
                            Type = ExportType.AllRecords,

                            ExcludeTheseFieldsInExportedRecords = new[]
                            {
                                new FieldSetting
                                {
                                    RecordField =
                                        new RecordField(Fields.account_.accountcategorycode,
                                            Fields.account_.accountcategorycode)
                                }
                            }
                        }
                    }
                }
            };

            var application = CreateAndLoadTestApplication<XrmSolutionImporterExporterModule>();
            application.NavigateAndProcessDialog<XrmSolutionImporterExporterModule, XrmSolutionImporterExporterDialog>(migrationRequest);

            migrationRequest = new XrmSolutionImporterExporterRequest();
            migrationRequest.ImportExportTask = SolutionImportExportTask.ExportSolutions;
            migrationRequest.FolderPath = new Folder(TestingFolder);
            migrationRequest.SolutionExports = new[]
            {
                new SolutionExport()
                {
                    Connection = GetSavedXrmRecordConfiguration(),
                    Solution = solution.ToLookup(),
                    ExportAsManaged = false,
                    DataToExport = new[]
                    {
                        new ImportExportRecordType()
                        {
                            RecordType = new RecordType(Entities.account, Entities.account),
                            Type = ExportType.AllRecords,

                            ExcludeTheseFieldsInExportedRecords = new[]
                            {
                                new FieldSetting
                                {
                                    RecordField =
                                        new RecordField(Fields.account_.accountcategorycode,
                                            Fields.account_.accountcategorycode)
                                }
                            }
                        }
                    }
                }
            };

            application.NavigateAndProcessDialog<XrmSolutionImporterExporterModule, XrmSolutionImporterExporterDialog>(migrationRequest);

            FileUtility.DeleteFiles(TestingFolder);
            FileUtility.DeleteSubFolders(TestingFolder);

            migrationRequest = new XrmSolutionImporterExporterRequest();
            migrationRequest.ImportExportTask = SolutionImportExportTask.CreateDeploymentPackage;
            migrationRequest.FolderPath = new Folder(TestingFolder);
            migrationRequest.Solution = solution.ToLookup();
            migrationRequest.ThisReleaseVersion = "3.0.0.0";
            migrationRequest.SetVersionPostRelease = "4.0.0.0";
            migrationRequest.DataToInclude = new[]
            {
                new ImportExportRecordType()
                {
                     RecordType = new RecordType(Entities.account, Entities.account), Type = ExportType.AllRecords
                }
            };

            application.NavigateAndProcessDialog<XrmSolutionImporterExporterModule, XrmSolutionImporterExporterDialog>(migrationRequest);

            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).First().EndsWith(".zip"));
            Assert.IsTrue(FileUtility.GetFolders(TestingFolder).First().EndsWith("Data"));
            Assert.IsTrue(FileUtility.GetFiles((FileUtility.GetFolders(TestingFolder).First())).Any());

            solution = XrmRecordService.Get(solution.Type, solution.Id);
            Assert.AreEqual("4.0.0.0", solution.GetStringField(Fields.solution_.version));

            //delete for recreation
            XrmService.Delete(account);

            //Okay now lets deploy it
            migrationRequest = new XrmSolutionImporterExporterRequest();
            migrationRequest.ImportExportTask = SolutionImportExportTask.DeployPackage;
            migrationRequest.FolderContainingPackage = new Folder(TestingFolder);
            migrationRequest.Connection = GetSavedXrmRecordConfiguration();

            application.NavigateAndProcessDialog<XrmSolutionImporterExporterModule, XrmSolutionImporterExporterDialog>(migrationRequest);

            solution = XrmRecordService.Get(solution.Type, solution.Id);
            Assert.AreEqual("3.0.0.0", solution.GetStringField(Fields.solution_.version));

            //should be recreated
            account = Refresh(account);

            migrationRequest = new XrmSolutionImporterExporterRequest();
            migrationRequest.ImportExportTask = SolutionImportExportTask.CreateDeploymentPackage;
            migrationRequest.FolderPath = new Folder(TestingFolder);
            migrationRequest.Solution = solution.ToLookup();
            migrationRequest.ThisReleaseVersion = "3.0.0.0";
            migrationRequest.SetVersionPostRelease = "4.0.0.0";
            migrationRequest.DeployPackageInto = GetSavedXrmRecordConfiguration();
            migrationRequest.DataToInclude = new[]
            {
                new ImportExportRecordType()
                {
                     RecordType = new RecordType(Entities.account, Entities.account), Type = ExportType.AllRecords
                }
            };
            //error if already .zips on the folder
            FileUtility.WriteToFile(TestingFolder, "Fake.zip", "FakeContent");
            try
            {
                application.NavigateAndProcessDialog<XrmSolutionImporterExporterModule, XrmSolutionImporterExporterDialog>(migrationRequest);
                Assert.Fail();
            }
            catch(Exception ex)
            {
                Assert.IsFalse(ex is AssertFailedException);
            }
            FileUtility.DeleteFiles(TestingFolder);
            FileUtility.DeleteSubFolders(TestingFolder);
            application.NavigateAndProcessDialog<XrmSolutionImporterExporterModule, XrmSolutionImporterExporterDialog>(migrationRequest);

            solution = XrmRecordService.Get(solution.Type, solution.Id);
            Assert.AreEqual("3.0.0.0", solution.GetStringField(Fields.solution_.version));
        }
    }
}
