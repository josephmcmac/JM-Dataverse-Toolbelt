using System;
using System.Linq;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Prism.XrmModule.Test;
using JosephM.Record.Extentions;
using JosephM.Xrm.ImportExporter.Prism;
using JosephM.Xrm.ImportExporter.Service;
using JosephM.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            migrationRequest.ImportToConnection = GetSavedXrmRecordConfiguration();
            migrationRequest.FolderPath = new Folder(TestingFolder);
            migrationRequest.SolutionMigrations = new[]
            {
                new SolutionMigration()
                {
                    Connection = GetSavedXrmRecordConfiguration(),
                    Solution = solution.ToLookup(),
                    Managed = false,
                    DataToExport = new[]
                    {
                        new ImportExportRecordType()
                        {
                            RecordType = new RecordType(Entities.account, Entities.account),
                            Type = ExportType.AllRecords,
                           
                            ExcludeFields = new[]
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
                    Managed = false,
                    DataToExport = new[]
                    {
                        new ImportExportRecordType()
                        {
                            RecordType = new RecordType(Entities.account, Entities.account),
                            Type = ExportType.AllRecords,
                           
                            ExcludeFields = new[]
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

        }
    }
}
