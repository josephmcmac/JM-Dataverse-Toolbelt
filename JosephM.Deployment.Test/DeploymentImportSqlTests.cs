using JosephM.Application.Application;
using JosephM.Application.Desktop.Application;
using JosephM.Application.Desktop.Console;
using JosephM.Application.Desktop.Console.Test;
using JosephM.Application.Desktop.Test;
using JosephM.Application.ViewModel.ApplicationOptions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Sql;
using JosephM.Core.Utility;
using JosephM.Deployment.ImportSql;
using JosephM.Record.Extentions;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Record.Sql;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Deployment.Test
{
    [TestClass]
    public class DeploymentImportSqlTests : XrmModuleTest
    {
        [TestMethod]
        public void DeploymentImportSqlTest()
        {
            //this script create some data in a database
            //then runs a synch twice into dynamics with a reference change in between

            //it also fakes the process running in a console app

            //also verifies email sent with summary
            
            var queueSendFrom = XrmRecordService.ToIRecord(TestQueue);
            var queueSendTo = XrmRecordService.ToIRecord(TestQueue1);

            //script of basic database import
            PrepareTests();
            DeleteAll(Entities.email);

            var sqlServer = @"LT5CG0110PZT";
            var databaseName = "TestScriptDatabaseImport";

            //request with mappings from sql to dynamics
            var request = new ImportSqlRequest()
            {
                ConnectionString = $"Provider=sqloledb;Data Source={sqlServer};Initial Catalog={databaseName};Integrated Security=SSPI;",
                SendNotificationAtCompletion = true,
                SendNotificationFromQueue = XrmRecordService.ToLookup(queueSendFrom),
                SendNotificationToQueue = XrmRecordService.ToLookup(queueSendTo),
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

            //configure synch data
            var recordsToCreate = 3;
            foreach (var tableMapping in request.Mappings)
            {
                //delete all in both
                var truncate = $"truncate table {tableMapping.SourceTable.Key}";
                recordMetadataService.ExecuteSql(truncate);
                DeleteAll(tableMapping.TargetType.Key);

                //create data in the db
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

            //run the synch
            var app = CreateAndLoadTestApplication<ImportSqlModule>();
            app.AddModule<ConsoleApplicationModule>();

            //navigate to the dialog
            var dialog = app.NavigateToDialog<ImportSqlModule, ImportSqlDialog>();
            var entryViewmodel = app.GetSubObjectEntryViewModel(dialog);
            app.EnterObject(request, entryViewmodel);
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

            //point the account field in the db to 3
            var linkAllToAccount3 = $"update TestRecords set Account = 3";
            recordMetadataService.ExecuteSql(linkAllToAccount3);

            dialog = app.NavigateToDialog<ImportSqlModule, ImportSqlDialog>();
            entryViewmodel = app.GetSubObjectEntryViewModel(dialog);
            app.EnterObject(request, entryViewmodel);

            //okay here we generate a saved request and get the command line for it
            //to also run a console app after this second synch
            ClearSavedRequests(app, entryViewmodel);
            //trigger save request
            var saveRequestButton = entryViewmodel.GetButton("SAVEREQUEST");
            saveRequestButton.Invoke();

            //enter and save details including autoload
            var saveRequestForm = app.GetSubObjectEntryViewModel(entryViewmodel);
            var detailsEntered = new SaveAndLoadFields()
            {
                Name = "TestName"
            };
            app.EnterAndSaveObject(detailsEntered, saveRequestForm);
            Assert.IsFalse(entryViewmodel.ChildForms.Any());
            Assert.IsFalse(entryViewmodel.LoadingViewModel.IsLoading);

            //invoke load request dialog
            var loadRequestButton = entryViewmodel.GetButton("LOADREQUEST");
            loadRequestButton.Invoke();
            var loadRequestForm = app.GetSubObjectEntryViewModel(entryViewmodel);

            //verify there is a saved request and trigger the generate bat button
            var subGrid = loadRequestForm.GetEnumerableFieldViewModel(nameof(SavedSettings.SavedRequests));
            Assert.IsTrue(subGrid.GridRecords.Count() == 1);
            subGrid.GridRecords.First().IsSelected = true;

            var generateBatButton = subGrid.DynamicGridViewModel.GetButton("GENERATEBAT");
            generateBatButton.Invoke();

            var testFiles = FileUtility.GetFiles(TestingFolder);
            Assert.AreEqual(1, testFiles.Count());
            Assert.IsTrue(testFiles.First().EndsWith(".bat"));
            var batContent = File.ReadAllText(testFiles.First());
            loadRequestForm.CancelButtonViewModel.Invoke();
            Assert.IsFalse(entryViewmodel.ChildForms.Any());
            Assert.IsFalse(entryViewmodel.LoadingViewModel.IsLoading);

            //okay we now have the bat args for later so lets run the update synch in the app
            entryViewmodel.SaveButtonViewModel.Invoke();
            completionScreen = app.GetCompletionViewModel(dialog);
            importResponse = completionScreen.GetObject() as ImportSqlResponse;
            Assert.IsNotNull(importResponse);
            Assert.IsFalse(importResponse.ResponseItems.Any());

            //verify all point to account 3
            var updatedRecords = XrmRecordService.RetrieveAll(Entities.jmcg_testentity, null);
            Assert.AreEqual(3, updatedRecords.Count());
            Assert.IsTrue(updatedRecords.All(r => r.GetLookupName(Fields.jmcg_testentity_.jmcg_account) == "3"));

            //okay lets fake run it for the console app args generated
            var args = ConsoleTestUtility.CommandLineToArgs(batContent)
                .Skip(1)
                .ToArray();

            var arguments = ConsoleApplication.ParseCommandLineArguments(args);
            var applicationName = arguments.ContainsKey("SettingsFolderName") ? arguments["SettingsFolderName"] : "Unknown Console Context";

            //okay need to create app
            var dependencyResolver = new DependencyContainer();
            var controller = new ConsoleApplicationController(applicationName, dependencyResolver);
            var settingsManager = new DesktopSettingsManager(controller);
            var applicationOptions = new ApplicationOptionsViewModel(controller);
            var consoleApp = new ConsoleApplication(controller, applicationOptions, settingsManager);
            //load modules in folder path
            consoleApp.LoadModulesInExecutionFolder();

            var connection = GetSavedXrmRecordConfiguration();

            SavedXrmConnectionsModule.RefreshXrmServices(GetXrmRecordConfiguration(), consoleApp.Controller);
            consoleApp.Controller.RegisterInstance<ISavedXrmConnections>(new SavedXrmConnections
            {
                Connections = new[] { connection }
            });

            //run app
            consoleApp.Run(args);

            //email created for each synch
            var emails = XrmRecordService.RetrieveAll(Entities.email, null);
            Assert.AreEqual(3, emails.Count());
            Assert.IsTrue(emails.All(e => e.GetOptionKey(Fields.email_.statecode) == OptionSets.Email.ActivityStatus.Completed.ToString()));
        }
        
        private void ClearSavedRequests(TestApplication app, RecordEntryFormViewModel entryViewmodel)
        {
            if (entryViewmodel.CustomFunctions.Any(cb => cb.Id == "LOADREQUEST"))
            {
                var loadRequestButton = entryViewmodel.GetButton("LOADREQUEST");
                loadRequestButton.Invoke();
                //enter and save details
                var saveRequestForm = app.GetSubObjectEntryViewModel(entryViewmodel);
                var requestsGrid = saveRequestForm.GetEnumerableFieldViewModel(nameof(SavedSettings.SavedRequests));
                foreach (var item in requestsGrid.GridRecords.ToArray())
                {
                    requestsGrid.DynamicGridViewModel.DeleteRow(item);
                }
                saveRequestForm.SaveButtonViewModel.Invoke();
                Assert.IsFalse(entryViewmodel.ChildForms.Any());
                Assert.IsFalse(entryViewmodel.LoadingViewModel.IsLoading);
            }
        }
    }
}