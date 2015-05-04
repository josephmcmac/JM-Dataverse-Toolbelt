using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Record.Application.SettingTypes;
using JosephM.Record.Xrm.Test;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.ImportExporter.Service;

namespace JosephM.Xrm.ImporterExporter.Test
{
    [TestClass]
    public class XrmImporterExporterTest : XrmRecordTest
    {
        [DeploymentItem(@"Files\Test Entity.csv")]
        [DeploymentItem(@"Files\Test Entity Two.csv")]
        [DeploymentItem(@"Files\Test Entity Three.csv")]
        [TestMethod]
        public void ExportImportCsvMultipleTest()
        {
            PrepareTests();
            var types = new[] { "new_testentitytwo", "new_testentitythree", "new_testentity" };
            var workFolder = ClearFilesAndData(types);

            File.Copy(@"Test Entity.csv", Path.Combine(workFolder, @"Test Entity.csv"));
            File.Copy(@"Test Entity Two.csv", Path.Combine(workFolder, @"Test Entity Two.csv"));
            File.Copy(@"Test Entity Three.csv", Path.Combine(workFolder, @"Test Entity Three.csv"));

            var importerExporterService = new XrmImporterExporterService<XrmRecord, XrmRecordService>(XrmRecordService);

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
        public void ExportImportXmlMultipleTest()
        {
            PrepareTests();
            var types = new[] {"new_testentitytwo", "new_testentitythree", "new_testentity"};
            var workFolder = ClearFilesAndData(types);

            var importerExporterService = new XrmImporterExporterService<XrmRecord, XrmRecordService>(XrmRecordService);

            var createRecords = new List<Entity>();
            foreach (var type in types)
            {
                createRecords.Add(CreateTestRecord(type, importerExporterService));
            }
            var request = new XrmImporterExporterRequest
            {
                FolderPath = new Folder(workFolder),
                ImportExportTask = ImportExportTask.ExportXml,
                RecordTypes = types.Select(t => new RecordTypeSetting(t, t))
            };
            var response = importerExporterService.Execute(request, Controller);
            Assert.IsFalse(response.HasError);

            foreach (var type in types)
                DeleteAll(type);

            request = new XrmImporterExporterRequest
            {
                FolderPath = new Folder(workFolder),
                ImportExportTask = ImportExportTask.ImportXml,
                RecordTypes = types.Select(t => new RecordTypeSetting(t, t))
            };
            response = importerExporterService.Execute(request, Controller);
            Assert.IsFalse(response.HasError);
        }

        [TestMethod]
        public void ExportImportXmlSimpleTest()
        {
            var type = TestEntityType;
            PrepareTests();
            var workFolder = ClearFilesAndData(type);

            var importerExporterService = new XrmImporterExporterService<XrmRecord, XrmRecordService>(XrmRecordService);

            var fields = GetFields(type, importerExporterService);
            var updateFields = GetUpdateFields(type, importerExporterService);

            var record = CreateTestRecord(type, importerExporterService);
            var createdEntity = XrmService.Retrieve(record.LogicalName, record.Id);

            var request = new XrmImporterExporterRequest
            {
                FolderPath = new Folder(workFolder),
                ImportExportTask = ImportExportTask.ExportXml,
                RecordTypes = new[] { new RecordTypeSetting(TestEntityType, TestEntityType) }
            };
            var response = importerExporterService.Execute(request, Controller);
            Assert.IsTrue(response.Success);

            XrmService.Delete(record);

            request = new XrmImporterExporterRequest
            {
                FolderPath = new Folder(workFolder),
                ImportExportTask = ImportExportTask.ImportXml,
                RecordTypes = new[] { new RecordTypeSetting(TestEntityType, TestEntityType) }
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

            request = new XrmImporterExporterRequest
            {
                FolderPath = new Folder(workFolder),
                ImportExportTask = ImportExportTask.ExportXml,
                RecordTypes = new[] { new RecordTypeSetting(TestEntityType, TestEntityType) }
            };
            response = importerExporterService.Execute(request, Controller);
            Assert.IsTrue(response.Success);

            request = new XrmImporterExporterRequest
            {
                FolderPath = new Folder(workFolder),
                ImportExportTask = ImportExportTask.ImportXml,
                RecordTypes = new[] { new RecordTypeSetting(TestEntityType, TestEntityType) }
            };

            response = importerExporterService.Execute(request, Controller);
            Assert.IsTrue(response.Success);
            var updatedRecord = XrmService.Retrieve(type, record.Id);

            foreach (var updateField in updateFields)
                Assert.IsTrue(XrmEntity.FieldsEqual(record.GetField(updateField),
                    updatedRecord.GetField(updateField)));
        }

        private Entity CreateTestRecord(string type, XrmImporterExporterService<XrmRecord, XrmRecordService> importerExporterService)
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

        private IEnumerable<string> GetFields(string type, XrmImporterExporterService<XrmRecord, XrmRecordService> importerExporterService)
        {
            var fields = XrmService.GetFields(type).Where(f => importerExporterService.IsIncludeField(f, type));
            return fields;
        }

        private IEnumerable<string> GetUpdateFields(string type, XrmImporterExporterService<XrmRecord, XrmRecordService> importerExporterService)
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