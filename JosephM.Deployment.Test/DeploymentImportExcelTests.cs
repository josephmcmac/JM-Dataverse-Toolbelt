using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Deployment.ImportExcel;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace JosephM.Deployment.Test
{
    [TestClass]
    public class DeploymentImportExcelTests : XrmModuleTest
    {
        [DeploymentItem(@"Files\TestExcelImportAccountAndContact.xlsx")]
        [TestMethod]
        public void DeploymentImportExcelBasicTest()
        {
            //imports an excel with 1 contact linked to 1 account
            PrepareTests();
            DeleteAll(Entities.account);
            DeleteAll(Entities.contact);

            var workFolder = TestingFolder + @"\ExcelImportScript";
            FileUtility.CheckCreateFolder(workFolder);
            var sourceExcelFile = Path.Combine(workFolder, @"TestExcelImportAccountAndContact.xlsx");
            File.Copy(@"TestExcelImportAccountAndContact.xlsx", sourceExcelFile);

            var app = CreateAndLoadTestApplication<ImportExcelModule>();

            var entryViewmodel = app.NavigateToDialogModuleEntryForm<ImportExcelModule, ImportExcelDialog>();
            entryViewmodel.GetBooleanFieldFieldViewModel(nameof(ImportExcelRequest.MaskEmails)).Value = true;

            //select the excel file
            entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);
            
            //okay on change trigger should have fired and populated mappings on contact
            var mappingGrid = entryViewmodel.GetEnumerableFieldViewModel(nameof(ImportExcelRequest.Mappings));
            var contactSource = mappingGrid.GridRecords.First(r => r.GetRecordTypeFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.SourceTab)).Value.Key.ToLower().Contains("contact"));
            Assert.AreEqual(Entities.contact, contactSource.GetRecordTypeFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.TargetType)).Value?.Key);

            contactSource.EditRowViewModel.Command.Execute();
            var contactMapEntryModel = app.GetSubObjectEntryViewModel(entryViewmodel);
            var fieldMappingGrid = contactMapEntryModel.GetEnumerableFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.Mappings));
            //verify the autmapped field 
            var fullNameSource = fieldMappingGrid.GridRecords.First(r => r.GetRecordFieldFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.ExcelImportFieldMapping.SourceColumn)).Value.Key.ToLower().Contains("full name"));
            Assert.AreEqual(Fields.contact_.fullname, fullNameSource.GetRecordFieldFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.ExcelImportFieldMapping.TargetField)).Value?.Key);
            //set mapping for the company field
            var companySource = fieldMappingGrid.GridRecords.First(r => r.GetRecordFieldFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.ExcelImportFieldMapping.SourceColumn)).Value.Key.ToLower().Contains("contact company"));
            companySource.GetRecordFieldFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.ExcelImportFieldMapping.TargetField)).Value = new RecordField(Fields.contact_.parentcustomerid, Fields.contact_.parentcustomerid);
            //remove unmapped fields
            foreach (var item in fieldMappingGrid.DynamicGridViewModel.GridRecords.ToArray())
            {
                if (item.GetFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.ExcelImportFieldMapping.TargetField)).ValueObject == null)
                    item.DeleteRowViewModel.Command.Execute();
            }
            contactMapEntryModel.SaveButtonViewModel.Command.Execute();
            Assert.IsFalse(entryViewmodel.ChildForms.Any());

            //map the other tab to accounts
            var accountSource = mappingGrid.GridRecords.First(r => r.GetRecordTypeFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.SourceTab)).Value.Key.ToLower().Contains("compan"));
            accountSource.GetRecordTypeFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.TargetType)).Value = new RecordType(Entities.account, Entities.account);
            accountSource.EditRowViewModel.Command.Execute();
            var accountMapEntryModel = app.GetSubObjectEntryViewModel(entryViewmodel);
            fieldMappingGrid = accountMapEntryModel.GetEnumerableFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.Mappings));
            //map the account name
            var nameSource = fieldMappingGrid.GridRecords.First(r => r.GetRecordFieldFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.ExcelImportFieldMapping.SourceColumn)).Value.Key.ToLower().Contains("name"));
            nameSource.GetRecordFieldFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.ExcelImportFieldMapping.TargetField)).Value = new RecordField(Fields.account_.name, Fields.account_.name);
            accountMapEntryModel.SaveButtonViewModel.Command.Execute();
            Assert.IsFalse(entryViewmodel.ChildForms.Any());

            //remove unmapped tabs
            foreach(var item in mappingGrid.DynamicGridViewModel.GridRecords.ToArray())
            {
                if (item.GetFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.TargetType)).ValueObject == null)
                    item.DeleteRowViewModel.Command.Execute();
            }

            //trigger the import
            entryViewmodel.SaveButtonViewModel.Command.Execute();

            var dialog = app.GetNavigatedDialog<ImportExcelDialog>();
            var completionScreen = dialog.CompletionItem as ImportExcelResponse;
            if (completionScreen.HasError)
                Assert.Fail(completionScreen.GetResponseItemsWithError().First().Exception.XrmDisplayString());

            //verify the account and contact created
            var accounts = XrmService.RetrieveAllEntityType(Entities.account);
            var contacts = XrmService.RetrieveAllEntityType(Entities.contact);
            Assert.AreEqual(1, accounts.Count());
            Assert.AreEqual(1, contacts.Count());

            //verify the contact has names populated and linked to the account
            foreach (var contact in contacts)
            {
                Assert.AreEqual(accounts.First().Id, contact.GetLookupGuid(Fields.contact_.parentcustomerid));
                Assert.IsNotNull(contact.GetStringField(Fields.contact_.firstname));
                Assert.IsNotNull(contact.GetStringField(Fields.contact_.lastname));
                if(contact.GetStringField(Fields.contact_.emailaddress1) != null)
                    Assert.IsTrue(contact.GetStringField(Fields.contact_.emailaddress1).Contains("@fake"));
                //this one is date only
                Assert.AreEqual(new DateTime(1980, 11, 15), contact.GetDateTimeField(Fields.contact_.birthdate));
                //this one is user local
                Assert.AreEqual(new DateTime(1980, 11, 15), contact.GetDateTimeField(Fields.contact_.lastonholdtime).Value.ToLocalTime());
            }
        }

        [DeploymentItem(@"Files\TestExcelImportContacts.xlsx")]
        [TestMethod]
        public void DeploymentImportExcelWithDistinctTest()
        {
            //imports an excel with contact spreadsheet including account column
            PrepareTests();
            DeleteAll(Entities.account);
            DeleteAll(Entities.contact);

            var workFolder = TestingFolder + @"\ExcelImportScript";
            FileUtility.CheckCreateFolder(workFolder);
            var sourceExcelFile = Path.Combine(workFolder, @"TestExcelImportContacts.xlsx");
            File.Copy(@"TestExcelImportContacts.xlsx", sourceExcelFile);

            var app = CreateAndLoadTestApplication<ImportExcelModule>();

            var entryViewmodel = app.NavigateToDialogModuleEntryForm<ImportExcelModule, ImportExcelDialog>();
            //select the excel file
            entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);

            //okay on change trigger should have fired and populated mappings on contact
            var mappingGrid = entryViewmodel.GetEnumerableFieldViewModel(nameof(ImportExcelRequest.Mappings));
            var contactSource = mappingGrid.GridRecords.First(r => r.GetRecordTypeFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.SourceTab)).Value.Key.ToLower().Contains("contact"));
            Assert.AreEqual(Entities.contact, contactSource.GetRecordTypeFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.TargetType)).Value?.Key);


            //add another map to create accounts for the parentcustomerid column
            mappingGrid.AddRow();
            var accountTarget = mappingGrid.GridRecords.First(r => r.GetRecordTypeFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.SourceTab)).Value == null);

            accountTarget.GetRecordTypeFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.SourceTab)).Value = accountTarget.GetRecordTypeFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.SourceTab)).ItemsSource.First();
            accountTarget.GetRecordTypeFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.TargetType)).Value = new RecordType(Entities.account, Entities.account);
            accountTarget.EditRowViewModel.Command.Execute();
            var accountMapEntryModel = app.GetSubObjectEntryViewModel(entryViewmodel);
            var fieldMappingGrid = accountMapEntryModel.GetEnumerableFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.Mappings));
            
            //map the account name
            fieldMappingGrid.AddRow();
            fieldMappingGrid.DynamicGridViewModel.GridRecords.First().GetRecordFieldFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.ExcelImportFieldMapping.SourceColumn)).Value = fieldMappingGrid.DynamicGridViewModel.GridRecords.First().GetRecordFieldFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.ExcelImportFieldMapping.SourceColumn)).ItemsSource.First(p => p.Key.Contains("parent"));
            fieldMappingGrid.DynamicGridViewModel.GridRecords.First().GetRecordFieldFieldViewModel(nameof(ImportExcelRequest.ExcelImportTabMapping.ExcelImportFieldMapping.TargetField)).Value = new RecordField(Fields.account_.name, Fields.account_.name);
            accountMapEntryModel.SaveButtonViewModel.Command.Execute();
            Assert.IsFalse(entryViewmodel.ChildForms.Any());

            //trigger the import
            entryViewmodel.SaveButtonViewModel.Command.Execute();

            var dialog = app.GetNavigatedDialog<ImportExcelDialog>();
            var completionScreen = dialog.CompletionItem as ImportExcelResponse;
            if (completionScreen.HasError)
                Assert.Fail(completionScreen.GetResponseItemsWithError().First().Exception.XrmDisplayString());

            //veirfy the account and contact created
            var accounts = XrmService.RetrieveAllEntityType(Entities.account);
            var contacts = XrmService.RetrieveAllEntityType(Entities.contact);
            Assert.AreEqual(2, accounts.Count());
            Assert.AreEqual(3, contacts.Count());

            //okay lets run a second import which will only be for the contacts and verify the same and no errors
            entryViewmodel = app.NavigateToDialogModuleEntryForm<ImportExcelModule, ImportExcelDialog>();
            entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);
            mappingGrid = entryViewmodel.GetEnumerableFieldViewModel(nameof(ImportExcelRequest.Mappings));
            Assert.AreEqual(1, mappingGrid.DynamicGridViewModel.GridRecords.Count);

            //trigger the import
            entryViewmodel.SaveButtonViewModel.Command.Execute();

            dialog = app.GetNavigatedDialog<ImportExcelDialog>();
            completionScreen = dialog.CompletionItem as ImportExcelResponse;
            if (completionScreen.HasError)
                Assert.Fail(completionScreen.GetResponseItemsWithError().First().Exception.XrmDisplayString());

            //verify still same number
            accounts = XrmService.RetrieveAllEntityType(Entities.account);
            contacts = XrmService.RetrieveAllEntityType(Entities.contact);
            Assert.AreEqual(2, accounts.Count());
            Assert.AreEqual(3, contacts.Count());

            //okay lets run another import only for the contacts
            //with match by name false
            //in this case we expect the records to be created again
            entryViewmodel = app.NavigateToDialogModuleEntryForm<ImportExcelModule, ImportExcelDialog>();
            entryViewmodel.GetBooleanFieldFieldViewModel(nameof(ImportExcelRequest.MatchRecordsByName)).ValueObject = false;
            entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);
            mappingGrid = entryViewmodel.GetEnumerableFieldViewModel(nameof(ImportExcelRequest.Mappings));
            Assert.AreEqual(1, mappingGrid.DynamicGridViewModel.GridRecords.Count);

            //trigger the import
            entryViewmodel.SaveButtonViewModel.Command.Execute();

            dialog = app.GetNavigatedDialog<ImportExcelDialog>();
            completionScreen = dialog.CompletionItem as ImportExcelResponse;
            if (completionScreen.HasError)
                Assert.Fail(completionScreen.GetResponseItemsWithError().First().Exception.XrmDisplayString());

            //verify still same number
            accounts = XrmService.RetrieveAllEntityType(Entities.account);
            contacts = XrmService.RetrieveAllEntityType(Entities.contact);
            Assert.AreEqual(2, accounts.Count());
            Assert.AreEqual(6, contacts.Count());
        }

        [DeploymentItem(@"Files\TestExcelImportAssociations.xlsx")]
        [TestMethod]
        public void DeploymentImportExcelWithAssociationTest()
        {
            //imports an excel with accounts linked to test entities
            PrepareTests();
            DeleteAll(Entities.account);
            DeleteAll(Entities.jmcg_testentity);

            var workFolder = TestingFolder + @"\ExcelImportScript";
            FileUtility.CheckCreateFolder(workFolder);
            var sourceExcelFile = Path.Combine(workFolder, @"TestExcelImportAssociations.xlsx");
            File.Copy(@"TestExcelImportAssociations.xlsx", sourceExcelFile);

            var app = CreateAndLoadTestApplication<ImportExcelModule>();

            var entryViewmodel = app.NavigateToDialogModuleEntryForm<ImportExcelModule, ImportExcelDialog>();
            //select the excel file
            entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);

            //okay on change trigger should have fired and populated all the required mappings
            
            //so lets just trigger the import
            entryViewmodel.SaveButtonViewModel.Command.Execute();

            var dialog = app.GetNavigatedDialog<ImportExcelDialog>();
            var completionScreen = dialog.CompletionItem as ImportExcelResponse;
            if (completionScreen.HasError)
                Assert.Fail(completionScreen.GetResponseItemsWithError().First().Exception.XrmDisplayString());

            //verify the account and contact created
            var accounts = XrmService.RetrieveAllEntityType(Entities.account);
            var testEntities = XrmService.RetrieveAllEntityType(Entities.jmcg_testentity);
            Assert.AreEqual(2, accounts.Count());
            Assert.AreEqual(2, testEntities.Count());

            var associations = XrmService.RetrieveAllEntityType(Relationships.account_.jmcg_testentity_account.EntityName);
            Assert.AreEqual(3, associations.Count());

            //okay lets run a second import which will only be for the contacts and verify the same and no errors
            entryViewmodel = app.NavigateToDialogModuleEntryForm<ImportExcelModule, ImportExcelDialog>();
            entryViewmodel.GetFieldViewModel(nameof(ImportExcelRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);

            //trigger the import
            entryViewmodel.SaveButtonViewModel.Command.Execute();

            dialog = app.GetNavigatedDialog<ImportExcelDialog>();
            completionScreen = dialog.CompletionItem as ImportExcelResponse;
            if (completionScreen.HasError)
                Assert.Fail(completionScreen.GetResponseItemsWithError().First().Exception.XrmDisplayString());

            //verify still same number
            accounts = XrmService.RetrieveAllEntityType(Entities.account);
            testEntities = XrmService.RetrieveAllEntityType(Entities.jmcg_testentity);
            Assert.AreEqual(2, accounts.Count());
            Assert.AreEqual(2, testEntities.Count());

            associations = XrmService.RetrieveAllEntityType(Relationships.account_.jmcg_testentity_account.EntityName);
            Assert.AreEqual(3, associations.Count());
        }
    }
}