﻿using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Core.Test;
using JosephM.Core.Utility;
using JosephM.Deployment.SpreadsheetImport;
using JosephM.Spreadsheet;
using JosephM.Xrm;
using JosephM.Xrm.CsvImport;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Deployment.Test
{
    [TestClass]
    public class CsvImportTests : XrmModuleTest
    {
        [DeploymentItem(@"Files\accounts.csv")]
        [TestMethod]
        public void CsvImportValidationTest()
        {
            //script through csv import where there are parse field errors
            //for an invalid picklist value and exceeding the max string length
            //in this case after submitting the form the validation/parse errors should 
            //display and allow moving back to entry or proceeding anyway
            PrepareTests();
            var workFolder = ClearFilesAndData(new[]
            {
                Entities.account
            });

            File.Copy(@"accounts.csv", Path.Combine(TestConstants.TestFolder, @"accounts.csv"));

            var application = CreateAndLoadTestApplication<CsvImportModule>();
            var dialog = application.NavigateToDialog<CsvImportModule, CsvImportDialog>();
            var entryViewModel = application.GetSubObjectEntryViewModel(dialog);

            var mappingField = entryViewModel.GetEnumerableFieldViewModel(nameof(CsvImportRequest.CsvsToImport));
            var mappingGrid = mappingField.DynamicGridViewModel;

            //the buttons aren't loading into the grid in this script for some obscure reason
            //think due to multiple for loading and something to do with observable collections
            //so lets just load get them directly from the form service

            //trigger the load folder function
            var customButtons = mappingGrid.GridsFunctionsToXrmButtons(entryViewModel.FormService.GetCustomFunctionsFor(mappingField.FieldName, entryViewModel));
            customButtons.First(b => b.Id == "LOADFOLDER").Invoke();
            entryViewModel.SaveButtonViewModel.Invoke();

            //check validation results displayed
            var validationResults = dialog.Controller.UiItems.First() as ObjectDisplayViewModel;
            Assert.IsNotNull(validationResults);
            Assert.IsTrue(validationResults.GetObject() is ParseIntoEntitiesResponse);

            //navigate back to entry form
            validationResults.BackButtonViewModel.Invoke();
            entryViewModel = dialog.Controller.UiItems.First() as ObjectEntryViewModel;
            Assert.IsNotNull(entryViewModel);
            Assert.IsTrue(entryViewModel.GetObject() is CsvImportRequest);

            //submit again
            entryViewModel.SaveButtonViewModel.Invoke();
            validationResults = dialog.Controller.UiItems.First() as ObjectDisplayViewModel;
            Assert.IsNotNull(validationResults);
            Assert.IsTrue(validationResults.GetObject() is ParseIntoEntitiesResponse);

            //at validation display proceed anyway
            validationResults.SaveButtonViewModel.Invoke();
            var completionScreen = application.GetCompletionViewModel(dialog);
            var ExcelImportResponse = completionScreen.GetObject() as CsvImportResponse;
            Assert.IsNotNull(ExcelImportResponse);
            Assert.IsTrue(ExcelImportResponse.ResponseItems.Any());
        }

        [DeploymentItem(@"Files\AccountsWithKeys.csv")]
        [TestMethod]
        public void CsvImportMatchKeysTest()
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

            var workFolder = TestingFolder + @"\CsvsImportScript";
            FileUtility.CheckCreateFolder(workFolder);
            var sourceCsvsFile = Path.Combine(workFolder, @"account.csv");
            File.Copy(@"AccountsWithKeys.csv", sourceCsvsFile);

            var app = CreateAndLoadTestApplication<CsvImportModule>();
            var dialog = app.NavigateToDialog<CsvImportModule, CsvImportDialog>();
            var entryViewmodel = app.GetSubObjectEntryViewModel(dialog);
            entryViewmodel.GetBooleanFieldFieldViewModel(nameof(CsvImportRequest.MaskEmails)).Value = true;

            //add csv row and set it to the source csv
            var tabMappingsGrid = entryViewmodel.GetEnumerableFieldViewModel(nameof(CsvImportRequest.CsvsToImport));
            tabMappingsGrid.DynamicGridViewModel.AddRow();
            var accountMap = tabMappingsGrid.DynamicGridViewModel.GridRecords.First();
            var fileReference = accountMap.GetFieldViewModel<FileRefFieldViewModel>(nameof(CsvImportRequest.CsvToImport.SourceCsv));
            fileReference.Value = new FileReference(sourceCsvsFile);
            //okay on change trigger should have fired and populated mappings on contact
            //now add match key 
            var keyMapsField = accountMap.GetEnumerableFieldViewModel(nameof(CsvImportRequest.CsvToImport.AltMatchKeys));
            keyMapsField.EditButton.Command.Execute();
            var altMatchKeyEntryForm = entryViewmodel.ChildForms.First() as ObjectEntryViewModel;
            Assert.IsNotNull(altMatchKeyEntryForm);
            altMatchKeyEntryForm.LoadFormSections();
            var mapsField = altMatchKeyEntryForm.GetEnumerableFieldViewModel(nameof(CsvImportRequest.CsvToImport.AltMatchKeys));
            mapsField.AddRow();
            var matchKeyField = mapsField.DynamicGridViewModel.GridRecords.First().GetRecordFieldFieldViewModel(nameof(CsvImportRequest.CsvToImport.CsvImportMatchKey.TargetField));
            Assert.IsTrue(matchKeyField.ItemsSource.Any());
            matchKeyField.Value = matchKeyField.ItemsSource.First(f => f.Key == Fields.account_.accountnumber);
            altMatchKeyEntryForm.SaveButtonViewModel.Invoke();
            Assert.IsFalse(entryViewmodel.ChildForms.Any());

            //run the import and verify response and account count
            entryViewmodel.SaveButtonViewModel.Invoke();
            var response = app.GetCompletionViewModel(dialog).GetObject() as CsvImportResponse;
            Assert.IsNotNull(response);

            Assert.AreEqual(1, response.GetResponseItemsWithError().Count());
            var accounts = XrmService.RetrieveAllEntityType(Entities.account);
            Assert.AreEqual(4, accounts.Count());

            //delete one of the imported accounts
            XrmService.Delete(accounts.First());
            //second import - this one only allows updates
            dialog = app.NavigateToDialog<CsvImportModule, CsvImportDialog>();
            entryViewmodel = app.GetSubObjectEntryViewModel(dialog);
            entryViewmodel.GetBooleanFieldFieldViewModel(nameof(CsvImportRequest.MaskEmails)).Value = true;
            entryViewmodel.GetBooleanFieldFieldViewModel(nameof(CsvImportRequest.UpdateOnly)).Value = true;

            //add csv row and set it to the source csv
            tabMappingsGrid = entryViewmodel.GetEnumerableFieldViewModel(nameof(CsvImportRequest.CsvsToImport));
            tabMappingsGrid.DynamicGridViewModel.AddRow();
            accountMap = tabMappingsGrid.DynamicGridViewModel.GridRecords.First();
            fileReference = accountMap.GetFieldViewModel<FileRefFieldViewModel>(nameof(CsvImportRequest.CsvToImport.SourceCsv));
            fileReference.Value = new FileReference(sourceCsvsFile);
            //okay on change trigger should have fired and populated mappings on contact
            //now add match key 
            keyMapsField = accountMap.GetEnumerableFieldViewModel(nameof(CsvImportRequest.CsvToImport.AltMatchKeys));
            keyMapsField.EditButton.Command.Execute();
            altMatchKeyEntryForm = entryViewmodel.ChildForms.First() as ObjectEntryViewModel;
            Assert.IsNotNull(altMatchKeyEntryForm);
            altMatchKeyEntryForm.LoadFormSections();
            mapsField = altMatchKeyEntryForm.GetEnumerableFieldViewModel(nameof(CsvImportRequest.CsvToImport.AltMatchKeys));
            mapsField.AddRow();
            matchKeyField = mapsField.DynamicGridViewModel.GridRecords.First().GetRecordFieldFieldViewModel(nameof(CsvImportRequest.CsvToImport.CsvImportMatchKey.TargetField));
            Assert.IsTrue(matchKeyField.ItemsSource.Any());
            matchKeyField.Value = matchKeyField.ItemsSource.First(f => f.Key == Fields.account_.accountnumber);
            altMatchKeyEntryForm.SaveButtonViewModel.Invoke();
            Assert.IsFalse(entryViewmodel.ChildForms.Any());

            //run the import and verify response and account count
            entryViewmodel.SaveButtonViewModel.Invoke();

            response = app.GetCompletionViewModel(dialog).GetObject() as CsvImportResponse;
            Assert.IsNotNull(response);

            Assert.AreEqual(2, response.GetResponseItemsWithError().Count());
            accounts = XrmService.RetrieveAllEntityType(Entities.account);
            Assert.AreEqual(3, accounts.Count());
        }

        [DeploymentItem(@"Files\Account.csv")]
        [DeploymentItem(@"Files\Contact.csv")]
        [TestMethod]
        public void CsvImportLoadFolderTest()
        {
            PrepareTests();
            var workFolder = ClearFilesAndData(new[]
            {
                Entities.account,
                Entities.contact
            });

            File.Copy(@"Account.csv", Path.Combine(TestConstants.TestFolder, @"Account.csv"));
            File.Copy(@"Contact.csv", Path.Combine(TestConstants.TestFolder, @"Contact.csv"));

            var application = CreateAndLoadTestApplication<CsvImportModule>();

            var entryViewModel = application.NavigateToDialogModuleEntryForm<CsvImportModule, CsvImportDialog>();

            var mappingField = entryViewModel.GetEnumerableFieldViewModel(nameof(CsvImportRequest.CsvsToImport));
            var mappingGrid = mappingField.DynamicGridViewModel;

            //the buttons aren't loading into the grid in this script for some obscure reason
            //think due to multiple for loading and something to do with observable collections
            //so lets just load get them directly from the form service

            //trigger the load folder function
            var customButtons = mappingGrid.GridsFunctionsToXrmButtons(entryViewModel.FormService.GetCustomFunctionsFor(mappingField.FieldName, entryViewModel));
            customButtons.First(b => b.Id == "LOADFOLDER").Invoke();

            //this should have loaded the csv files as well as automapped them
            //so lets trigger the process by saving
            entryViewModel.SaveButtonViewModel.Invoke();

            //check no errors
            var dialog = application.GetNavigatedDialog<CsvImportDialog>();
            var completionScreen = dialog.CompletionItem as CsvImportResponse;
            if (completionScreen.HasError)
                Assert.Fail(completionScreen.GetResponseItemsWithError().First().Exception.XrmDisplayString());

            //check the accounts and contacts created
            var accounts = XrmService.RetrieveAllEntityType(Entities.account);
            var contacts = XrmService.RetrieveAllEntityType(Entities.contact);
            Assert.AreEqual(1, accounts.Count());
            Assert.AreEqual(2, contacts.Count());
        }

        [DeploymentItem(@"Files\Account.csv")]
        [DeploymentItem(@"Files\Contact.csv")]
        [TestMethod]
        public void CsvImportAccountAndContactsTest()
        {
            PrepareTests();
            var workFolder = ClearFilesAndData(new[]
            {
                Entities.account,
                Entities.contact
            });

            File.Copy(@"Account.csv", Path.Combine(workFolder, @"Account.csv"));
            File.Copy(@"Contact.csv", Path.Combine(workFolder, @"Contact.csv"));

            var application = CreateAndLoadTestApplication<CsvImportModule>();

            var request = new CsvImportRequest
            {
                CsvsToImport = new []
                {
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Contact.csv")) },
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Account.csv")) },
                },
                DateFormat = DateFormat.American
            };

            var response = application.NavigateAndProcessDialog<CsvImportModule, CsvImportDialog, CsvImportResponse>(request);
            if (response.HasError)
                Assert.Fail(response.GetResponseItemsWithError().First().Exception.XrmDisplayString());

            var accounts = XrmService.RetrieveAllEntityType(Entities.account);
            var contacts = XrmService.RetrieveAllEntityType(Entities.contact);
            Assert.AreEqual(1, accounts.Count());
            Assert.AreEqual(2, contacts.Count());

            foreach (var contact in contacts)
            {
                Assert.AreEqual(accounts.First().Id, contact.GetLookupGuid(Fields.contact_.parentcustomerid));
                Assert.IsNotNull(contact.GetStringField(Fields.contact_.firstname));
                Assert.IsNotNull(contact.GetStringField(Fields.contact_.lastname));
            }
        }

        [DeploymentItem(@"Files\Account.csv")]
        [DeploymentItem(@"Files\jmcg_testentity_account.csv")]
        [DeploymentItem(@"Files\Test Entity.csv")]
        [DeploymentItem(@"Files\Test Entity Two.csv")]
        [DeploymentItem(@"Files\Test Entity Three.csv")]
        [DeploymentItem(@"Files\Team.csv")]
        [DeploymentItem(@"Files\jmcg_testentity.csv")]
        [TestMethod]
        public void DeploymentImportCsvMultipleTest()
        {
            PrepareTests();
            var typesExTeam = new[] { Entities.jmcg_testentitytwo, Entities.jmcg_testentitythree, Entities.jmcg_testentity, Entities.account };
            var workFolder = ClearFilesAndData(typesExTeam);
            var testimportTeam = XrmService.GetFirst("team", "name", "TestImportTeam");
            if (testimportTeam != null)
                XrmService.Delete(testimportTeam);
            DeleteAll(Entities.account);
            File.Copy(@"Account.csv", Path.Combine(workFolder, @"Account.csv"));
            File.Copy(@"jmcg_testentity_account.csv", Path.Combine(workFolder, @"jmcg_testentity_account.csv"));
            File.Copy(@"Test Entity.csv", Path.Combine(workFolder, @"Test Entity.csv"));
            File.Copy(@"Test Entity Two.csv", Path.Combine(workFolder, @"Test Entity Two.csv"));
            File.Copy(@"Test Entity Three.csv", Path.Combine(workFolder, @"Test Entity Three.csv"));
            File.Copy(@"Team.csv", Path.Combine(workFolder, @"Team.csv"));

            var application = CreateAndLoadTestApplication<CsvImportModule>();

            var request = new CsvImportRequest
            {
                DateFormat = DateFormat.American,
                CsvsToImport = new[] {
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Account.csv")) },
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"jmcg_testentity_account.csv")) },
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Test Entity.csv")) },
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Test Entity Two.csv")) },
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Test Entity Three.csv")) },
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Team.csv")) },
                    }
            };

            var response = application.NavigateAndProcessDialog<CsvImportModule, CsvImportDialog, CsvImportResponse>(request);
            Assert.IsFalse(response.HasError);

            application = CreateAndLoadTestApplication<CsvImportModule>();

            request = new CsvImportRequest
            {
                DateFormat = DateFormat.American,
                CsvsToImport = new[] {
                    new CsvImportRequest.CsvToImport() {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Account.csv")) } }
            };
            response = application.NavigateAndProcessDialog<CsvImportModule, CsvImportDialog, CsvImportResponse>(request);
            Assert.IsFalse(response.HasError);

            File.Copy(@"jmcg_testentity.csv", Path.Combine(workFolder, @"jmcg_testentity.csv"));

            //this one sets a record inactive state
            var accounta = CreateTestRecord(Entities.account, new Dictionary<string, object>()
            {
                { Fields.account_.name, "accounta" }
            });
            var accountb = CreateTestRecord(Entities.account, new Dictionary<string, object>()
            {
                { Fields.account_.name, "accountb" }
            });

            application = CreateAndLoadTestApplication<CsvImportModule>();

            request = new CsvImportRequest
            {
                DateFormat = DateFormat.English,
                CsvsToImport = new[] {
                    new CsvImportRequest.CsvToImport() {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"jmcg_testentity.csv")) } }
            };

            response = application.NavigateAndProcessDialog<CsvImportModule, CsvImportDialog, CsvImportResponse>(request);
            Assert.IsFalse(response.HasError);

            var entity = XrmService.GetFirst(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_name, "BLAH 2");
            Assert.AreEqual(XrmPicklists.State.Inactive, entity.GetOptionSetValue(Fields.jmcg_testentity_.statecode));
        }

        [DeploymentItem(@"Files\Price List Items.csv")]
        [DeploymentItem(@"Files\Price Lists.csv")]
        [DeploymentItem(@"Files\Products.csv")]
        [DeploymentItem(@"Files\uom.csv")]
        [DeploymentItem(@"Files\uomschedule.csv")]
        [TestMethod]
        public void CsvImportTestProductsAndPricings()
        {
            //imports product configurations in csv files
            PrepareTests();
            var workFolder = ClearFilesAndData();

            File.Copy(@"Price List Items.csv", Path.Combine(workFolder, @"Price List Items.csv"));
            File.Copy(@"Price Lists.csv", Path.Combine(workFolder, @"Price Lists.csv"));
            File.Copy(@"Products.csv", Path.Combine(workFolder, @"Products.csv"));
            File.Copy(@"uom.csv", Path.Combine(workFolder, @"uom.csv"));
            File.Copy(@"uomschedule.csv", Path.Combine(workFolder, @"uomschedule.csv"));

            //lets delete all these items
            var products = CsvUtility
                .SelectAllRows(Path.Combine(workFolder, @"Products.csv"))
                .Select(r => r.GetFieldAsString("Name"))
                .ToArray();
            DeleteAllMatchingName(Entities.product, products);
            var unitGroups = CsvUtility
                .SelectAllRows(Path.Combine(workFolder, @"uomschedule.csv"))
                .Select(r => r.GetFieldAsString("Name"))
                .ToArray();
            DeleteAllMatchingName(Entities.uomschedule, unitGroups);
            var priceLists = CsvUtility
                .SelectAllRows(Path.Combine(workFolder, @"Price Lists.csv"))
                .Select(r => r.GetFieldAsString("Name"))
                .ToArray();
            DeleteAllMatchingName(Entities.pricelevel, priceLists);

            //run the import and verify no errors
            var application = CreateAndLoadTestApplication<CsvImportModule>();
            var request = new CsvImportRequest
            {
                CsvsToImport = new[]
                {
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Price List Items.csv")) },
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Price Lists.csv")) },
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Products.csv")) },
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"uom.csv")) },
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"uomschedule.csv")) },
                },
                DateFormat = DateFormat.American
            };
            var response = application.NavigateAndProcessDialog<CsvImportModule, CsvImportDialog, CsvImportResponse>(request);
            Assert.IsFalse(response.HasError);

            //okay lets get the last created price list item
            var query = XrmService.BuildQuery(Entities.productpricelevel, null, null, null);
            query.Orders.Add(new OrderExpression(Fields.productpricelevel_.createdon, OrderType.Descending));
            var latestPriceListItem = XrmService.RetrieveFirst(query);
            //verify it has a price
            var initialPrice = latestPriceListItem.GetMoneyValue(Fields.productpricelevel_.amount);
            Assert.IsTrue(initialPrice > 0);
            //now lets set it something else so we can verify it gets updated after the second run
            latestPriceListItem.SetMoneyField(Fields.productpricelevel_.amount, initialPrice.Value + 1);
            latestPriceListItem = UpdateFieldsAndRetreive(latestPriceListItem, Fields.productpricelevel_.amount);

            //run again and verify no errors
            request = new CsvImportRequest
            {
                CsvsToImport = new[]
                {
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Price List Items.csv")) },
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Price Lists.csv")) },
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Products.csv")) },
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"uom.csv")) },
                    new CsvImportRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"uomschedule.csv")) },
                },
                DateFormat = DateFormat.American
            };
            response = application.NavigateAndProcessDialog<CsvImportModule, CsvImportDialog, CsvImportResponse>(request);
            Assert.IsFalse(response.HasError);
            //verify the price list item we updated is changed
            latestPriceListItem = Refresh(latestPriceListItem);
            Assert.AreEqual(initialPrice, latestPriceListItem.GetMoneyValue(Fields.productpricelevel_.amount));
        }

        [DeploymentItem(@"Files\Account.csv")]
        [TestMethod]
        public void CsvImportTestMaskEmails()
        {
            PrepareTests();
            var workFolder = ClearFilesAndData();
            DeleteAll(Entities.account);
            File.Copy(@"Account.csv", Path.Combine(workFolder, @"Account.csv"));

            var application = CreateAndLoadTestApplication<CsvImportModule>();

            var request = new CsvImportRequest
            {
                CsvsToImport = new[] {
                    new CsvImportRequest.CsvToImport()
                    {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Account.csv"))
                    }
                },
                MaskEmails = true
            };
            var response = application.NavigateAndProcessDialog<CsvImportModule, CsvImportDialog, CsvImportResponse>(request);
            if (response.HasError)
                Assert.Fail(response.GetResponseItemsWithError().First().Exception.XrmDisplayString());

            var entity = XrmService.GetFirst(Entities.account);
            Assert.IsTrue(entity.GetStringField(Fields.account_.emailaddress1).Contains("_AT_"));

            request = new CsvImportRequest
            {
                CsvsToImport = new[] {
                    new CsvImportRequest.CsvToImport()
                    {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Account.csv"))
                    }
                },
                MaskEmails = false
            };
            response = application.NavigateAndProcessDialog<CsvImportModule, CsvImportDialog, CsvImportResponse>(request);
            if (response.HasError)
                Assert.Fail(response.GetResponseItemsWithError().First().Exception.XrmDisplayString());

            entity = XrmService.GetFirst(Entities.account);
            Assert.IsFalse(entity.GetStringField(Fields.account_.emailaddress1).Contains("_AT_"));
        }


        /// <summary>
        /// Scripts through generation of csv import templates
        /// which is a custom button option during the import csv dialog
        /// </summary>
        [DeploymentItem(@"Files\Account.csv")]
        [TestMethod]
        public void CsvImportDownloadTemplateTests()
        {
            //navigate to the download csv templates form
            var application = CreateAndLoadTestApplication<CsvImportModule>();
            var csvImportForm = application.NavigateToDialogModuleEntryForm<CsvImportModule, CsvImportDialog>();
            var downloadButton = csvImportForm.GetButton("DOWNLOADCSVTEMPLATES");
            downloadButton.Invoke();
            var downloadTemplatesForm = application.GetSubObjectEntryViewModel(csvImportForm);

            //okay now lets add an export for account selected fields
            //and for contact specific fields
            //this one will use labels 
            var entry = new GenerateTemplatesRequest
            {
                UseSchemaNames = false,
                FolderToSaveInto = new Folder(TestingFolder),
                CsvsToGenerate = new[]
                 {
                     new GenerateTemplateConfiguration
                     {
                          RecordType = new RecordType(Entities.account, Entities.account),
                          FieldsToInclude = new []
                          {
                              new FieldSetting { RecordField = new RecordField(Fields.account_.name, Fields.account_.name) },
                              new FieldSetting { RecordField = new RecordField(Fields.account_.accountnumber, Fields.account_.accountnumber) },
                              new FieldSetting { RecordField = new RecordField(Fields.account_.customertypecode, Fields.account_.customertypecode) },
                          }
                     },
                     new GenerateTemplateConfiguration
                     {
                          RecordType = new RecordType(Entities.contact, Entities.contact),
                          AllFields = true
                     }
                 }
            };
            //verify saves, creates the files, and returns to main form
            application.EnterAndSaveObject(entry, downloadTemplatesForm);
            Assert.AreEqual(2, FileUtility.GetFiles(TestingFolder).Count());
            Assert.IsFalse(csvImportForm.ChildForms.Any());

            //okay lets just verify it works for schema names as well
            entry.UseSchemaNames = true;
            downloadButton = csvImportForm.GetButton("DOWNLOADCSVTEMPLATES");
            downloadButton.Invoke();
            downloadTemplatesForm = application.GetSubObjectEntryViewModel(csvImportForm);
            application.EnterAndSaveObject(entry, downloadTemplatesForm);
            Assert.AreEqual(4, FileUtility.GetFiles(TestingFolder).Count());
            Assert.IsFalse(csvImportForm.ChildForms.Any());
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