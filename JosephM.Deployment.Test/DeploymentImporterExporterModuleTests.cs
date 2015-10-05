using System;
using System.Linq;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Prism.XrmModule.Test;
using JosephM.Xrm.ImportExporter.Prism;
using JosephM.Xrm.ImportExporter.Service;
using JosephM.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.Deployment.Test
{
    [TestClass]
    public class DeploymentImporterExporterModuleTests : XrmModuleTest
    {
        [TestMethod]
        public void DeploymentImporterExporterModuleTest()
        {
            var account = CreateAccount();
            FileUtility.DeleteFiles(TestingFolder);

            var application = CreateAndLoadTestApplication<XrmImporterExporterModule>();

            var instance = new XrmImporterExporterRequest();
            instance.ImportExportTask = ImportExportTask.ExportXml;
            instance.IncludeNotes = true;
            instance.IncludeNNRelationshipsBetweenEntities = true;
            instance.FolderPath = new Folder(TestingFolder);
            instance.RecordTypes = new[]
            {
                new ImportExportRecordType()
                {
                    Type = ExportType.AllRecords,
                    RecordType = new RecordType(Entities.account, Entities.account),
                    ExcludeFields = new [] { new FieldSetting() { RecordField = new RecordField(Fields.account_.accountcategorycode, Fields.account_.accountcategorycode)}}
                }
            };

            application.NavigateAndProcessDialog<XrmImporterExporterModule, XrmImporterExporterDialog>(instance);

            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());
        }
    }
}
