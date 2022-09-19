using JosephM.Application.Desktop.Module.SavedRequests;
using JosephM.Application.Desktop.Test;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Xrm;
using JosephM.Xrm.DataImportExport.MappedImport;
using JosephM.Xrm.DataImportExport.XmlExport;
using JosephM.Xrm.DataImportExport.XmlImport;
using JosephM.Xrm.ExcelImport;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace JosephM.Deployment.Test
{
    [TestClass]
    public class ExcelImportTests : XrmModuleTest
    {
        [DeploymentItem(@"Files\TestExcelImportValidationErrors.xlsx")]
        [TestMethod]
        public void ExcelImportValidationTest()
        {
            //script through excel import where there are parse field errors
            //for an invalid picklist value and exceeding the max string length
            //in this case after submitting the form the validation/parse errors should 
            //display and allow moving back to entry or proceeding anyway
            PrepareTests();
            DeleteAll(Entities.account);

            var workFolder = TestingFolder + @"\ExcelImportScript";
            FileUtility.CheckCreateFolder(workFolder);
            var sourceExcelFile = Path.Combine(workFolder, @"TestExcelImportValidationErrors.xlsx");
            File.Copy(@"TestExcelImportValidationErrors.xlsx", sourceExcelFile);

            var app = CreateAndLoadTestApplication<ExcelImportModule>();
            app.AddModule<SavedRequestModule>();

            //there was an autoload so lets delete it
            ClearSavedRequests(app);

            //navigate to the dialog
            var dialog = app.NavigateToDialog<ExcelImportModule, ExcelImportDialog>();
            var entryViewmodel = app.GetSubObjectEntryViewModel(dialog);
            //select the excel file with the errors and submit form
            entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);
            entryViewmodel.SaveButtonViewModel.Invoke();

            //check validation results displayed
            var validationResults = dialog.Controller.UiItems.First() as ObjectDisplayViewModel;
            Assert.IsNotNull(validationResults);
            Assert.IsTrue(validationResults.GetObject() is ParseIntoEntitiesResponse);

            //navigate back to entry form
            validationResults.BackButtonViewModel.Invoke();
            entryViewmodel = dialog.Controller.UiItems.First() as ObjectEntryViewModel;
            Assert.IsNotNull(entryViewmodel);
            Assert.IsTrue(entryViewmodel.GetObject() is ExcelImportRequest);

            //submit again
            entryViewmodel.SaveButtonViewModel.Invoke();
            validationResults = dialog.Controller.UiItems.First() as ObjectDisplayViewModel;
            Assert.IsNotNull(validationResults);
            Assert.IsTrue(validationResults.GetObject() is ParseIntoEntitiesResponse);

            //at validation display proceed anyway
            validationResults.SaveButtonViewModel.Invoke();
            var completionScreen = app.GetCompletionViewModel(dialog);
            var ExcelImportResponse = completionScreen.GetObject() as ExcelImportResponse;
            Assert.IsNotNull(ExcelImportResponse);
            Assert.IsTrue(ExcelImportResponse.ResponseItems.Any());
        }

        private void ClearSavedRequests(TestApplication app)
        {
            var entryViewmodel = app.NavigateToDialogModuleEntryForm<ExcelImportModule, ExcelImportDialog>() as ObjectEntryViewModel;
            ClearSavedRequests(app, entryViewmodel);
        }

        [DeploymentItem(@"Files\AccountsWithNullCells.xlsx")]
        [TestMethod]
        public void ExcelImportIgnoreNullValuesTest()
        {
            PrepareTests();
            DeleteAll(Entities.account);

            RunIgnoreNullExcelImport(false);

            var accounts = XrmService.RetrieveAllEntityType(Entities.account);
            Assert.AreEqual(2, accounts.Count());

            var theJoker = accounts.First(a => a.GetStringField(Fields.account_.accountnumber) == "1234567");
            var theRiddler = accounts.First(a => a.GetStringField(Fields.account_.accountnumber) == "1234568");

            Assert.IsNotNull(theJoker.GetStringField(Fields.account_.description));
            Assert.IsNull(theJoker.GetStringField(Fields.account_.address1_city));

            Assert.IsNull(theRiddler.GetStringField(Fields.account_.description));
            Assert.IsNotNull(theRiddler.GetStringField(Fields.account_.address1_city));

            foreach(var account in accounts)
            {
                account.SetField(Fields.account_.description, "I Updated");
                account.SetField(Fields.account_.address1_city, "I Updated");
                XrmService.Update(account, new[] { Fields.account_.description, Fields.account_.address1_city });
            }

            RunIgnoreNullExcelImport(true);

            accounts = XrmService.RetrieveAllEntityType(Entities.account);
            Assert.AreEqual(2, accounts.Count());

            theJoker = accounts.First(a => a.GetStringField(Fields.account_.accountnumber) == "1234567");
            theRiddler = accounts.First(a => a.GetStringField(Fields.account_.accountnumber) == "1234568");

            Assert.IsNotNull(theJoker.GetStringField(Fields.account_.description));
            Assert.AreNotEqual("I Updated", theJoker.GetStringField(Fields.account_.description));
            Assert.AreEqual("I Updated", theJoker.GetStringField(Fields.account_.address1_city));

            Assert.AreEqual("I Updated", theRiddler.GetStringField(Fields.account_.description));
            Assert.IsNotNull(theRiddler.GetStringField(Fields.account_.address1_city));
            Assert.AreNotEqual("I Updated", theRiddler.GetStringField(Fields.account_.address1_city));

            RunIgnoreNullExcelImport(false);

            accounts = XrmService.RetrieveAllEntityType(Entities.account);
            Assert.AreEqual(2, accounts.Count());

            theJoker = accounts.First(a => a.GetStringField(Fields.account_.accountnumber) == "1234567");
            theRiddler = accounts.First(a => a.GetStringField(Fields.account_.accountnumber) == "1234568");

            Assert.IsNotNull(theJoker.GetStringField(Fields.account_.description));
            Assert.IsNull(theJoker.GetStringField(Fields.account_.address1_city));

            Assert.IsNull(theRiddler.GetStringField(Fields.account_.description));
            Assert.IsNotNull(theRiddler.GetStringField(Fields.account_.address1_city));
        }

        private void RunIgnoreNullExcelImport(bool ignoreNulls)
        {
            var workFolder = TestingFolder + @"\ExcelImportScript";
            FileUtility.CheckCreateFolder(workFolder);
            var sourceExcelFile = Path.Combine(workFolder, @"AccountsWithNullCells.xlsx");
            File.Copy(@"AccountsWithNullCells.xlsx", sourceExcelFile, overwrite: true);

            var app = CreateAndLoadTestApplication<ExcelImportModule>();
            var dialog = app.NavigateToDialog<ExcelImportModule, ExcelImportDialog>();
            var entryViewmodel = app.GetSubObjectEntryViewModel(dialog);
            entryViewmodel.GetBooleanFieldFieldViewModel(nameof(ExcelImportRequest.IgnoreEmptyCells)).Value = ignoreNulls;

            //select the excel file
            entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);

            //okay on change trigger should have fired and populated mappings on contact
            //now add match key for account number
            var tabMappingsGrid = entryViewmodel.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.Mappings));
            var accountMap = tabMappingsGrid.DynamicGridViewModel.GridRecords.First();
            var keyMapsField = accountMap.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.AltMatchKeys));
            keyMapsField.EditButton.Command.Execute();
            var altMatchKeyEntryForm = entryViewmodel.ChildForms.First() as ObjectEntryViewModel;
            Assert.IsNotNull(altMatchKeyEntryForm);
            altMatchKeyEntryForm.LoadFormSections();
            var mapsField = altMatchKeyEntryForm.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.AltMatchKeys));
            mapsField.AddRow();
            var matchKeyField = mapsField.DynamicGridViewModel.GridRecords.First().GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportMatchKey.TargetField));
            Assert.IsTrue(matchKeyField.ItemsSource.Any());
            matchKeyField.Value = matchKeyField.ItemsSource.First(f => f.Key == Fields.account_.accountnumber);
            altMatchKeyEntryForm.SaveButtonViewModel.Invoke();
            Assert.IsFalse(entryViewmodel.ChildForms.Any());

            //run the import and verify response and account count
            entryViewmodel.SaveButtonViewModel.Invoke();
            var response = app.GetCompletionViewModel(dialog).GetObject() as ExcelImportResponse;
            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.GetResponseItemsWithError().Count());
        }

        [DeploymentItem(@"Files\AccountsWithKeys.xlsx")]
        [TestMethod]
        public void ExcelImportMatchKeysTest()
        {
            //okay this script imports a sheet using
            //account number as the match key

            //the initial import creates new records
            //though one of the five rows throws error due to missing account number (key)

            //the subsequent one only allows updates
            //and I delete one of the records imported to verify
            //it throws an extra error due to a missing key for update in the target

            PrepareTests();
            DeleteAll(Entities.account);

            var workFolder = TestingFolder + @"\ExcelImportScript";
            FileUtility.CheckCreateFolder(workFolder);
            var sourceExcelFile = Path.Combine(workFolder, @"AccountsWithKeys.xlsx");
            File.Copy(@"AccountsWithKeys.xlsx", sourceExcelFile);

            var app = CreateAndLoadTestApplication<ExcelImportModule>();
            var dialog = app.NavigateToDialog<ExcelImportModule, ExcelImportDialog>();
            var entryViewmodel = app.GetSubObjectEntryViewModel(dialog);
            entryViewmodel.GetBooleanFieldFieldViewModel(nameof(ExcelImportRequest.MaskEmails)).Value = true;

            //select the excel file
            entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);

            //okay on change trigger should have fired and populated mappings on contact
            //now add match key 
            var tabMappingsGrid = entryViewmodel.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.Mappings));
            var accountMap = tabMappingsGrid.DynamicGridViewModel.GridRecords.First();
            var keyMapsField = accountMap.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.AltMatchKeys));
            keyMapsField.EditButton.Command.Execute();
            var altMatchKeyEntryForm = entryViewmodel.ChildForms.First() as ObjectEntryViewModel;
            Assert.IsNotNull(altMatchKeyEntryForm);
            altMatchKeyEntryForm.LoadFormSections();
            var mapsField = altMatchKeyEntryForm.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.AltMatchKeys));
            mapsField.AddRow();
            var matchKeyField = mapsField.DynamicGridViewModel.GridRecords.First().GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportMatchKey.TargetField));
            Assert.IsTrue(matchKeyField.ItemsSource.Any());
            matchKeyField.Value = matchKeyField.ItemsSource.First(f => f.Key == Fields.account_.accountnumber);
            altMatchKeyEntryForm.SaveButtonViewModel.Invoke();
            Assert.IsFalse(entryViewmodel.ChildForms.Any());

            //run the import and verify response and account count
            entryViewmodel.SaveButtonViewModel.Invoke();
            var response = app.GetCompletionViewModel(dialog).GetObject() as ExcelImportResponse;
            Assert.IsNotNull(response);

            Assert.AreEqual(1, response.GetResponseItemsWithError().Count());
            var accounts = XrmService.RetrieveAllEntityType(Entities.account);
            Assert.AreEqual(4, accounts.Count());

            //delete one of the imported accounts
            XrmService.Delete(accounts.First());
            //second import - this one only allows updates
            dialog = app.NavigateToDialog<ExcelImportModule, ExcelImportDialog>();
            entryViewmodel = app.GetSubObjectEntryViewModel(dialog);
            entryViewmodel.GetBooleanFieldFieldViewModel(nameof(ExcelImportRequest.MaskEmails)).Value = true;
            entryViewmodel.GetBooleanFieldFieldViewModel(nameof(ExcelImportRequest.UpdateOnly)).Value = true;
            //select the excel file
            entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);

            //okay on change trigger should have fired and populated mappings on contact
            //now add match key 
            tabMappingsGrid = entryViewmodel.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.Mappings));
            accountMap = tabMappingsGrid.DynamicGridViewModel.GridRecords.First();
            keyMapsField = accountMap.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.AltMatchKeys));
            keyMapsField.EditButton.Command.Execute();
            altMatchKeyEntryForm = entryViewmodel.ChildForms.First() as ObjectEntryViewModel;
            Assert.IsNotNull(altMatchKeyEntryForm);
            altMatchKeyEntryForm.LoadFormSections();
            mapsField = altMatchKeyEntryForm.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.AltMatchKeys));
            mapsField.AddRow();
            matchKeyField = mapsField.DynamicGridViewModel.GridRecords.First().GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportMatchKey.TargetField));
            Assert.IsTrue(matchKeyField.ItemsSource.Any());
            matchKeyField.Value = matchKeyField.ItemsSource.First(f => f.Key == Fields.account_.accountnumber);
            altMatchKeyEntryForm.SaveButtonViewModel.Invoke();
            Assert.IsFalse(entryViewmodel.ChildForms.Any());

            //run the import and verify response and account count
            entryViewmodel.SaveButtonViewModel.Invoke();

            response = app.GetCompletionViewModel(dialog).GetObject() as ExcelImportResponse;
            Assert.IsNotNull(response);

            Assert.AreEqual(2, response.GetResponseItemsWithError().Count());
            accounts = XrmService.RetrieveAllEntityType(Entities.account);
            Assert.AreEqual(3, accounts.Count());
        }

        [DeploymentItem(@"Files\TestExcelImportAccountAndContact.xlsx")]
        [TestMethod]
        public void ExcelImportBasicTest()
        {
            //imports an excel with 1 contact linked to 1 account
            PrepareTests();
            DeleteAll(Entities.account);
            DeleteAll(Entities.contact);

            var workFolder = TestingFolder + @"\ExcelImportScript";
            FileUtility.CheckCreateFolder(workFolder);
            var sourceExcelFile = Path.Combine(workFolder, @"TestExcelImportAccountAndContact.xlsx");
            File.Copy(@"TestExcelImportAccountAndContact.xlsx", sourceExcelFile);

            var app = CreateAndLoadTestApplication<ExcelImportModule>();
            app.AddModule<SavedRequestModule>();
            ClearSavedRequests(app);

            var entryViewmodel = app.NavigateToDialogModuleEntryForm<ExcelImportModule, ExcelImportDialog>();
            entryViewmodel.GetBooleanFieldFieldViewModel(nameof(ExcelImportRequest.MaskEmails)).Value = true;

            //select the excel file
            entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);

            //okay on change trigger should have fired and populated mappings on contact
            var mappingGrid = entryViewmodel.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.Mappings));
            var contactSource = mappingGrid.GridRecords.First(r => r.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.SourceTab)).Value.Key.ToLower().Contains("contact"));
            Assert.AreEqual(Entities.contact, contactSource.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.TargetType)).Value?.Key);

            contactSource.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.Mappings)).EditButton.Command.Execute();
            var contactMapEntryModel = app.GetSubObjectEntryViewModel(entryViewmodel);
            var fieldMappingGrid = contactMapEntryModel.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.Mappings));
            //verify the autmapped field 
            var fullNameSource = fieldMappingGrid.GridRecords.First(r => r.GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.SourceColumn)).Value.Key.ToLower().Contains("full name"));
            Assert.AreEqual(Fields.contact_.fullname, fullNameSource.GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.TargetField)).Value?.Key);
            //set mapping for the company field
            var companySource = fieldMappingGrid.GridRecords.First(r => r.GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.SourceColumn)).Value.Key.ToLower().Contains("contact company"));
            companySource.GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.TargetField)).Value = new RecordField(Fields.contact_.parentcustomerid, Fields.contact_.parentcustomerid);
            //remove unmapped fields
            foreach (var item in fieldMappingGrid.DynamicGridViewModel.GridRecords.ToArray())
            {
                if (item.GetFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.TargetField)).ValueObject == null)
                    item.DeleteRowViewModel.Command.Execute();
            }


            contactMapEntryModel.SaveButtonViewModel.Command.Execute();
            Assert.IsFalse(entryViewmodel.ChildForms.Any());

            //map the other tab to accounts
            var accountSource = mappingGrid.GridRecords.First(r => r.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.SourceTab)).Value.Key.ToLower().Contains("compan"));
            accountSource.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.TargetType)).Value = new RecordType(Entities.account, Entities.account);
            accountSource.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.Mappings)).EditButton.Command.Execute();
            var accountMapEntryModel = app.GetSubObjectEntryViewModel(entryViewmodel);
            fieldMappingGrid = accountMapEntryModel.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.Mappings));
            //map the account name
            var nameSource = fieldMappingGrid.GridRecords.First(r => r.GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.SourceColumn)).Value.Key.ToLower().Contains("name"));
            nameSource.GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.TargetField)).Value = new RecordField(Fields.account_.name, Fields.account_.name);
            accountMapEntryModel.SaveButtonViewModel.Command.Execute();
            Assert.IsFalse(entryViewmodel.ChildForms.Any());

            //remove unmapped tabs
            foreach (var item in mappingGrid.DynamicGridViewModel.GridRecords.ToArray())
            {
                if (item.GetFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.TargetType)).ValueObject == null)
                    item.DeleteRowViewModel.Command.Execute();
            }

            ClearSavedRequests(app, entryViewmodel);
            //lets save the request with autoload so we can run again afterwards
            SaveWithAutoload(app, entryViewmodel);

            //trigger the import
            entryViewmodel.SaveButtonViewModel.Command.Execute();

            var dialog = app.GetNavigatedDialog<ExcelImportDialog>();
            var completionScreen = dialog.CompletionItem as ExcelImportResponse;
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
                if (contact.GetStringField(Fields.contact_.emailaddress1) != null)
                    Assert.IsTrue(contact.GetStringField(Fields.contact_.emailaddress1).Contains("@fake"));
                //this one is date only
                Assert.AreEqual(new DateTime(1980, 11, 15), contact.GetDateTimeField(Fields.contact_.birthdate));
                //this one is user local
                Assert.AreEqual(new DateTime(1980, 11, 15), contact.GetDateTimeField(Fields.contact_.lastonholdtime).Value.ToLocalTime());
            }

            //okay lets do a second import and verify no creates or updates
            entryViewmodel = app.NavigateToDialogModuleEntryForm<ExcelImportModule, ExcelImportDialog>();
            entryViewmodel.SaveButtonViewModel.Command.Execute();
            dialog = app.GetNavigatedDialog<ExcelImportDialog>();
            completionScreen = dialog.CompletionItem as ExcelImportResponse;
            if (completionScreen.HasError)
                Assert.Fail(completionScreen.GetResponseItemsWithError().First().Exception.XrmDisplayString());
            Assert.IsTrue(completionScreen.ImportSummary.All(i => i.Created == 0 && i.Updated == 0));
        }

        private static void SaveWithAutoload(TestApplication app, RecordEntryFormViewModel entryViewmodel)
        {
            var saveRequestButton = entryViewmodel.GetButton("SAVEREQUEST");
            saveRequestButton.Invoke();
            //enter and save details
            var saveRequestForm = app.GetSubObjectEntryViewModel(entryViewmodel);
            var detailsEntered = new SaveAndLoadFields()
            {
                Name = "ScriptSaveAutoload",
                Autoload = true
            };
            app.EnterAndSaveObject(detailsEntered, saveRequestForm);
            Assert.IsFalse(entryViewmodel.ChildForms.Any());
            Assert.IsFalse(entryViewmodel.LoadingViewModel.IsLoading);
        }

        [DeploymentItem(@"Files\TestExcelImportContacts.xlsx")]
        [TestMethod]
        public void ExcelImportWithDistinctTest()
        {
            //imports an excel with contact spreadsheet including account column
            PrepareTests();
            DeleteAll(Entities.account);
            DeleteAll(Entities.contact);

            var workFolder = TestingFolder + @"\ExcelImportScript";
            FileUtility.CheckCreateFolder(workFolder);
            var sourceExcelFile = Path.Combine(workFolder, @"TestExcelImportContacts.xlsx");
            File.Copy(@"TestExcelImportContacts.xlsx", sourceExcelFile);

            var app = CreateAndLoadTestApplication<ExcelImportModule>();
            app.AddModule<SavedRequestModule>();
            ClearSavedRequests(app);

            var entryViewmodel = app.NavigateToDialogModuleEntryForm<ExcelImportModule, ExcelImportDialog>();
            //select the excel file
            entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);

            //okay on change trigger should have fired and populated mappings on contact
            var mappingGrid = entryViewmodel.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.Mappings));
            var contactSource = mappingGrid.GridRecords.First(r => r.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.SourceTab)).Value.Key.ToLower().Contains("contact"));
            Assert.AreEqual(Entities.contact, contactSource.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.TargetType)).Value?.Key);


            //add another map to create accounts for the parentcustomerid column
            mappingGrid.AddRow();
            var accountTarget = mappingGrid.GridRecords.First(r => r.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.SourceTab)).Value == null);
            accountTarget.GetBooleanFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.IgnoreDuplicates)).Value = true;
            accountTarget.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.SourceTab)).Value = accountTarget.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.SourceTab)).ItemsSource.First();
            accountTarget.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.TargetType)).Value = new RecordType(Entities.account, Entities.account);
            accountTarget.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.Mappings)).EditButton.Command.Execute();
            var accountMapEntryModel = app.GetSubObjectEntryViewModel(entryViewmodel);
            var fieldMappingGrid = accountMapEntryModel.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.Mappings));
            
            //map the account name
            fieldMappingGrid.AddRow();
            fieldMappingGrid.DynamicGridViewModel.GridRecords.First().GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.SourceColumn)).Value = fieldMappingGrid.DynamicGridViewModel.GridRecords.First().GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.SourceColumn)).ItemsSource.First(p => p.Key.Contains("parent"));
            fieldMappingGrid.DynamicGridViewModel.GridRecords.First().GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.TargetField)).Value = new RecordField(Fields.account_.name, Fields.account_.name);
            accountMapEntryModel.SaveButtonViewModel.Command.Execute();
            Assert.IsFalse(entryViewmodel.ChildForms.Any());

            //trigger the import
            entryViewmodel.SaveButtonViewModel.Command.Execute();

            //since there are duplicates the validation is dsiplayed to the user so lets proceed through it
            var dialog = app.GetNavigatedDialog<ExcelImportDialog>();

            var completionViewModel = app.GetCompletionViewModel(dialog);
            Assert.IsNotNull(completionViewModel);
            var completionScreen = completionViewModel.GetObject() as ExcelImportResponse;
            if (completionScreen.HasError)
                Assert.Fail(completionScreen.GetResponseItemsWithError().First().Exception.XrmDisplayString());

            //verify the account and contact created
            var accounts = XrmService.RetrieveAllEntityType(Entities.account);
            var contacts = XrmService.RetrieveAllEntityType(Entities.contact);
            Assert.AreEqual(2, accounts.Count());
            Assert.AreEqual(3, contacts.Count());

            //okay lets run a second import which will only be for the contacts and verify the same and no errors
            entryViewmodel = app.NavigateToDialogModuleEntryForm<ExcelImportModule, ExcelImportDialog>();
            entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);
            mappingGrid = entryViewmodel.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.Mappings));
            Assert.AreEqual(1, mappingGrid.DynamicGridViewModel.GridRecords.Count);

            //trigger the import
            entryViewmodel.SaveButtonViewModel.Command.Execute();

            dialog = app.GetNavigatedDialog<ExcelImportDialog>();
            completionScreen = dialog.CompletionItem as ExcelImportResponse;
            if (completionScreen.HasError)
                Assert.Fail(completionScreen.GetResponseItemsWithError().First().Exception.XrmDisplayString());

            Assert.IsTrue(completionScreen.ImportSummary.All(i => i.Created == 0 && i.Updated == 0));
            //verify still same number
            accounts = XrmService.RetrieveAllEntityType(Entities.account);
            contacts = XrmService.RetrieveAllEntityType(Entities.contact);
            Assert.AreEqual(2, accounts.Count());
            Assert.AreEqual(3, contacts.Count());

            //okay lets run another import only for the contacts
            //with match by name false
            //in this case we expect the records to be created again
            entryViewmodel = app.NavigateToDialogModuleEntryForm<ExcelImportModule, ExcelImportDialog>();
            entryViewmodel.GetBooleanFieldFieldViewModel(nameof(ExcelImportRequest.MatchRecordsByName)).ValueObject = false;
            entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);
            mappingGrid = entryViewmodel.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.Mappings));
            Assert.AreEqual(1, mappingGrid.DynamicGridViewModel.GridRecords.Count);

            //trigger the import
            entryViewmodel.SaveButtonViewModel.Command.Execute();

            dialog = app.GetNavigatedDialog<ExcelImportDialog>();
            completionScreen = dialog.CompletionItem as ExcelImportResponse;
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
        public void ExcelImportWithAssociationTest()
        {
            //imports an excel with accounts linked to test entities
            PrepareTests();
            DeleteAll(Entities.account);
            DeleteAll(Entities.jmcg_testentity);

            var workFolder = TestingFolder + @"\ExcelImportScript";
            FileUtility.CheckCreateFolder(workFolder);
            var sourceExcelFile = Path.Combine(workFolder, @"TestExcelImportAssociations.xlsx");
            File.Copy(@"TestExcelImportAssociations.xlsx", sourceExcelFile);

            var app = CreateAndLoadTestApplication<ExcelImportModule>();
            app.AddModule<SavedRequestModule>();
            ClearSavedRequests(app);

            var entryViewmodel = app.NavigateToDialogModuleEntryForm<ExcelImportModule, ExcelImportDialog>();
            //select the excel file
            entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);

            //okay on change trigger should have fired and populated all the required mappings
            
            //so lets just trigger the import
            entryViewmodel.SaveButtonViewModel.Command.Execute();

            var dialog = app.GetNavigatedDialog<ExcelImportDialog>();
            var completionScreen = dialog.CompletionItem as ExcelImportResponse;
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
            entryViewmodel = app.NavigateToDialogModuleEntryForm<ExcelImportModule, ExcelImportDialog>();
            entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);

            //trigger the import
            entryViewmodel.SaveButtonViewModel.Command.Execute();

            dialog = app.GetNavigatedDialog<ExcelImportDialog>();
            completionScreen = dialog.CompletionItem as ExcelImportResponse;
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

        [DeploymentItem(@"Files\TestExcelImportAltMatchKeys.xlsx")]
        [TestMethod]
        public void ExcelImportAltLookupsTest()
        {
            //imports an excel with accounts linked to test entities
            PrepareTests();
            DeleteAll(Entities.account);
            DeleteAll(Entities.jmcg_testentity);

            var workFolder = TestingFolder + @"\ExcelImportScript";
            FileUtility.CheckCreateFolder(workFolder);
            var sourceExcelFile = Path.Combine(workFolder, @"TestExcelImportAltMatchKeys.xlsx");
            File.Copy(@"TestExcelImportAltMatchKeys.xlsx", sourceExcelFile);

            var app = CreateAndLoadTestApplication<ExcelImportModule>();
            app.AddModule<SavedRequestModule>();
            ClearSavedRequests(app);

            var entryViewmodel = app.NavigateToDialogModuleEntryForm<ExcelImportModule, ExcelImportDialog>();
            //select the excel file
            entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);

            //okay on change trigger should have fired and populated all the required mappings

            //set alt keys for test entity . account
            var mappingsGrid = entryViewmodel.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.Mappings));

            //get tab mappng for test entity
            var testEntityMapping = mappingsGrid.GridRecords.First(m => m.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.TargetType)).Value.Key == Entities.jmcg_testentity);
            //set edit its field mappings
            testEntityMapping.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.Mappings)).EditButton.Invoke();
            var childForm = entryViewmodel.ChildForms.First() as ObjectEntryViewModel;
            Assert.IsNotNull(childForm);
            childForm.LoadFormSections();
            var childTestEntityMappingFieldMappings = childForm.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.Mappings));
            //get the account field mapping
            var accountFieldMapping = childTestEntityMappingFieldMappings.GridRecords.First(f => f.GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.TargetField)).Value.Key == Fields.jmcg_testentity_.jmcg_account);

            //set use alt match and type for match becomes visible
            Assert.IsFalse(accountFieldMapping.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.AltMatchFieldType)).IsVisible);
            accountFieldMapping.GetBooleanFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.UseAltMatchField)).Value = true;
            Assert.IsTrue(accountFieldMapping.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.AltMatchFieldType)).IsVisible);
            //set type account
            accountFieldMapping.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.AltMatchFieldType)).Value = accountFieldMapping.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.AltMatchFieldType)).ItemsSource.First(t => t.Key == Entities.account);
            //set use account number for alt match field
            Assert.IsTrue(accountFieldMapping.GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.AltMatchField)).IsVisible);
            accountFieldMapping.GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.AltMatchField)).Value = accountFieldMapping.GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.AltMatchField)).ItemsSource.First(f => f.Key == Fields.account_.accountnumber);
            //save
            childForm.SaveButtonViewModel.Invoke();
            Assert.IsFalse(entryViewmodel.ChildForms.Any());

            //get tab mappng for association
            var associationMapping = mappingsGrid.GridRecords.First(m => m.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.TargetType)).Value.Key == Relationships.jmcg_testentity_.jmcg_testentity_account.EntityName);
            //set edit its field mappings
            associationMapping.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.Mappings)).EditButton.Invoke();
            childForm = entryViewmodel.ChildForms.First() as ObjectEntryViewModel;
            Assert.IsNotNull(childForm);
            childForm.LoadFormSections();
            childTestEntityMappingFieldMappings = childForm.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.Mappings));
            //get the account field mapping
            var associationAccountFieldMapping = childTestEntityMappingFieldMappings.GridRecords.First(f => f.GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.TargetField)).Value.Key == Fields.account_.accountid);
            //set use alt match and type for match becomes visible
            Assert.IsFalse(associationAccountFieldMapping.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.AltMatchFieldType)).IsVisible);
            associationAccountFieldMapping.GetBooleanFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.UseAltMatchField)).Value = true;
            Assert.IsTrue(associationAccountFieldMapping.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.AltMatchFieldType)).IsVisible);
            //set type account
            associationAccountFieldMapping.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.AltMatchFieldType)).Value = associationAccountFieldMapping.GetRecordTypeFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.AltMatchFieldType)).ItemsSource.First(t => t.Key == Entities.account);
            //set use account number for alt match field
            Assert.IsTrue(associationAccountFieldMapping.GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.AltMatchField)).IsVisible);
            associationAccountFieldMapping.GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.AltMatchField)).Value = associationAccountFieldMapping.GetRecordFieldFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping.AltMatchField)).ItemsSource.First(f => f.Key == Fields.account_.accountnumber);
            //save
            childForm.SaveButtonViewModel.Invoke();
            Assert.IsFalse(entryViewmodel.ChildForms.Any());

            //so lets trigger the import
            entryViewmodel.SaveButtonViewModel.Command.Execute();

            var dialog = app.GetNavigatedDialog<ExcelImportDialog>();
            var completionScreen = dialog.CompletionItem as ExcelImportResponse;
            if (completionScreen.HasError)
                Assert.Fail(completionScreen.GetResponseItemsWithError().First().Exception.XrmDisplayString());

            //verify the account and contact created
            var accounts = XrmService.RetrieveAllEntityType(Entities.account);
            var testEntities = XrmService.RetrieveAllEntityType(Entities.jmcg_testentity);
            Assert.AreEqual(3, accounts.Count());
            var account1 = accounts.First(a => a.GetStringField(Fields.account_.accountnumber) == "1");
            var account2 = accounts.First(a => a.GetStringField(Fields.account_.accountnumber) == "2");
            Assert.AreEqual(2, testEntities.Count());
            Assert.AreEqual(1, testEntities.Count(t => t.GetLookupGuid(Fields.jmcg_testentity_.jmcg_account) == account1.Id));
            Assert.AreEqual(1, testEntities.Count(t => t.GetLookupGuid(Fields.jmcg_testentity_.jmcg_account) == account2.Id));

            var associations = XrmService.RetrieveAllEntityType(Relationships.account_.jmcg_testentity_account.EntityName);
            Assert.AreEqual(3, associations.Count());
            Assert.AreEqual(2, associations.Count(a => a.GetGuidField(Fields.account_.accountid) == account1.Id));
            Assert.AreEqual(1, associations.Count(a => a.GetGuidField(Fields.account_.accountid) == account2.Id));
        }

        [DeploymentItem(@"Files\TestImportMultiples.xlsx")]
        [TestMethod]
        public void ExcelImportMultipleCachingTest()
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

                var ExcelImportApp = CreateAndLoadTestApplication<ExcelImportModule>();
                ExcelImportApp.AddModule<SavedRequestModule>();
                ClearSavedRequests<ExcelImportModule, ExcelImportDialog>(ExcelImportApp);

                //navigate to the dialog
                var dialog = ExcelImportApp.NavigateToDialog<ExcelImportModule, ExcelImportDialog>();
                var entryViewmodel = ExcelImportApp.GetSubObjectEntryViewModel(dialog);
                //select the excel file with the errors and submit form
                entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);
                entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.ExecuteMultipleSetSize)).ValueObject = setSize;
                entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.TargetCacheLimit)).ValueObject = cacheLimit;
                Assert.IsTrue(entryViewmodel.Validate());
                entryViewmodel.SaveButtonViewModel.Invoke();

                var ExcelImportResponse = dialog.CompletionItem as ExcelImportResponse;
                if (ExcelImportResponse.HasError)
                    Assert.Fail(ExcelImportResponse.GetResponseItemsWithError().First().Exception.XrmDisplayString());

                Assert.IsTrue(ExcelImportResponse.ImportSummary.All(i => i.Created > 0));
                Assert.IsTrue(ExcelImportResponse.ImportSummary.All(i => i.Updated == 0));
                Assert.IsTrue(ExcelImportResponse.ImportSummary.All(i => i.NoChange == 0));
                Assert.IsTrue(ExcelImportResponse.ImportSummary.All(i => i.Errors == 0));

                ValidateimportedData();

                //lets do another and verify no errors or changes
                dialog = ExcelImportApp.NavigateToDialog<ExcelImportModule, ExcelImportDialog>();
                entryViewmodel = ExcelImportApp.GetSubObjectEntryViewModel(dialog);
                //select the excel file with the errors and submit form
                entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);
                entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.ExecuteMultipleSetSize)).ValueObject = setSize;
                entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.TargetCacheLimit)).ValueObject = cacheLimit;
                Assert.IsTrue(entryViewmodel.Validate());
                entryViewmodel.SaveButtonViewModel.Invoke();

                ExcelImportResponse = dialog.CompletionItem as ExcelImportResponse;
                if (ExcelImportResponse.HasError)
                    Assert.Fail(ExcelImportResponse.GetResponseItemsWithError().First().Exception.XrmDisplayString());

                Assert.IsTrue(ExcelImportResponse.ImportSummary.All(i => i.Created == 0));
                Assert.IsTrue(ExcelImportResponse.ImportSummary.All(i => i.Updated == 0));
                Assert.IsTrue(ExcelImportResponse.ImportSummary.All(i => i.NoChange > 0));
                Assert.IsTrue(ExcelImportResponse.ImportSummary.All(i => i.Errors == 0));

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

                ExcelImportApp = CreateAndLoadTestApplication<ExcelImportModule>();
                ExcelImportApp.AddModule<SavedRequestModule>();
                ClearSavedRequests<ExcelImportModule, ExcelImportDialog>(ExcelImportApp);

                dialog = ExcelImportApp.NavigateToDialog<ExcelImportModule, ExcelImportDialog>();
                entryViewmodel = ExcelImportApp.GetSubObjectEntryViewModel(dialog);
                entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.ExcelFile)).ValueObject = new FileReference(sourceExcelFile);
                entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.ExecuteMultipleSetSize)).ValueObject = setSize;
                entryViewmodel.GetFieldViewModel(nameof(ExcelImportRequest.TargetCacheLimit)).ValueObject = cacheLimit;
                Assert.IsTrue(entryViewmodel.Validate());
                entryViewmodel.SaveButtonViewModel.Invoke();

                ExcelImportResponse = dialog.CompletionItem as ExcelImportResponse;
                if (ExcelImportResponse.HasError)
                    Assert.Fail(ExcelImportResponse.GetResponseItemsWithError().First().Exception.XrmDisplayString());

                Assert.IsTrue(ExcelImportResponse.ImportSummary.All(i => i.Created > 0));
                Assert.IsTrue(ExcelImportResponse.ImportSummary.All(i => i.Updated == 0));
                Assert.IsTrue(ExcelImportResponse.ImportSummary.All(i => i.NoChange == 0));
                Assert.IsTrue(ExcelImportResponse.ImportSummary.All(i => i.Errors == 0));

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
            foreach (var type in types)
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