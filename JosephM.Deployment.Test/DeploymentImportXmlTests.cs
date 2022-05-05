using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment.DataImport;
using JosephM.Deployment.ExportXml;
using JosephM.Deployment.ImportCsvs;
using JosephM.Deployment.ImportXml;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Spreadsheet;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Deployment.Test
{
    [TestClass]
    public class DeploymentImportXmlTests : XrmModuleTest
    {
        /// <summary>
        /// When creating multiple records during an import where multiple have the same name
        /// verify each is created (they dont match to each other)
        /// </summary>
        [TestMethod]
        public void DeploymentImportXmlMultipleFilesForSameRecordTest()
        {
            var type = Entities.jmcg_testentity;
            PrepareTests();
            var workFolder = ClearFilesAndData(type);

            var recordName1 = CreateTestRecord(TestEntityType, new Dictionary<string, object>
            {
                { Fields.jmcg_testentity_.jmcg_name, "TestName1" }
            });
            var recordName2 = CreateTestRecord(TestEntityType, new Dictionary<string, object>
            {
                { Fields.jmcg_testentity_.jmcg_name, "TestName2" }
            });

            var exportService = new ExportXmlService(XrmRecordService);
            var exportRequest = new ExportXmlRequest
            {
                Folder = new Folder(workFolder),
                RecordTypesToExport = new[] { new ExportRecordType() { RecordType = new RecordType(TestEntityType, TestEntityType) } }
            };
            var exportResponse = exportService.Execute(exportRequest, new ServiceRequestController(Controller));
            Assert.IsTrue(exportResponse.Success);

            recordName1.SetField(Fields.jmcg_testentity_.jmcg_name, "TestName1 - Updated!");
            recordName1 = UpdateFieldsAndRetreive(recordName1, Fields.jmcg_testentity_.jmcg_name);
            exportResponse = exportService.Execute(exportRequest, new ServiceRequestController(Controller));
            Assert.IsTrue(exportResponse.Success);

            Assert.AreEqual(3, FileUtility.GetFiles(workFolder).Count());

            var app = CreateAndLoadTestApplication<ImportXmlModule>();

            var importRequest = new ImportXmlRequest
            {
                Folder = new Folder(workFolder)
            };

            var dialog = app.NavigateToDialog<ImportXmlModule, ImportXmlDialog>();
            var entryViewmodel = app.GetSubObjectEntryViewModel(dialog);
            //select the excel file with the errors and submit form
            app.EnterAndSaveObject(importRequest, entryViewmodel);

            //check validation results displayed
            var validationResults = dialog.Controller.UiItems.First() as ObjectDisplayViewModel;
            Assert.IsNotNull(validationResults);
            Assert.IsTrue(validationResults.GetObject() is ImportXmlValidationDialog.DuplicateImportXmlRecords);

            Assert.AreEqual(2, ((ImportXmlValidationDialog.DuplicateImportXmlRecords)validationResults.GetObject()).Duplicates.Count());
        }

        /// <summary>
        /// When creating multiple records during an import where multiple have the same name
        /// verify each is created (they dont match to each other)
        /// </summary>
        [TestMethod]
        public void DeploymentImportXmlCreateMultipleWithSameNameTest()
        {
            var type = Entities.jmcg_testentity;
            PrepareTests();
            var workFolder = ClearFilesAndData(type);

            var recordName1a = CreateTestRecord(TestEntityType, new Dictionary<string, object>
            {
                { Fields.jmcg_testentity_.jmcg_name, "TestName1" }
            });
            var recordName1b = CreateTestRecord(TestEntityType, new Dictionary<string, object>
            {
                { Fields.jmcg_testentity_.jmcg_name, "TestName1" }
            });
            var recordName2 = CreateTestRecord(TestEntityType, new Dictionary<string, object>
            {
                { Fields.jmcg_testentity_.jmcg_name, "TestName2" }
            });

            var exportService = new ExportXmlService(XrmRecordService);
            var exportRequest = new ExportXmlRequest
            {
                Folder = new Folder(workFolder),
                RecordTypesToExport = new[] { new ExportRecordType() { RecordType = new RecordType(TestEntityType, TestEntityType) } }
            };
            var exportResponse = exportService.Execute(exportRequest, new ServiceRequestController(Controller));
            Assert.IsTrue(exportResponse.Success);

            DeleteAll(type);

            var application = CreateAndLoadTestApplication<ImportXmlModule>();

            var importRequest = new ImportXmlRequest
            {
                Folder = new Folder(workFolder)
            };

            var response = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(response.HasError);

            var records = XrmRecordService.RetrieveAll(type, new string[0]);
            Assert.AreEqual(3, records.Count());
        }

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
            var aNote = CreateTestRecord(Entities.annotation, new Dictionary<string, object>
            {
                { Fields.annotation_.objectid, createRecords[0].ToEntityReference() },
                { Fields.annotation_.subject, "Test Scripting" },
                { Fields.annotation_.notetext, "Just For Importing Testing" },
            });


            var exportService = new ExportXmlService(XrmRecordService);
            var exportRequest = new ExportXmlRequest
            {
                Folder = new Folder(workFolder),
                RecordTypesToExport = types.Select(t => new ExportRecordType() { RecordType = new RecordType(t, t) }),
                IncludeNNRelationshipsBetweenEntities = true,
                IncludeNotes = true
            };
            var response = exportService.Execute(exportRequest, new ServiceRequestController(Controller));
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
                    var notes = XrmService.RetrieveAllAndConditions(Entities.annotation, new[]
                    {
                        new ConditionExpression(Fields.annotation_.objectid, ConditionOperator.Equal, loaded.Id)
                    });
                    Assert.AreEqual(1, notes.Count());
                    var associated = XrmService.GetAssociatedIds(rMetadata.SchemaName, rMetadata.Entity1IntersectAttribute, loaded.Id, rMetadata.Entity2IntersectAttribute);
                    Assert.AreEqual(1, associated.Count());
                    Assert.AreEqual(createRecords[1].Id, associated.First());
                }
            }
        }

        [DeploymentItem(@"Files\Price List Items.csv")]
        [DeploymentItem(@"Files\Price Lists.csv")]
        [DeploymentItem(@"Files\Products.csv")]
        [DeploymentItem(@"Files\uom.csv")]
        [DeploymentItem(@"Files\uomschedule.csv")]
        [TestMethod]
        public void DeploymentImportXmlProductsAndPricingTest()
        {
            PrepareTests();

            //created csv files for all the relevant data so lets just hook into that to generate test data
            GenerateProductData();

            //export to xml
            var exportApp = new ExportXmlService(XrmRecordService);// CreateAndLoadTestApplication<ExportXmlModule>();
            var exportRequest = new ExportXmlRequest
            {
                Folder = new Folder(WorkFolder),
                IncludeNNRelationshipsBetweenEntities = true,
                RecordTypesToExport = new[]
                 {
                     new ExportRecordType() { RecordType = new RecordType(Entities.product, Entities.product)},
                     new ExportRecordType() { RecordType = new RecordType(Entities.pricelevel, Entities.pricelevel)},
                     new ExportRecordType() { RecordType = new RecordType(Entities.productpricelevel, Entities.productpricelevel)},
                     new ExportRecordType() { RecordType = new RecordType(Entities.uom, Entities.uom)},
                     new ExportRecordType() { RecordType = new RecordType(Entities.uomschedule, Entities.uomschedule)},
                 }
            };
            //verify no errors
            var exportResponse = exportApp.Execute(exportRequest, new ServiceRequestController(Controller));
            Assert.IsFalse(exportResponse.HasError);

            //delete all the data so when we import the xml it is creating new
            DeleteProductData();

            //import the xml
            var application = CreateAndLoadTestApplication<ImportXmlModule>();
            var importRequest = new ImportXmlRequest
            {
                Folder = new Folder(WorkFolder)
            };
            //verify no errors
            var importResponse = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(importResponse.HasError);

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

            //rerun the import
            application = CreateAndLoadTestApplication<ImportXmlModule>();
            importRequest = new ImportXmlRequest
            {
                Folder = new Folder(WorkFolder)
            };

            importResponse = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(importResponse.HasError);
            //verify the price list item we updated is changed
            latestPriceListItem = Refresh(latestPriceListItem);
            Assert.AreEqual(initialPrice, latestPriceListItem.GetMoneyValue(Fields.productpricelevel_.amount));
        }

        private void GenerateProductData()
        {
            var workFolder = ClearFilesAndData();
            File.Copy(@"Price List Items.csv", Path.Combine(workFolder, @"Price List Items.csv"));
            File.Copy(@"Price Lists.csv", Path.Combine(workFolder, @"Price Lists.csv"));
            File.Copy(@"Products.csv", Path.Combine(workFolder, @"Products.csv"));
            File.Copy(@"uom.csv", Path.Combine(workFolder, @"uom.csv"));
            File.Copy(@"uomschedule.csv", Path.Combine(workFolder, @"uomschedule.csv"));

            //run the import and verify no errors
            var application = CreateAndLoadTestApplication<ImportCsvsModule>();

            var request = new ImportCsvsRequest
            {
                CsvsToImport = new[]
                {
                    new ImportCsvsRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Price List Items.csv")) },
                    new ImportCsvsRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Price Lists.csv")) },
                    new ImportCsvsRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"Products.csv")) },
                    new ImportCsvsRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"uom.csv")) },
                    new ImportCsvsRequest.CsvToImport {
                        SourceCsv = new FileReference(Path.Combine(workFolder, @"uomschedule.csv")) },
                },
                DateFormat = DateFormat.American
            };

            var response = application.NavigateAndProcessDialog<ImportCsvsModule, ImportCsvsDialog, ImportCsvsResponse>(request);
            if (response.HasError)
                Assert.Fail(response.GetResponseItemsWithError().First().Exception.XrmDisplayString());
        }

        public void DeleteProductData()
        {
            var products = CsvUtility
                .SelectAllRows(Path.Combine(WorkFolder, @"Products.csv"))
                .Select(r => r.GetFieldAsString("Name"))
                .ToArray();
            DeleteAllMatchingName(Entities.product, products);
            var unitGroups = CsvUtility
                .SelectAllRows(Path.Combine(WorkFolder, @"uomschedule.csv"))
                .Select(r => r.GetFieldAsString("Name"))
                .ToArray();
            DeleteAllMatchingName(Entities.uomschedule, unitGroups);
            var priceLists = CsvUtility
                .SelectAllRows(Path.Combine(WorkFolder, @"Price Lists.csv"))
                .Select(r => r.GetFieldAsString("Name"))
                .ToArray();
            DeleteAllMatchingName(Entities.pricelevel, priceLists);
        }

        [TestMethod]
        public void DeploymentImportXmlPortalTypeConfigTest()
        {
            PrepareTests();
            var workFolder = ClearFilesAndData();

            //okay this script is to verify importing microsoft portal types
            //which aren't so simple as to just match by name
            //as for example webpage have a root page, and language specific page(s)

            RecreatePortalData(createSecondDuplicateSite: true);

            var parentWebPage = XrmService.RetrieveAllAndConditions(Entities.adx_webpage, new[]
            {
                new ConditionExpression(Fields.adx_webpage_.adx_rootwebpageid, ConditionOperator.Null)
            }).First();

            //export to xml
            var exportApp = CreateAndLoadTestApplication<ExportXmlModule>();
            exportApp.AddModule<AddPortalDataModule>();
            var exportDialog = exportApp.NavigateToDialog<ExportXmlModule, ExportXmlDialog>();
            var exportEntry = exportApp.GetSubObjectEntryViewModel(exportDialog);
            exportEntry.GetFieldViewModel<FolderFieldViewModel>(nameof(ExportXmlRequest.Folder)).Value = new Folder(workFolder);
            var dataToExport = exportEntry.GetEnumerableFieldViewModel(nameof(ExportXmlRequest.RecordTypesToExport));
            var addAllPortalDataButtonButton = dataToExport.DynamicGridViewModel.GridsFunctionsToXrmButtons(exportEntry.FormService.GetCustomFunctionsFor(dataToExport.FieldName, exportEntry));
            addAllPortalDataButtonButton.First(b => b.Id == "ADDPORTALDATA").ChildButtons.First().Invoke();
            Assert.IsTrue(exportEntry.Validate());
            exportEntry.SaveButtonViewModel.Invoke();
            var response = exportDialog.CompletionItem as ExportXmlResponse;
            Assert.IsFalse(response.HasError);

            //lets recreate all the web page data so the ids don't match when importing
            //this will verify it matches them based on the root/unique field configs in XrmTypesConfig
            RecreatePortalData(createSecondDuplicateSite: true);

            var application = CreateAndLoadTestApplication<ImportXmlModule>();
            var importRequest = new ImportXmlRequest
            {
                Folder = new Folder(workFolder)
            };

            var importResponse = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(importResponse.HasError);

            VerifyImportedPortalData();

            //lets just do several other things which will verify some other matching logic

            //delete the root page in the import files - this will verify the language specific page
            //will correctly resolve its parent independently
            //despite it having a different id and not being part of the import
            var parentWebPageFile = FileUtility.GetFiles(workFolder).First(f => f.Contains(parentWebPage.Id.ToString().Replace("-","_")));
            File.Delete(parentWebPageFile);

            importResponse = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(importResponse.HasError);

            VerifyImportedPortalData();

            //delete both web pages in the import files - this will verify the web page access contorl rule
            //will correctly resolve its parent independently
            //despite it having a different id and not being part of the import
            var webPageFile = FileUtility.GetFiles(workFolder).Where(f => f.Contains(Entities.adx_webpage + "_"));
            foreach (var file in webPageFile)
                File.Delete(file);

            importResponse = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(importResponse.HasError);

            VerifyImportedPortalData();

            //lets do the same but delete the web page access control rule
            //first to verify it also matches the parent when creating
            XrmService.Delete(XrmService.GetFirst(Entities.adx_webpageaccesscontrolrule));
            importResponse = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(importResponse.HasError);

            VerifyImportedPortalData();

            //okay lets do one last import where we delete all the data and do an import creating all the records new

            workFolder = ClearFilesAndData();
            //export again
            exportApp = CreateAndLoadTestApplication<ExportXmlModule>();
            exportApp.AddModule<AddPortalDataModule>();
            exportDialog = exportApp.NavigateToDialog<ExportXmlModule, ExportXmlDialog>();
            exportEntry = exportApp.GetSubObjectEntryViewModel(exportDialog);
            exportEntry.GetFieldViewModel<FolderFieldViewModel>(nameof(ExportXmlRequest.Folder)).Value = new Folder(workFolder);
            dataToExport = exportEntry.GetEnumerableFieldViewModel(nameof(ExportXmlRequest.RecordTypesToExport));
            addAllPortalDataButtonButton = dataToExport.DynamicGridViewModel.GridsFunctionsToXrmButtons(exportEntry.FormService.GetCustomFunctionsFor(dataToExport.FieldName, exportEntry));
            addAllPortalDataButtonButton.First(b => b.Id == "ADDPORTALDATA").ChildButtons.First().Invoke();
            Assert.IsTrue(exportEntry.Validate());
            exportEntry.SaveButtonViewModel.Invoke();
            response = exportDialog.CompletionItem as ExportXmlResponse;
            Assert.IsFalse(response.HasError);

            //delete all the data
            DeleteAllPortalData(dontDeleteTypes: new[] { Entities.adx_webpage, Entities.adx_websitelanguage, Entities.adx_website });

            //import
            importResponse = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(importResponse.HasError);

            //verify
            VerifyImportedPortalData();
        }

        private void VerifyImportedPortalData(int numberOfSites = 2)
        {
            var webSiteRecords = XrmService.RetrieveAllEntityType(Entities.adx_website);
            var webFileRecords = XrmService.RetrieveAllEntityType(Entities.adx_webfile);
            var webFileAttachmentRecords = XrmService.RetrieveAllOrClauses(Entities.annotation, webFileRecords.Select(e => new ConditionExpression(Fields.annotation_.objectid, ConditionOperator.Equal, e.Id)));
            var webLinkSetRecords = XrmService.RetrieveAllEntityType(Entities.adx_weblinkset);
            var webLinkRecords = XrmService.RetrieveAllEntityType(Entities.adx_weblink);
            var languageRecords = XrmService.RetrieveAllEntityType(Entities.adx_websitelanguage);
            var roleRecords = XrmService.RetrieveAllEntityType(Entities.adx_webrole);
            var pageRecords = XrmService.RetrieveAllEntityType(Entities.adx_webpage);
            var accessRecords = XrmService.RetrieveAllEntityType(Entities.adx_webpageaccesscontrolrule);
            var accessRoleAssociations = XrmService.RetrieveAllEntityType(Relationships.adx_webrole_.adx_webpageaccesscontrolrule_webrole.EntityName);
            var entityFormRecords = XrmService.RetrieveAllEntityType(Entities.adx_entityform);
            var entityFormMetadataRecords = XrmService.RetrieveAllEntityType(Entities.adx_entityformmetadata);
            var webFormRecords = XrmService.RetrieveAllEntityType(Entities.adx_webform);
            var webFormStepRecords = XrmService.RetrieveAllEntityType(Entities.adx_webformstep);
            var webFormMetadataRecords = XrmService.RetrieveAllEntityType(Entities.adx_webformmetadata);
            var webTemplateRecords = XrmService.RetrieveAllEntityType(Entities.adx_webtemplate);
            var entityListRecords = XrmService.RetrieveAllEntityType(Entities.adx_entitylist);
            var contentSnippetRecords = XrmService.RetrieveAllEntityType(Entities.adx_contentsnippet);
            var entityPermissionRecords = XrmService.RetrieveAllEntityType(Entities.adx_entitypermission);
            var pageTemplateRecords = XrmService.RetrieveAllEntityType(Entities.adx_pagetemplate);
            var publishingStateRecords = XrmService.RetrieveAllEntityType(Entities.adx_publishingstate);
            var siteSettingRecords = XrmService.RetrieveAllEntityType(Entities.adx_sitesetting);
            var siteMarkerRecords = XrmService.RetrieveAllEntityType(Entities.adx_sitemarker);

            //the records wont; have been updated as data the same - but we verify that the system matched
            //them and therefore didn't create duplicates or throw errors
            Assert.AreEqual(numberOfSites * 1, webSiteRecords.Count());
            Assert.AreEqual(numberOfSites * 2, webFileRecords.Count());
            Assert.AreEqual(numberOfSites * 1, webFileAttachmentRecords.Count());
            Assert.AreEqual(numberOfSites * 2, webLinkSetRecords.Count());
            Assert.AreEqual(numberOfSites * 5, webLinkRecords.Count());
            Assert.AreEqual(numberOfSites * 1, roleRecords.Count());
            Assert.IsTrue(numberOfSites * 4 == pageRecords.Count() || numberOfSites * 2 == pageRecords.Count());
            Assert.AreEqual(numberOfSites * 1, accessRecords.Count());
            Assert.AreEqual(numberOfSites * 1, accessRoleAssociations.Count());
            Assert.AreEqual(numberOfSites * 1, entityFormRecords.Count());
            Assert.AreEqual(numberOfSites * 3, entityFormMetadataRecords.Count());
            Assert.AreEqual(numberOfSites * 1, webFormRecords.Count());
            Assert.AreEqual(numberOfSites * 2, webFormStepRecords.Count());
            Assert.AreEqual(numberOfSites * 4, webFormMetadataRecords.Count());
            Assert.AreEqual(numberOfSites * 3, webTemplateRecords.Count());
            Assert.AreEqual(numberOfSites * 1, entityListRecords.Count());
            Assert.AreEqual(numberOfSites * 1, contentSnippetRecords.Count());
            Assert.AreEqual(numberOfSites * 1, entityPermissionRecords.Count());
            Assert.AreEqual(numberOfSites * 1, pageTemplateRecords.Count());
            Assert.AreEqual(numberOfSites * 1, publishingStateRecords.Count());
            Assert.AreEqual(numberOfSites * 1, siteSettingRecords.Count());
            Assert.AreEqual(numberOfSites * 1, siteMarkerRecords.Count());

            //verify the access control rule is correctly linked to the child web page
            var rootChildWebPage = pageRecords.First(e => e.GetLookupGuid(Fields.adx_webpage_.adx_parentpageid).HasValue && !e.GetLookupGuid(Fields.adx_webpage_.adx_rootwebpageid).HasValue);
            Assert.IsTrue(accessRecords.Any(ar => ar.GetLookupGuid(Fields.adx_webpageaccesscontrolrule_.adx_webpageid) == rootChildWebPage.Id));
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
            var response = exportService.Execute(exportRequest, new ServiceRequestController(Controller));
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

        [DeploymentItem(@"Files\mcgregor.jpg")]
        [TestMethod]
        public void DeploymentImportXmlWithFileFieldTest()
        {
            var fileBytes = File.ReadAllBytes("mcgregor.jpg");

            PrepareTests();
            var workFolder = ClearFilesAndData();

            var contactWithImage1 = CreateContact();
            XrmService.SetFileFieldBase64(contactWithImage1.LogicalName, contactWithImage1.Id, "entityimage", "macdonald.jpg", Convert.ToBase64String(fileBytes));
            var convertedBase64 = XrmService.GetFileFieldBase64(contactWithImage1.LogicalName, contactWithImage1.Id, "entityimage");
            var contactWithImage2 = CreateContact();
            XrmService.SetFileFieldBase64(contactWithImage2.LogicalName, contactWithImage2.Id, "entityimage", "macdonald.jpg", Convert.ToBase64String(fileBytes));
            var contactNoImage = CreateContact();

            contactWithImage1 = Refresh(contactWithImage1);

            var importService = new ImportXmlService(XrmRecordService);

            var exportService = new ExportXmlService(XrmRecordService);
            var exportRequest = new ExportXmlRequest
            {
                Folder = new Folder(workFolder),
                IncludeFileAndImageFields = true,
                RecordTypesToExport = new[] {
                    new ExportRecordType() {
                        RecordType = new RecordType(Entities.contact, Entities.contact),
                        Type = ExportType.SpecificRecords,
                        SpecificRecordsToExport = new []
                        {
                            new LookupSetting { Record = new Lookup(contactNoImage.LogicalName, contactNoImage.Id.ToString(), null)
                            },
                            new LookupSetting { Record = new Lookup(contactWithImage2.LogicalName, contactWithImage2.Id.ToString(), null) },
                            new LookupSetting { Record = new Lookup(contactWithImage1.LogicalName, contactWithImage1.Id.ToString(), null) }
                        }
                    } },
            };
            var exportResponse = exportService.Execute(exportRequest, new ServiceRequestController(Controller));
            Assert.IsTrue(exportResponse.Success);

            XrmService.Delete(contactWithImage1);
            XrmService.Delete(contactWithImage2);
            XrmService.Delete(contactNoImage);

            var application = CreateAndLoadTestApplication<ImportXmlModule>();

            var importRequest = new ImportXmlRequest
            {
                Folder = new Folder(workFolder)
            };

            var response = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            Assert.IsFalse(response.HasError);

            contactWithImage1 = Refresh(contactWithImage1);
            var imageBase64 = XrmService.GetFileFieldBase64(contactWithImage1.LogicalName, contactWithImage1.Id, "entityimage");
            Assert.IsNotNull(imageBase64);

            contactWithImage2 = Refresh(contactWithImage2);
            imageBase64 = XrmService.GetFileFieldBase64(contactWithImage2.LogicalName, contactWithImage2.Id, "entityimage");
            Assert.IsNotNull(imageBase64);

            contactNoImage = Refresh(contactNoImage);
            imageBase64 = XrmService.GetFileFieldBase64(contactNoImage.LogicalName, contactNoImage.Id, "entityimage");
            Assert.IsNull(imageBase64);
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
            var exportResponse = exportService.Execute(exportRequest, new ServiceRequestController(Controller));
            Assert.IsTrue(exportResponse.Success);

            XrmService.Delete(record);

            var application = CreateAndLoadTestApplication<ImportXmlModule>();

            var importRequest = new ImportXmlRequest
            {
                Folder = new Folder(workFolder)
            };

            var response = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
            if(response.HasError)
                Assert.Fail(response.GetResponseItemsWithError().First().Exception.DisplayString());
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
            var exportResponse = exportService.Execute(exportRequest, new ServiceRequestController(Controller));
            Assert.IsFalse(exportResponse.HasError);

            var entities = ImportXmlService.LoadEntitiesFromXmlFiles(workFolder);

            // ? self referencing (parentid) field created on test entity 
            //which meant extra one created
            Assert.AreEqual(8, entities.Count());

            var importRequest = new ImportXmlRequest
            {
                Folder = new Folder(workFolder)
            };
            var importResponse = importService.Execute(importRequest, new ServiceRequestController(Controller));
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
            var ecxportResponse = exportService.Execute(exportRequest, new ServiceRequestController(Controller));
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

            var type = Entities.knowledgearticle;
            if (XrmRecordService.RecordTypeExists(type))
            {
                DeleteAll(type);

                var workFolder = ClearFilesAndData();

                //create or get a knowledge article
                var knowledgeArticle = XrmService.GetFirst(type, Fields.knowledgearticle_.articlepublicnumber, PopulateStringValue);
                if (knowledgeArticle == null)
                    knowledgeArticle = CreateRecordAllFieldsPopulated(Entities.knowledgearticle);

                var articleNumber = knowledgeArticle.GetStringField(Fields.knowledgearticle_.articlepublicnumber);

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
                var ecxportResponse = exportService.Execute(exportRequest, new ServiceRequestController(Controller));
                Assert.IsFalse(ecxportResponse.HasError);

                //okay we will delete then create one with the same article number
                XrmService.Delete(knowledgeArticle);
                knowledgeArticle = CreateRecordAllFieldsPopulated(Entities.knowledgearticle, new Dictionary<string, object>
                {
                    { Fields.knowledgearticle_.articlepublicnumber, articleNumber }
                });

                var kaCount = XrmService.RetrieveAllAndConditions(type, new[]
                {
                    new ConditionExpression(Fields.knowledgearticle_.isrootarticle, ConditionOperator.Equal, false)
                }).Count();

                //import should match them
                var importRequest = new ImportXmlRequest
                {
                    Folder = new Folder(workFolder),
                    MaskEmails = true
                };
                var application = CreateAndLoadTestApplication<ImportXmlModule>();
                var importResponse = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
                Assert.IsFalse(importResponse.HasError);
                Assert.AreEqual(kaCount, XrmService.RetrieveAllAndConditions(type, new[]
                {
                    new ConditionExpression(Fields.knowledgearticle_.isrootarticle, ConditionOperator.Equal, false)
                }).Count());

                //now lets just verify for create (delete it prioor to import)
                XrmService.Delete(knowledgeArticle);
                importRequest = new ImportXmlRequest
                {
                    Folder = new Folder(workFolder),
                    MaskEmails = true
                };
                application = CreateAndLoadTestApplication<ImportXmlModule>();
                importResponse = application.NavigateAndProcessDialog<ImportXmlModule, ImportXmlDialog, ImportXmlResponse>(importRequest);
                Assert.IsFalse(importResponse.HasError);
                Assert.AreEqual(kaCount, XrmService.RetrieveAllAndConditions(type, new[]
                {
                    new ConditionExpression(Fields.knowledgearticle_.isrootarticle, ConditionOperator.Equal, false)
                }).Count());
            }
        }


        private Entity CreateTestRecord(string type, ImportXmlService importService)
        {
            var record = new Entity(type);
            var fields1 = GetFields(type, importService);
            foreach (var field in fields1)
            {
                if (field == XrmService.GetPrimaryNameField(type))
                {
                    record.SetField(field, DateTime.UtcNow.ToFileTimeUtc().ToString());
                }
                else
                {
                    record.SetField(field, CreateNewEntityFieldValue(field, type, record));
                }
            }
            record.Id = XrmService.Create(record);
            return record;
        }

        private IEnumerable<string> GetFields(string type, ImportXmlService importService)
        {
            var fields = XrmService
                .GetFields(type)
                .Where(f => DataImportContainer.IsIncludeField(f, type, importService.XrmRecordService, false, false));
            fields = fields.Except(new[] { "statecode", "statuscode" }).ToArray();
            return fields;
        }

        private IEnumerable<string> GetUpdateFields(string type, ImportXmlService importService)
        {
            var updateFields = XrmService.GetFields(type).Where(f => DataImportContainer.IsIncludeField(f, type, importService.XrmRecordService, false, false));
            return updateFields;
        }

        private string ClearFilesAndData(params string[] typesToDelete)
        {
            if (typesToDelete != null)
            {
                foreach (var type in typesToDelete)
                    DeleteAll(type);
            }
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