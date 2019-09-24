using JosephM.Application.Desktop.Module.SavedRequests;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Deployment.ExportXml;
using JosephM.Deployment.ImportExcel;
using JosephM.Deployment.ImportXml;
using JosephM.Record.Extentions;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Deployment.Test
{
    [TestClass]
    public class DeploymentImportGeneralTests : XrmModuleTest
    {
        [DeploymentItem(@"Files\TestImportMultiples.xlsx")]
        [TestMethod]
        public void DeploymentImportActivityPartiesTest()
        {
            
            PrepareTests();

            var contact1 = XrmService.GetFirst(Entities.contact, Fields.contact_.fullname, "TESTEMAIL IMPORT 1")
                ?? CreateTestRecord(Entities.contact, new Dictionary<string, object>
                {
                    { Fields.contact_.firstname, "TESTEMAIL" },
                    { Fields.contact_.lastname, "IMPORT 1" },
                    { Fields.contact_.emailaddress1, "testscriptcontactimport1@example.com" }
                });
            var contact2 = XrmService.GetFirst(Entities.contact, Fields.contact_.fullname, "TESTEMAIL IMPORT 2")
                ?? CreateTestRecord(Entities.contact, new Dictionary<string, object>
                {
                    { Fields.contact_.firstname, "TESTEMAIL" },
                    { Fields.contact_.lastname, "IMPORT 2" },
                    { Fields.contact_.emailaddress1, "testscriptcontactimport2@example.com" }
                });
            var account = XrmService.GetFirst(Entities.account, Fields.account_.name, "TESTEMAILIMPORT")
                ?? CreateTestRecord(Entities.account, new Dictionary<string, object>
                {
                    { Fields.account_.name, "TESTEMAILIMPORT" },
                    { Fields.account_.emailaddress1, "testscriptaccountimport@example.com" }
                });
            var queue = XrmService.GetFirst(Entities.queue, Fields.queue_.name, "TESTQUEUE")
                ?? CreateTestRecord(Entities.queue, new Dictionary<string, object>
                {
                    { Fields.queue_.name, "TESTQUEUE" },
                    { Fields.queue_.emailaddress, "testscriptqueue@example.com" }
                });

            var email = new Entity(Entities.email);
            email.SetField(Fields.email_.subject, "Testing Import Email " + DateTime.Now.ToFileTime().ToString());
            email.AddFromParty(queue.LogicalName, queue.Id);
            email.AddToParty(contact1.LogicalName, contact1.Id);
            email.AddToParty(contact2.LogicalName, contact2.Id);
            email.AddToParty(account.LogicalName, account.Id);
            email.AddActivityParty(Fields.email_.to, "testtoemail@example.com");
            email = CreateAndRetrieve(email);

            XrmService.SetState(email, OptionSets.Email.ActivityStatus.Completed, OptionSets.Email.StatusReason.Sent);

            //okay lets xml export
            var exportXmlApplication = CreateAndLoadTestApplication<ExportXmlModule>();

            var instance = new ExportXmlRequest();
            instance.IncludeNNRelationshipsBetweenEntities = true;
            instance.Folder = new Folder(TestingFolder);
            instance.RecordTypesToExport = new[]
            {
                new ExportRecordType()
                {
                    Type = ExportType.SpecificRecords,
                    RecordType = new RecordType(Entities.email, Entities.email),
                    SpecificRecordsToExport = new LookupSetting[]
                    {
                        new LookupSetting()
                        {
                                Record = new Lookup(email.LogicalName, email.Id.ToString(), email.GetStringField(Fields.email_.subject))
                        }
                    }
                }
            };

            var exportXmlResponse = exportXmlApplication.NavigateAndProcessDialog<ExportXmlModule, ExportXmlDialog, ExportXmlResponse>(instance);
            Assert.IsFalse(exportXmlResponse.HasError);

            //okay lets delete the email then import it
            XrmService.Delete(email);

            //do an xml import
            var importXmlApplication = CreateAndLoadTestApplication<ImportXmlModule>();

            var importRequest = new ImportXmlRequest
            {
                Folder = new Folder(TestingFolder)
            };

            var importXmlResponse = importXmlApplication.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(importXmlResponse.HasError);

            Assert.AreEqual(1, importXmlResponse.ImportSummary.Count());
            Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.Created == 1));
            Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.Updated == 0));
            Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.NoChange == 0));
            Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.Errors == 0));

            var importedEmail = XrmService.Retrieve(email.LogicalName, email.Id);

            Assert.AreEqual(OptionSets.Email.StatusReason.Sent, importedEmail.GetOptionSetValue(Fields.email_.statuscode));
            Assert.IsTrue(importedEmail.GetActivityParties(Fields.email_.from).Count() == 1);
            Assert.IsTrue(importedEmail.GetActivityParties(Fields.email_.from).Count(p => Entities.queue == p.GetLookupType(Fields.activityparty_.partyid)) == 1);
            Assert.IsTrue(importedEmail.GetActivityParties(Fields.email_.to).Count() == 4);
            Assert.IsTrue(importedEmail.GetActivityParties(Fields.email_.to).Count(p => Entities.contact == p.GetLookupType(Fields.activityparty_.partyid)) == 2);
            Assert.IsTrue(importedEmail.GetActivityParties(Fields.email_.to).Count(p => Entities.account == p.GetLookupType(Fields.activityparty_.partyid)) == 1);
            Assert.IsTrue(importedEmail.GetActivityParties(Fields.email_.to).Count(p => null == p.GetField(Fields.activityparty_.partyid) && p.GetStringField(Fields.activityparty_.addressused) == "testtoemail@example.com") == 1);

            importXmlResponse = importXmlApplication.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(importXmlResponse.HasError);

            Assert.AreEqual(1, importXmlResponse.ImportSummary.Count());
            Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.Created == 0));
            Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.Updated == 0));
            Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.NoChange == 1));
            Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.Errors == 0));

            importedEmail = XrmService.Retrieve(email.LogicalName, email.Id);
            XrmService.SetState(importedEmail, OptionSets.Email.ActivityStatus.Open);

            var toParties = importedEmail.GetActivityParties(Fields.email_.to);
            importedEmail.SetField(Fields.email_.to, toParties.Skip(1).ToArray());
            importedEmail = UpdateFieldsAndRetreive(importedEmail, Fields.email_.to);

            importXmlResponse = importXmlApplication.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(importXmlResponse.HasError);

            Assert.AreEqual(1, importXmlResponse.ImportSummary.Count());
            Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.Created == 0));
            Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.Updated == 1));
            Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.NoChange == 0));
            Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.Errors == 0));

            importedEmail = XrmService.Retrieve(email.LogicalName, email.Id);

            Assert.IsTrue(importedEmail.GetActivityParties(Fields.email_.from).Count() == 1);
            Assert.IsTrue(importedEmail.GetActivityParties(Fields.email_.from).Count(p => Entities.queue == p.GetLookupType(Fields.activityparty_.partyid)) == 1);
            Assert.IsTrue(importedEmail.GetActivityParties(Fields.email_.to).Count() == 4);
            Assert.IsTrue(importedEmail.GetActivityParties(Fields.email_.to).Count(p => Entities.contact == p.GetLookupType(Fields.activityparty_.partyid)) == 2);
            Assert.IsTrue(importedEmail.GetActivityParties(Fields.email_.to).Count(p => Entities.account == p.GetLookupType(Fields.activityparty_.partyid)) == 1);
            Assert.IsTrue(importedEmail.GetActivityParties(Fields.email_.to).Count(p => null == p.GetField(Fields.activityparty_.partyid) && p.GetStringField(Fields.activityparty_.addressused) == "testtoemail@example.com") == 1);

        }

        [DeploymentItem(@"Files\TestImportMultiples.xlsx")]
        [TestMethod]
        public void DeploymentImportMultipleCachingTest()
        {
            //this script added when refactored to use execute multiples
            //so that a script covered resolving matches both with and without
            //caching target records
            //generally the scripts use a small number of records
            //which doesnt exceed the standard cache threshold
            //this uses a caching and non changing threshold for
            //a spreadsheet which contain various lookup fields
            //including circular and multi type lookups

            //runs through for both excel and xml imports
            //including where ids do and dont match

            PrepareTests();
            var cacheLimits = new[] { 5, 50 };
            var setSize = 10;

            foreach (var cacheLimit in cacheLimits)
            {
                FileUtility.DeleteFiles(TestingFolder);
                FileUtility.DeleteSubFolders(TestingFolder);

                var workFolder = TestingFolder + @"\TestImportMultiples";
                FileUtility.CheckCreateFolder(workFolder);
                var sourceExcelFile = Path.Combine(workFolder, @"TestImportMultiples.xlsx");
                File.Copy(@"TestImportMultiples.xlsx", sourceExcelFile);

                DeleteAll(Entities.account);
                DeleteAll(Entities.jmcg_testentity);
                DeleteAll(Entities.contact);

                var importExcelApp = CreateAndLoadTestApplication<ImportExcelModule>();
                importExcelApp.AddModule<SavedRequestModule>();
                ClearSavedRequests<ImportExcelModule, ImportExcelDialog>(importExcelApp);

                //navigate to the dialog
                var dialog = importExcelApp.NavigateToDialog<ImportExcelModule, ImportExcelDialog>();
                var entryViewmodel = importExcelApp.GetSubObjectEntryViewModel(dialog);
                //select the excel file with the errors and submit form
                entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);
                entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.ExecuteMultipleSetSize)).ValueObject = setSize;
                entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.TargetCacheLimit)).ValueObject = cacheLimit;
                Assert.IsTrue(entryViewmodel.Validate());
                entryViewmodel.SaveButtonViewModel.Invoke();

                var importExcelResponse = dialog.CompletionItem as ImportExcelResponse;
                if (importExcelResponse.HasError)
                    Assert.Fail(importExcelResponse.GetResponseItemsWithError().First().Exception.XrmDisplayString());

                Assert.IsTrue(importExcelResponse.ImportSummary.All(i => i.Created > 0));
                Assert.IsTrue(importExcelResponse.ImportSummary.All(i => i.Updated == 0));
                Assert.IsTrue(importExcelResponse.ImportSummary.All(i => i.NoChange == 0));
                Assert.IsTrue(importExcelResponse.ImportSummary.All(i => i.Errors == 0));

                ValidateimportedData();

                //lets do another and verify no errors or changes
                dialog = importExcelApp.NavigateToDialog<ImportExcelModule, ImportExcelDialog>();
                entryViewmodel = importExcelApp.GetSubObjectEntryViewModel(dialog);
                //select the excel file with the errors and submit form
                entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);
                entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.ExecuteMultipleSetSize)).ValueObject = setSize;
                entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.TargetCacheLimit)).ValueObject = cacheLimit;
                Assert.IsTrue(entryViewmodel.Validate());
                entryViewmodel.SaveButtonViewModel.Invoke();

                importExcelResponse = dialog.CompletionItem as ImportExcelResponse;
                if (importExcelResponse.HasError)
                    Assert.Fail(importExcelResponse.GetResponseItemsWithError().First().Exception.XrmDisplayString());

                Assert.IsTrue(importExcelResponse.ImportSummary.All(i => i.Created == 0));
                Assert.IsTrue(importExcelResponse.ImportSummary.All(i => i.Updated == 0));
                Assert.IsTrue(importExcelResponse.ImportSummary.All(i => i.NoChange > 0));
                Assert.IsTrue(importExcelResponse.ImportSummary.All(i => i.Errors == 0));

                ValidateimportedData();

                //okay lets do an xml export
                var exportXmlApplication = CreateAndLoadTestApplication<ExportXmlModule>();

                var instance = new ExportXmlRequest();
                instance.IncludeNNRelationshipsBetweenEntities = true;
                instance.Folder = new Folder(TestingFolder);
                instance.RecordTypesToExport = new[]
                {
                    new ExportRecordType()
                    {
                        Type = ExportType.AllRecords,
                        RecordType = new RecordType(Entities.account, Entities.account),
                    },
                    new ExportRecordType()
                    {
                        Type = ExportType.AllRecords,
                        RecordType = new RecordType(Entities.contact, Entities.contact),
                    },
                    new ExportRecordType()
                    {
                        Type = ExportType.AllRecords,
                        RecordType = new RecordType(Entities.jmcg_testentity, Entities.jmcg_testentity),
                    },
                };

                var exportXmlResponse = exportXmlApplication.NavigateAndProcessDialog<ExportXmlModule, ExportXmlDialog, ExportXmlResponse>(instance);
                Assert.IsFalse(exportXmlResponse.HasError);

                //do an xml import
                var importXmlApplication = CreateAndLoadTestApplication<ImportXmlModule>();

                var importRequest = new ImportXmlRequest
                {
                    Folder = new Folder(TestingFolder)
                };

                var importXmlResponse = importXmlApplication.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
                Assert.IsFalse(importXmlResponse.HasError);

                Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.Created == 0));
                Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.Updated == 0));
                Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.NoChange > 0));
                Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.Errors == 0));

                ValidateimportedData();

                //do another xml import after deleting the data and recreating with different ids (from excel import)

                DeleteAll(Entities.account);
                DeleteAll(Entities.jmcg_testentity);
                DeleteAll(Entities.contact);

                importExcelApp = CreateAndLoadTestApplication<ImportExcelModule>();
                importExcelApp.AddModule<SavedRequestModule>();
                ClearSavedRequests<ImportExcelModule, ImportExcelDialog>(importExcelApp);

                dialog = importExcelApp.NavigateToDialog<ImportExcelModule, ImportExcelDialog>();
                entryViewmodel = importExcelApp.GetSubObjectEntryViewModel(dialog);
                entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);
                entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.ExecuteMultipleSetSize)).ValueObject = setSize;
                entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.TargetCacheLimit)).ValueObject = cacheLimit;
                Assert.IsTrue(entryViewmodel.Validate());
                entryViewmodel.SaveButtonViewModel.Invoke();

                importExcelResponse = dialog.CompletionItem as ImportExcelResponse;
                if (importExcelResponse.HasError)
                    Assert.Fail(importExcelResponse.GetResponseItemsWithError().First().Exception.XrmDisplayString());

                Assert.IsTrue(importExcelResponse.ImportSummary.All(i => i.Created > 0));
                Assert.IsTrue(importExcelResponse.ImportSummary.All(i => i.Updated == 0));
                Assert.IsTrue(importExcelResponse.ImportSummary.All(i => i.NoChange == 0));
                Assert.IsTrue(importExcelResponse.ImportSummary.All(i => i.Errors == 0));

                ValidateimportedData();

                importXmlApplication = CreateAndLoadTestApplication<ImportXmlModule>();

                importRequest = new ImportXmlRequest
                {
                    Folder = new Folder(TestingFolder)
                };

                importXmlResponse = importXmlApplication.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
                Assert.IsFalse(importXmlResponse.HasError);

                Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.Created == 0));
                Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.Updated == 0));
                Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.NoChange > 0));
                Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.Errors == 0));

                ValidateimportedData();

                //okay finally lets delete half the records just for inclusion of a parital import

                //xml import half deleted

                DeleteHalfTheRecords();

                importXmlApplication = CreateAndLoadTestApplication<ImportXmlModule>();

                importRequest = new ImportXmlRequest
                {
                    Folder = new Folder(TestingFolder)
                };

                importXmlResponse = importXmlApplication.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
                Assert.IsFalse(importXmlResponse.HasError);

                Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.Created > 0));
                Assert.IsTrue(importXmlResponse.ImportSummary.Any(i => i.NoChange > 0));
                Assert.IsTrue(importXmlResponse.ImportSummary.All(i => i.Errors == 0));

                ValidateimportedData();

                //excel import half deleted

                importExcelApp = CreateAndLoadTestApplication<ImportExcelModule>();
                importExcelApp.AddModule<SavedRequestModule>();
                ClearSavedRequests<ImportExcelModule, ImportExcelDialog>(importExcelApp);

                DeleteHalfTheRecords();

                dialog = importExcelApp.NavigateToDialog<ImportExcelModule, ImportExcelDialog>();
                entryViewmodel = importExcelApp.GetSubObjectEntryViewModel(dialog);
                entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);
                entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.ExecuteMultipleSetSize)).ValueObject = setSize;
                entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.TargetCacheLimit)).ValueObject = cacheLimit;
                Assert.IsTrue(entryViewmodel.Validate());
                entryViewmodel.SaveButtonViewModel.Invoke();

                importExcelResponse = dialog.CompletionItem as ImportExcelResponse;
                if (importExcelResponse.HasError)
                    Assert.Fail(importExcelResponse.GetResponseItemsWithError().First().Exception.XrmDisplayString());

                Assert.IsTrue(importExcelResponse.ImportSummary.All(i => i.Created > 0));
                Assert.IsTrue(importExcelResponse.ImportSummary.Any(i => i.NoChange > 0));
                Assert.IsTrue(importExcelResponse.ImportSummary.All(i => i.Errors == 0));

                ValidateimportedData();

                DeleteAll(Entities.account);
                DeleteAll(Entities.jmcg_testentity);
                DeleteAll(Entities.contact);
            }
        }

        private void DeleteHalfTheRecords()
        {
            var types = new[]
            {
                Entities.account,
                Entities.contact,
                Entities.jmcg_testentity
            };
            foreach(var type in types)
            {
                var records = XrmRecordService.RetrieveAll(type, null);
                var deletionResponse = XrmRecordService.DeleteMultiple(records.Take(records.Count() / 2).ToArray());
                Assert.IsFalse(deletionResponse.Any());
            }
        }

        private void ValidateimportedData()
        {
            var accounts = XrmRecordService.RetrieveAll(Entities.account, null);
            var contacts = XrmRecordService.RetrieveAll(Entities.contact, null);
            var testEntities = XrmRecordService.RetrieveAll(Entities.jmcg_testentity, null);
            var accountTestEntityAssociations = XrmRecordService.RetrieveAll(Relationships.account_.jmcg_testentity_account.EntityName, null);
            Assert.AreEqual(32, accounts.Count());
            Assert.AreEqual(32, accounts.Count(a => a.GetLookupId(Fields.account_.primarycontactid) != null));
            Assert.AreEqual(7, accounts.Count(a => a.GetLookupId(Fields.account_.parentaccountid) != null));
            Assert.AreEqual(37, contacts.Count());
            Assert.AreEqual(32, contacts.Count(a => a.GetLookupType(Fields.contact_.parentcustomerid) == Entities.account));
            Assert.AreEqual(3, contacts.Count(a => a.GetLookupType(Fields.contact_.parentcustomerid) == Entities.contact));
            Assert.AreEqual(8, testEntities.Count());
            Assert.AreEqual(34, accountTestEntityAssociations.Count());
        }
    }
}