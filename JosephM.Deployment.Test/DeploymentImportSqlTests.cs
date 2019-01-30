using JosephM.Core.FieldType;
using JosephM.Core.Sql;
using JosephM.Deployment.ImportExcel;
using JosephM.Deployment.ImportSql;
using JosephM.Record.Metadata;
using JosephM.Record.Sql;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using JosephM.Record.Extentions;

namespace JosephM.Deployment.Test
{
    [TestClass]
    public class DeploymentImportSqlTests : XrmModuleTest
    {
        [TestMethod]
        public void DeploymentImportSqlTest()
        {
            //this script create some data in a database
            //then runs a synch into dynamics

            //script of basic database import
            PrepareTests();

            var sqlServer = @"localhost\SQLEXPRESS";
            var databaseName = "TestScriptDatabaseImport";

            //request with mappings from sql to dynamics
            var request = new ImportSqlRequest()
            {
                ConnectionString = $"Provider=sqloledb;Data Source={sqlServer};Initial Catalog={databaseName};Integrated Security=SSPI;",
                Mappings = new[]
                {
                      new ImportSqlRequest.SqlImportTableMapping
                      {
                           SourceTable = new RecordType("Accounts", "Accounts"),
                           TargetType = new RecordType(Entities.account, Entities.account),
                           Mappings = new []
                           {
                               new ImportSqlRequest.SqlImportTableMapping.SqlImportFieldMapping()
                               {
                                   SourceColumn = new RecordField("Name", "Name"),
                                   TargetField = new RecordField(Fields.account_.name, Fields.account_.name)
                               }
                           }
                      },
                      new ImportSqlRequest.SqlImportTableMapping
                      {
                           SourceTable = new RecordType("TestRecords", "TestRecords"),
                           TargetType = new RecordType(Entities.jmcg_testentity, Entities.jmcg_testentity),
                           Mappings = new []
                           {
                               new ImportSqlRequest.SqlImportTableMapping.SqlImportFieldMapping()
                               {
                                   SourceColumn = new RecordField("Name", "Name"),
                                   TargetField = new RecordField(Fields.jmcg_testentity_.jmcg_name, Fields.jmcg_testentity_.jmcg_name)
                               },
                               new ImportSqlRequest.SqlImportTableMapping.SqlImportFieldMapping()
                               {
                                   SourceColumn = new RecordField("Account", "Account"),
                                   TargetField = new RecordField(Fields.jmcg_testentity_.jmcg_account, Fields.jmcg_testentity_.jmcg_account)
                               },
                               new ImportSqlRequest.SqlImportTableMapping.SqlImportFieldMapping()
                               {
                                   SourceColumn = new RecordField("Integer", "Integer"),
                                   TargetField = new RecordField(Fields.jmcg_testentity_.jmcg_integer, Fields.jmcg_testentity_.jmcg_integer)
                               }
                           }
                      }
                }
            };

            //create record and field metadata for the source mappings
            //which will be used to create the source database
            var recordMetadatas = new List<RecordMetadata>();
            foreach (var item in request.Mappings)
            {
                var recordMetadata = new RecordMetadata()
                {
                    SchemaName = item.SourceTable.Key,
                    DisplayName = item.SourceTable.Key
                };
                var fields = new List<FieldMetadata>();
                foreach (var column in item.Mappings)
                {
                    fields.Add(new StringFieldMetadata(column.SourceColumn.Key, column.SourceColumn.Key));
                }
                recordMetadata.Fields = fields;
                recordMetadatas.Add(recordMetadata);
            }

            //if the database doesnt exist create it
            if (!SqlProvider.DatabaseExists(sqlServer, databaseName))
                SqlProvider.CreateDatabase(sqlServer, databaseName);
            //ensure the source database contains tables/columns for our source mappings metadata
            var recordMetadataService = new SqlRecordMetadataService(new SqlServerAndDbSettings(sqlServer, databaseName), recordMetadatas);
            recordMetadataService.RefreshSource();


            var recordsToCreate = 3;

            foreach (var tableMapping in request.Mappings)
            {
                var truncate = $"truncate table {tableMapping.SourceTable.Key}";
                recordMetadataService.ExecuteSql(truncate);
                DeleteAll(tableMapping.TargetType.Key);

                for (var i = 1; i <= recordsToCreate; i++)
                {
                    var newRecord = recordMetadataService.NewRecord(tableMapping.SourceTable.Key);
                    foreach (var fieldMapping in tableMapping.Mappings)
                    {
                        newRecord.SetField(fieldMapping.SourceColumn.Key, i.ToString(), recordMetadataService);
                    }
                    recordMetadataService.Create(newRecord);
                }
            }

            var app = CreateAndLoadTestApplication<ImportSqlModule>();

            //navigate to the dialog
            var dialog = app.NavigateToDialog<ImportSqlModule, ImportSqlDialog>();
            var entryViewmodel = app.GetSubObjectEntryViewModel(dialog);
            app.EnterObject(request, entryViewmodel);
            //select the excel file with the errors and submit form
            entryViewmodel.SaveButtonViewModel.Invoke();

            var completionScreen = app.GetCompletionViewModel(dialog);
            var importResponse = completionScreen.GetObject() as ImportSqlResponse;
            Assert.IsNotNull(importResponse);
            Assert.IsFalse(importResponse.ResponseItems.Any());

            foreach (var tableMapping in request.Mappings)
            {
                var records = XrmRecordService.RetrieveAll(tableMapping.TargetType.Key, null);
                Assert.AreEqual(recordsToCreate, records.Count());
            }

            var linkAllToAccount3 = $"update TestRecords set Account = 3";
            recordMetadataService.ExecuteSql(linkAllToAccount3);

            dialog = app.NavigateToDialog<ImportSqlModule, ImportSqlDialog>();
            entryViewmodel = app.GetSubObjectEntryViewModel(dialog);
            app.EnterObject(request, entryViewmodel);
            //select the excel file with the errors and submit form
            entryViewmodel.SaveButtonViewModel.Invoke();

            completionScreen = app.GetCompletionViewModel(dialog);
            importResponse = completionScreen.GetObject() as ImportSqlResponse;
            Assert.IsNotNull(importResponse);
            Assert.IsFalse(importResponse.ResponseItems.Any());

            var updatedRecords = XrmRecordService.RetrieveAll(Entities.jmcg_testentity, null);
            Assert.AreEqual(3, updatedRecords.Count());
            Assert.IsTrue(updatedRecords.All(r => r.GetLookupName(Fields.jmcg_testentity_.jmcg_account) == "3"));
        }
    }
}