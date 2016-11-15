using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using JosephM.Application.ViewModel.SettingTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Record.Xrm.Test;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.ImportExporter.Service;
using JosephM.Xrm.Test;
using Microsoft.Xrm.Sdk.Query;

namespace JosephM.Xrm.ImporterExporter.Test
{
    [TestClass]
    public class DeploymentImporterExporterTest : XrmRecordTest
    {
        [DeploymentItem(@"Files\Account.csv")]
        [DeploymentItem(@"Files\jmcg_testentity_account.csv")]
        [DeploymentItem(@"Files\Test Entity.csv")]
        [DeploymentItem(@"Files\Test Entity Two.csv")]
        [DeploymentItem(@"Files\Test Entity Three.csv")]
        [TestMethod]
        public void DeploymentExportImportCsvMultipleTest()
        {
            PrepareTests(true);
            var types = new[] { Entities.jmcg_testentitytwo, Entities.jmcg_testentitythree, Entities.jmcg_testentity, Entities.account };
            var workFolder = ClearFilesAndData(types);

            File.Copy(@"Account.csv", Path.Combine(workFolder, @"Account.csv"));
            File.Copy(@"jmcg_testentity_account.csv", Path.Combine(workFolder, @"jmcg_testentity_account.csv"));
            File.Copy(@"Test Entity.csv", Path.Combine(workFolder, @"Test Entity.csv"));
            File.Copy(@"Test Entity Two.csv", Path.Combine(workFolder, @"Test Entity Two.csv"));
            File.Copy(@"Test Entity Three.csv", Path.Combine(workFolder, @"Test Entity Three.csv"));

            var importerExporterService = new XrmImporterExporterService<XrmRecordService>(XrmRecordService);

            var request = new XrmImporterExporterRequest
            {
                FolderPath = new Folder(workFolder),
                ImportExportTask = ImportExportTask.ImportCsvs,
                DateFormat = DateFormat.English
            };
            var response = importerExporterService.Execute(request, Controller);
            if (response.HasError)
                throw response.ResponseItemsWithError.First().Exception;
        }

        [TestMethod]
        public void DeploymentExportImportXmlMultipleTest()
        {
            PrepareTests(true);
            var types = new[] {Entities.jmcg_testentitytwo, Entities.jmcg_testentitythree, Entities.jmcg_testentity};
            var workFolder = ClearFilesAndData(types);

            var importerExporterService = new XrmImporterExporterService<XrmRecordService>(XrmRecordService);

            var createRecords = new List<Entity>();
            foreach (var type in types)
            {
                createRecords.Add(CreateTestRecord(type, importerExporterService));
            }
            var request = new XrmImporterExporterRequest
            {
                FolderPath = new Folder(workFolder),
                ImportExportTask = ImportExportTask.ExportXml,
                RecordTypes = types.Select(t => new ImportExportRecordType() { RecordType = new RecordType(t, t) })
            };
            var response = importerExporterService.Execute(request, Controller);
            Assert.IsFalse(response.HasError);

            foreach (var type in types)
                DeleteAll(type);

            request = new XrmImporterExporterRequest
            {
                FolderPath = new Folder(workFolder),
                ImportExportTask = ImportExportTask.ImportXml,
                RecordTypes = types.Select(t => new ImportExportRecordType() { RecordType = new RecordType(t, t) })
            };
            response = importerExporterService.Execute(request, Controller);
            Assert.IsFalse(response.HasError);
        }

        [TestMethod]
        public void DeploymentExportImportXmlSimpleTest()
        {
            var type = TestEntityType;
            PrepareTests(true);
            var workFolder = ClearFilesAndData(type);

            var importerExporterService = new XrmImporterExporterService<XrmRecordService>(XrmRecordService);

            var fields = GetFields(type, importerExporterService);
            var updateFields = GetUpdateFields(type, importerExporterService);

            var record = CreateTestRecord(type, importerExporterService);
            var createdEntity = XrmService.Retrieve(record.LogicalName, record.Id);

            var request = new XrmImporterExporterRequest
            {
                FolderPath = new Folder(workFolder),
                ImportExportTask = ImportExportTask.ExportXml,
                RecordTypes = new [] { new ImportExportRecordType() { RecordType = new RecordType(TestEntityType, TestEntityType) } }
            };
            var response = importerExporterService.Execute(request, Controller);
            Assert.IsTrue(response.Success);

            XrmService.Delete(record);

            request = new XrmImporterExporterRequest
            {
                FolderPath = new Folder(workFolder),
                ImportExportTask = ImportExportTask.ImportXml,
                RecordTypes = new[] { new ImportExportRecordType() { RecordType = new RecordType(TestEntityType, TestEntityType) } }
            };

            response = importerExporterService.Execute(request, Controller);
            Assert.IsTrue(response.Success);
            var createdRecord = XrmService.Retrieve(type, createdEntity.Id);

            foreach (var updateField in updateFields)
                Assert.IsTrue(XrmEntity.FieldsEqual(createdEntity.GetField(updateField),
                    createdRecord.GetField(updateField)));

            foreach (var field in fields)
            {
                record.SetField(field, CreateNewEntityFieldValue(field, type, record));
            }

            XrmService.Update(record);
            record = XrmService.Retrieve(record.LogicalName, record.Id);

            //workFolder = WorkFolder;
            //FileUtility.DeleteFiles(workFolder);

            //request = new XrmImporterExporterRequest
            //{
            //    FolderPath = new Folder(workFolder),
            //    ImportExportTask = ImportExportTask.ExportXml,
            //    RecordTypes = new[] { new ImportExportRecordType() { RecordType = new RecordType(TestEntityType, TestEntityType) } }
            //};
            //response = importerExporterService.Execute(request, Controller);
            //Assert.IsTrue(response.Success);

            request = new XrmImporterExporterRequest
            {
                FolderPath = new Folder(workFolder),
                ImportExportTask = ImportExportTask.ImportXml,
                RecordTypes = new[] { new ImportExportRecordType() { RecordType = new RecordType(TestEntityType, TestEntityType) } }
            };

            response = importerExporterService.Execute(request, Controller);
            Assert.IsTrue(response.Success);
            var updatedRecord = XrmService.Retrieve(type, record.Id);

            foreach (var updateField in updateFields)
                Assert.IsTrue(XrmEntity.FieldsEqual(createdEntity.GetField(updateField),
                    updatedRecord.GetField(updateField)));
        }

        /// <summary>
        /// Test just verifies that an export xml for the various different types executes
        /// </summary>
        [TestMethod]
        public void DeploymentExportXmlTypesTest()
        {
            var query = new QueryExpression();

            PrepareTests(true);
            var types = new[] { Entities.jmcg_testentitytwo, Entities.jmcg_testentitythree, Entities.jmcg_testentity };
            var workFolder = ClearFilesAndData(types);

            var importerExporterService = new XrmImporterExporterService<XrmRecordService>(XrmRecordService);

            var t1_1 = CreateTestRecord(Entities.jmcg_testentity, importerExporterService);
            var t1_2 = CreateTestRecord(Entities.jmcg_testentity, importerExporterService);
            var t1_3 = CreateTestRecord(Entities.jmcg_testentity, importerExporterService);

            var t2_1 = CreateTestRecord(Entities.jmcg_testentitytwo, importerExporterService);
            var t2_2 = CreateTestRecord(Entities.jmcg_testentitytwo, importerExporterService);
            var t2_3 = CreateTestRecord(Entities.jmcg_testentitytwo, importerExporterService);

            var t3_1 = CreateTestRecord(Entities.jmcg_testentitythree, importerExporterService);
            var t3_2 = CreateTestRecord(Entities.jmcg_testentitythree, importerExporterService);
            var t3_3 = CreateTestRecord(Entities.jmcg_testentitythree, importerExporterService);

            var t1RequestAll = new ImportExportRecordType()
            {
                Type = ExportType.AllRecords,
                RecordType = new RecordType(Entities.jmcg_testentity, Entities.jmcg_testentity)
            };
            var t2RequestFetch = new ImportExportRecordType()
            {
                Type = ExportType.FetchXml,
                RecordType = new RecordType(Entities.jmcg_testentitytwo, Entities.jmcg_testentitytwo),
                FetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' count='2' >
                      <entity name='" + Entities.jmcg_testentitytwo + @"'>
                      </entity>
                    </fetch>"
            };
            var t3RequestSpecific = new ImportExportRecordType()
            {
                Type = ExportType.SpecificRecords,
                RecordType = new RecordType(Entities.jmcg_testentitythree, Entities.jmcg_testentitythree),
                OnlyExportSpecificRecords = new[]
                {
                    new LookupSetting() { Record = new Lookup(Entities.jmcg_testentitythree, t3_1.Id.ToString(), "t3_1") },
                    new LookupSetting() { Record = new Lookup(Entities.jmcg_testentitythree, t3_2.Id.ToString(), "t3_2") },
                }
            };

            var request = new XrmImporterExporterRequest
            {
                FolderPath = new Folder(workFolder),
                ImportExportTask = ImportExportTask.ExportXml,
                RecordTypes = new [] { t1RequestAll, t2RequestFetch, t3RequestSpecific}
            };
            var response = importerExporterService.Execute(request, Controller);
            Assert.IsFalse(response.HasError);

            var entities = importerExporterService.LoadEntitiesFromXmlFiles(workFolder);

            Assert.AreEqual(7, entities.Count());
        }

        private Entity CreateTestRecord(string type, XrmImporterExporterService<XrmRecordService> importerExporterService)
        {
            var record = new Entity(type);
            var fields1 = GetFields(type, importerExporterService);
            foreach (var field in fields1)
            {
                record.SetField(field, CreateNewEntityFieldValue(field, type, record));
            }
            record.Id = XrmService.Create(record);
            return record;
        }

        private IEnumerable<string> GetFields(string type, XrmImporterExporterService<XrmRecordService> importerExporterService)
        {
            var fields = XrmService.GetFields(type).Where(f => importerExporterService.IsIncludeField(f, type));
            return fields;
        }

        private IEnumerable<string> GetUpdateFields(string type, XrmImporterExporterService<XrmRecordService> importerExporterService)
        {
            var updateFields = XrmService.GetFields(type).Where(f => importerExporterService.IsIncludeField(f, type));
            return updateFields;
        }

        private string ClearFilesAndData(params string[] typesToDelete)
        {
            foreach (var type in typesToDelete)
                DeleteAll(type);
            var workFolder = WorkFolder;
            FileUtility.DeleteFiles(workFolder);
            return workFolder;
        }

        private string WorkFolder
        {
            get
            {
                var workFolder = TestingFolder + @"\ExportedRecords";
                return workFolder;
            }
        }
    }
}