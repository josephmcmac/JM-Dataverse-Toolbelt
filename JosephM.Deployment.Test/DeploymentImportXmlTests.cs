using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Deployment.ExportXml;
using JosephM.Deployment.ImportXml;
using JosephM.Prism.XrmModule.Test;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.Test;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Xrm.ImporterExporter.Test
{
    [TestClass]
    public class DeploymentImportXmlTests : XrmModuleTest
    {
        [TestMethod]
        public void DeploymentImportXmlAssociationsAndNotesTest()
        {
            PrepareTests();
            var types = new[] { Entities.jmcg_testentity };
            var workFolder = ClearFilesAndData(types);

            var importService = new ImportXmlService(XrmRecordService);

            var createRecords = new List<Entity>();
            foreach (var type in types)
            {
                for (var i = 0; i < 2; i++)
                {
                    createRecords.Add(CreateTestRecord(type, importService));
                }
            }
            //if throws error ensure the test entity allows notes
            XrmService.Associate(Relationships.jmcg_testentity_.jmcg_testentity_jmcg_testentity.Name, Entities.jmcg_testentity, createRecords[0].Id, true, Entities.jmcg_testentity, createRecords[1].Id);
            var aNote = CreateTestRecord(Schema.Entities.annotation, new Dictionary<string, object>
            {
                { Schema.Fields.annotation_.objectid, createRecords[0].ToEntityReference() },
                { Schema.Fields.annotation_.subject, "Test Scripting" },
                { Schema.Fields.annotation_.notetext, "Just For Importing Testing" },
            });


            var exportService = new ExportXmlService(XrmRecordService);
            var exportRequest = new ExportXmlRequest
            {
                Folder = new Folder(workFolder),
                RecordTypesToExport = types.Select(t => new ExportRecordType() { RecordType = new RecordType(t, t) }),
                IncludeNNRelationshipsBetweenEntities = true,
                IncludeNotes = true
            };
            var response = exportService.Execute(exportRequest, Controller);
            Assert.IsFalse(response.HasError);

            foreach (var type in types)
                DeleteAll(type);

            var application = CreateAndLoadTestApplication<ImportXmlModule>();

            var importRequest = new ImportXmlRequest
            {
                Folder = new Folder(workFolder)
            };

            var importResponse = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(importResponse.HasError);

            var rMetadata = XrmService.GetRelationshipMetadata(Relationships.jmcg_testentity_.jmcg_testentity_jmcg_testentity.Name);

            foreach (var record in createRecords)
            {
                var loaded = Refresh(record);
                if(createRecords[0].Id == loaded.Id)
                {
                    var notes = XrmService.RetrieveAllAndClauses(Schema.Entities.annotation, new[]
                    {
                        new ConditionExpression(Schema.Fields.annotation_.objectid, ConditionOperator.Equal, loaded.Id)
                    });
                    Assert.AreEqual(1, notes.Count());
                    var associated = XrmService.GetAssociatedIds(rMetadata.SchemaName, rMetadata.Entity1IntersectAttribute, loaded.Id, rMetadata.Entity2IntersectAttribute);
                    Assert.AreEqual(1, associated.Count());
                    Assert.AreEqual(createRecords[1].Id, associated.First());
                }
            }
        }

        [TestMethod]
        public void DeploymentImportXmlMultipleTest()
        {
            PrepareTests();
            var types = new[] { Entities.jmcg_testentitytwo, Entities.jmcg_testentitythree, Entities.jmcg_testentity };
            var workFolder = ClearFilesAndData(types);

            var importService = new ImportXmlService(XrmRecordService);

            var createRecords = new List<Entity>();
            foreach (var type in types)
            {
                createRecords.Add(CreateTestRecord(type, importService));
            }

            var exportService = new ExportXmlService(XrmRecordService);
            var exportRequest = new ExportXmlRequest
            {
                Folder = new Folder(workFolder),
                RecordTypesToExport = types.Select(t => new ExportRecordType() { RecordType = new RecordType(t, t) })
            };
            var response = exportService.Execute(exportRequest, Controller);
            Assert.IsFalse(response.HasError);

            foreach (var type in types)
                DeleteAll(type);

            var application = CreateAndLoadTestApplication<ImportXmlModule>();

            var importRequest = new ImportXmlRequest
            {
                Folder = new Folder(workFolder)
            };

            var importResponse = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(importResponse.HasError);
        }

        [TestMethod]
        public void DeploymentImportXmlSimpleTest()
        {
            var type = TestEntityType;
            PrepareTests();
            var workFolder = ClearFilesAndData(type);

            var importService = new ImportXmlService(XrmRecordService);

            var fields = GetFields(type, importService);
            var updateFields = GetUpdateFields(type, importService);

            var record = CreateTestRecord(type, importService);
            var createdEntity = XrmService.Retrieve(record.LogicalName, record.Id);

            var exportService = new ExportXmlService(XrmRecordService);
            var exportRequest = new ExportXmlRequest
            {
                Folder = new Folder(workFolder),
                RecordTypesToExport = new[] { new ExportRecordType() { RecordType = new RecordType(TestEntityType, TestEntityType) } }
            };
            var exportResponse = exportService.Execute(exportRequest, Controller);
            Assert.IsTrue(exportResponse.Success);

            XrmService.Delete(record);

            var application = CreateAndLoadTestApplication<ImportXmlModule>();

            var importRequest = new ImportXmlRequest
            {
                Folder = new Folder(workFolder)
            };

            var response = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(response.HasError);

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

            importRequest = new ImportXmlRequest
            {
                Folder = new Folder(workFolder)
            };

            application = CreateAndLoadTestApplication<ImportXmlModule>();
            response = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(response.HasError);

            var updatedRecord = XrmService.Retrieve(type, record.Id);

            foreach (var updateField in updateFields)
                Assert.IsTrue(XrmEntity.FieldsEqual(createdEntity.GetField(updateField),
                    updatedRecord.GetField(updateField)));
        }

        /// <summary>
        /// Test just verifies that an export xml for the various different types executes
        /// </summary>
        [TestMethod]
        public void DeploymentImportXmlTypesTest()
        {
            var query = new QueryExpression();

            PrepareTests();
            var types = new[] { Entities.jmcg_testentitytwo, Entities.jmcg_testentitythree, Entities.jmcg_testentity };
            var workFolder = ClearFilesAndData(types);

            var importService = new ImportXmlService(XrmRecordService);

            var t1_1 = CreateTestRecord(Entities.jmcg_testentity, importService);
            var t1_2 = CreateTestRecord(Entities.jmcg_testentity, importService);
            var t1_3 = CreateTestRecord(Entities.jmcg_testentity, importService);

            var t2_1 = CreateTestRecord(Entities.jmcg_testentitytwo, importService);
            var t2_2 = CreateTestRecord(Entities.jmcg_testentitytwo, importService);
            var t2_3 = CreateTestRecord(Entities.jmcg_testentitytwo, importService);

            var t3_1 = CreateTestRecord(Entities.jmcg_testentitythree, importService);
            var t3_2 = CreateTestRecord(Entities.jmcg_testentitythree, importService);
            var t3_3 = CreateTestRecord(Entities.jmcg_testentitythree, importService);

            var t1RequestAll = new ExportRecordType()
            {
                Type = ExportType.AllRecords,
                RecordType = new RecordType(Entities.jmcg_testentity, Entities.jmcg_testentity)
            };
            var t2RequestFetch = new ExportRecordType()
            {
                Type = ExportType.FetchXml,
                RecordType = new RecordType(Entities.jmcg_testentitytwo, Entities.jmcg_testentitytwo),
                FetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' count='2' >
                      <entity name='" + Entities.jmcg_testentitytwo + @"'>
                      </entity>
                    </fetch>"
            };
            var t3RequestSpecific = new ExportRecordType()
            {
                Type = ExportType.SpecificRecords,
                RecordType = new RecordType(Entities.jmcg_testentitythree, Entities.jmcg_testentitythree),
                SpecificRecordsToExport = new[]
                {
                    new LookupSetting() { Record = new Lookup(Entities.jmcg_testentitythree, t3_1.Id.ToString(), "t3_1") },
                    new LookupSetting() { Record = new Lookup(Entities.jmcg_testentitythree, t3_2.Id.ToString(), "t3_2") },
                }
            };

            var exportRequest = new ExportXmlRequest
            {
                Folder = new Folder(workFolder),
                RecordTypesToExport = new[] { t1RequestAll, t2RequestFetch, t3RequestSpecific }
            };
            var exportService = new ExportXmlService(XrmRecordService);
            var exportResponse = exportService.Execute(exportRequest, Controller);
            Assert.IsFalse(exportResponse.HasError);

            var entities = importService.LoadEntitiesFromXmlFiles(workFolder);

            Assert.AreEqual(7, entities.Count());

            var importRequest = new ImportXmlRequest
            {
                Folder = new Folder(workFolder)
            };
            var importResponse = importService.Execute(importRequest, Controller);
            Assert.IsFalse(importResponse.HasError);
        }

        [TestMethod]
        public void DeploymentImportXmlMaskEmailsTest()
        {
            PrepareTests();
            var workFolder = ClearFilesAndData();
            DeleteAll(Entities.account);

            var entity = CreateAccount();
            Assert.IsFalse(entity.GetStringField(Fields.account_.emailaddress1).Contains("_AT_"));

            var accountsExport = new ExportRecordType()
            {
                Type = ExportType.AllRecords,
                RecordType = new RecordType(Entities.account, Entities.account)
            };

            var exportRequest = new ExportXmlRequest
            {
                Folder = new Folder(workFolder),
                RecordTypesToExport = new[] { accountsExport }
            };
            var exportService = new ExportXmlService(XrmRecordService);
            var ecxportResponse = exportService.Execute(exportRequest, Controller);
            Assert.IsFalse(ecxportResponse.HasError);

            var importRequest = new ImportXmlRequest
            {
                Folder = new Folder(workFolder),
                MaskEmails = true
            };
            var application = CreateAndLoadTestApplication<ImportXmlModule>();
            var immportResponse = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(immportResponse.HasError);

            entity = XrmService.GetFirst(Entities.account);
            Assert.IsTrue(entity.GetStringField(Fields.account_.emailaddress1).Contains("_AT_"));

            importRequest = new ImportXmlRequest
            {
                Folder = new Folder(workFolder),
                MaskEmails = false
            };
            application = CreateAndLoadTestApplication<ImportXmlModule>();
            immportResponse = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(immportResponse.HasError);

            entity = XrmService.GetFirst(Entities.account);
            Assert.IsFalse(entity.GetStringField(Fields.account_.emailaddress1).Contains("_AT_"));

        }
        /// <summary>
        /// Test just verifies knowledge articles import both create and update
        /// Initially was throwing various including one existed with the same article number
        /// </summary>
        [TestMethod]
        public void DeploymentImportXmlKnowledgeArticleTest()
        {
            PrepareTests();

            var type = "knowledgearticle";
            if (XrmRecordService.RecordTypeExists(type))
            {
                DeleteAll(type);

                var workFolder = ClearFilesAndData();

                //create or get a knowledge article
                var knowledgeArticle = XrmService.GetFirst(type, "articlepublicnumber", PopulateStringValue);
                if (knowledgeArticle == null)
                    knowledgeArticle = CreateRecordAllFieldsPopulated("knowledgearticle");

                //export to xml
                var export = new ExportRecordType()
                {
                    Type = ExportType.AllRecords,
                    RecordType = new RecordType(type, type)
                };

                var exportRequest = new ExportXmlRequest
                {
                    Folder = new Folder(workFolder),
                    RecordTypesToExport = new[] { export }
                };
                var exportService = new ExportXmlService(XrmRecordService);
                var ecxportResponse = exportService.Execute(exportRequest, Controller);
                Assert.IsFalse(ecxportResponse.HasError);

                //okay we will delete then create one with the same article number
                XrmService.Delete(knowledgeArticle);
                knowledgeArticle = CreateRecordAllFieldsPopulated("knowledgearticle");

                var kaCount = XrmService.RetrieveAllEntityType(type).Count();

                //import should match them
                var importRequest = new ImportXmlRequest
                {
                    Folder = new Folder(workFolder),
                    MaskEmails = true
                };
                var application = CreateAndLoadTestApplication<ImportXmlModule>();
                var immportResponse = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
                Assert.IsFalse(immportResponse.HasError);
                Assert.AreEqual(kaCount, XrmService.RetrieveAllEntityType(type).Count());

                //now lets just verify for create (delete it prioor to import)
                XrmService.Delete(knowledgeArticle);
                importRequest = new ImportXmlRequest
                {
                    Folder = new Folder(workFolder),
                    MaskEmails = true
                };
                application = CreateAndLoadTestApplication<ImportXmlModule>();
                immportResponse = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
                Assert.IsFalse(immportResponse.HasError);
                Assert.AreEqual(kaCount, XrmService.RetrieveAllEntityType(type).Count());
            }
        }


        private Entity CreateTestRecord(string type, ImportXmlService importService)
        {
            var record = new Entity(type);
            var fields1 = GetFields(type, importService);
            foreach (var field in fields1)
            {
                record.SetField(field, CreateNewEntityFieldValue(field, type, record));
            }
            record.Id = XrmService.Create(record);
            return record;
        }

        private IEnumerable<string> GetFields(string type, ImportXmlService importService)
        {
            var fields = XrmService.GetFields(type).Where(f => importService.IsIncludeField(f, type));
            return fields;
        }

        private IEnumerable<string> GetUpdateFields(string type, ImportXmlService importService)
        {
            var updateFields = XrmService.GetFields(type).Where(f => importService.IsIncludeField(f, type));
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
                if (!Directory.Exists(workFolder))
                    Directory.CreateDirectory(workFolder);
                return workFolder;
            }
        }
    }
}