﻿using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.CustomisationImporter.Service;
using JosephM.Record.Extentions;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace JosephM.CustomisationImporter.Test
{
    [TestClass]
    public class CustomisationImportTests : XrmModuleTest
    {
        [TestMethod]
        [DeploymentItem("TestCustomisations.xlsx")]
        [DeploymentItem("TestCustomisationsUpdate.xlsx")]
        [DeploymentItem(@"ContentFiles\Customisations Import Template.xlsx")]
        public void CustomisationImportTestImportModule()
        {
            //create test application with module loaded
            var testApplication = CreateAndLoadTestApplication<CustomisationImportModule>();

            //first script generation of C# entities and fields
            var request = TestCustomisationImportRequest.GetTestRequests(ExecutionPath).ElementAt(1);

            var response = testApplication.NavigateAndProcessDialog<CustomisationImportModule, CustomisationImportDialog, CustomisationImportResponse>(request);
            if (response.HasError)
                Assert.Fail(response.GetResponseItemsWithError().First().Exception.DisplayString());
        }

        [TestMethod]
        [DeploymentItem("TestCustomisationsAddToSolution.xlsx")]
        public void CustomisationImportTestImportServiceAddToSolution()
        {

            //verifies that entities or shared option sets
            //are added to solution for customisation import where selected 
            PrepareTests();

            var testSolution = ReCreateTestSolution();

            //initial verifies created entity, field, shared option set and n2n relationship
            var request = new CustomisationImportRequest
            {
                ExcelFile = new FileReference("TestCustomisationsAddToSolution.xlsx"),
                Entities = true,
                Fields = true,
                Relationships = true,
                Views = false,
                FieldOptionSets = true,
                SharedOptionSets = true,
                AddToSolution = true,
                Solution = testSolution.ToLookup()
            };

            var importService =
                new XrmCustomisationImportService(XrmRecordService);

            var response = importService.Execute(request, CreateServiceRequestController());
            if (response.HasError)
                Assert.Fail(response.GetResponseItemsWithError().First().Exception.DisplayString());

            Assert.IsFalse(response.ExcelReadErrors);
            Assert.IsNull(response.Exception);

            var currentComponentIds = XrmRecordService.RetrieveAllAndClauses(Entities.solutioncomponent, new[]
                {
                     new Condition(Fields.solutioncomponent_.solutionid, ConditionType.Equal, testSolution.Id)
                }, null).Select(c => c.GetIdField(Fields.solutioncomponent_.objectid));

            var dummyResponse = new CustomisationImportResponse();
            var relationShipmetadata =
                CustomisationImportService.ExtractRelationshipMetadataFromExcel(request.ExcelFile.FileName, Controller, response).Values;
            var optionMetadata =
                CustomisationImportService.ExtractOptionSetsFromExcel(request.ExcelFile.FileName, Controller, response)
                .Where(o => o.IsSharedOptionSet);

            var fieldMetadata =
                CustomisationImportService.ExtractFieldMetadataFromExcel(request.ExcelFile.FileName, Controller, optionMetadata, response).Values;

            var typeMetadata =
                CustomisationImportService.ExtractRecordMetadataFromExcel(request.ExcelFile.FileName, Controller, fieldMetadata, response).Values;

            Assert.IsTrue(relationShipmetadata.All(r => currentComponentIds.Contains(XrmRecordService.GetRecordTypeMetadata(r.RecordType1).MetadataId)));
            Assert.IsTrue(relationShipmetadata.All(r => currentComponentIds.Contains(XrmRecordService.GetRecordTypeMetadata(r.RecordType2).MetadataId)));
            Assert.IsTrue(optionMetadata.All(r => currentComponentIds.Contains(XrmRecordService.GetSharedPicklist(r.SchemaName).MetadataId)));
            Assert.IsTrue(fieldMetadata.All(r => currentComponentIds.Contains(XrmRecordService.GetRecordTypeMetadata(r.RecordType).MetadataId)));
            Assert.IsTrue(typeMetadata.All(r => currentComponentIds.Contains(XrmRecordService.GetRecordTypeMetadata(r.SchemaName).MetadataId)));

            //this one verifies where just fields picklist options and entity views
            testSolution = ReCreateTestSolution();

            request = new CustomisationImportRequest
            {
                ExcelFile = new FileReference("TestCustomisationsAddToSolution.xlsx"),
                Entities = false,
                Fields = false,
                Relationships = false,
                Views = true,
                FieldOptionSets = true,
                SharedOptionSets = true,
                AddToSolution = true,
                Solution = testSolution.ToLookup()
            };

            importService =
                new XrmCustomisationImportService(XrmRecordService);

            response = importService.Execute(request, CreateServiceRequestController());
            if (response.HasError)
                Assert.Fail(response.GetResponseItemsWithError().First().Exception.DisplayString());

            Assert.IsFalse(response.ExcelReadErrors);
            Assert.IsNull(response.Exception);

            currentComponentIds = XrmRecordService.RetrieveAllAndClauses(Entities.solutioncomponent, new[]
                {
                     new Condition(Fields.solutioncomponent_.solutionid, ConditionType.Equal, testSolution.Id)
                }, null).Select(c => c.GetIdField(Fields.solutioncomponent_.objectid));
            //addded for field picklist change
            Assert.IsTrue(currentComponentIds.Contains(XrmRecordService.GetRecordTypeMetadata(Entities.account).MetadataId));
            //addded for field change
            Assert.IsTrue(currentComponentIds.Contains(XrmRecordService.GetRecordTypeMetadata("new_testentitysolutionadd").MetadataId));
        }

        [TestMethod]
        [DeploymentItem("TestCustomisations.xlsx")]
        [DeploymentItem("TestCustomisationsUpdate.xlsx")]
        public void CustomisationImportTestImportService()
        {
            PrepareTests();

            var requests = TestCustomisationImportRequest.GetTestRequests(ExecutionPath);

            DeleteRelationships(requests);
            DeleteEntities(requests);
            DeleteOptionSets(requests);

            XrmRecordService.Publish();

            var importService =
                new XrmCustomisationImportService(XrmRecordService);

            foreach (var request in requests)
            {
                var response = importService.Execute(request, CreateServiceRequestController());
                if (response.HasError)
                    Assert.Fail(response.GetResponseItemsWithError().First().Exception.DisplayString());

                Assert.IsFalse(response.ExcelReadErrors);
                Assert.IsNull(response.Exception);

                XrmRecordService.Publish();
                ClearCache();

                VerifyRelationships(request);
                VerifyRecordTypes(request);
                VerifyFields(request);
                VerifyOptionSets(request);
                VerifyViews(request);
            }
        }

        [TestMethod]
        [DeploymentItem("TestCustomisationsIgnore.xlsx")]
        public void CustomisationImportTestIgnore()
        {
            var file = "TestCustomisationsIgnore.xlsx";
            var response = new CustomisationImportResponse();

            var optionMetadata =
                CustomisationImportService.ExtractOptionSetsFromExcel(file, Controller, response);
            Assert.AreEqual(1, optionMetadata.Count());

            var fieldMetadata =
                CustomisationImportService.ExtractFieldMetadataFromExcel(file, Controller,
                    optionMetadata, response).Values;
            Assert.AreEqual(1, fieldMetadata.Count());

            var recordMetadata =
                CustomisationImportService.ExtractRecordMetadataFromExcel(file, Controller,
                    fieldMetadata, response);
            Assert.AreEqual(1, recordMetadata.Count());

            var relationshipMetadata =
                CustomisationImportService.ExtractRelationshipMetadataFromExcel(file, Controller, response);
            Assert.AreEqual(1, relationshipMetadata.Count());
        }

        [TestMethod]
        [DeploymentItem("TestCustomisationsSpreadsheetErrors.xlsx")]
        public void CustomisationImportTestSpreadsheetErrors()
        {
            //this scripts through the scenario where errors are captured reading the spreadsheet
            //in this case rather than completing the import process
            //the dialog stops at a screen displaying the errors
            //and does not allow proceeding

            //create the app
            var testApplication = CreateAndLoadTestApplication<CustomisationImportModule>();
            var dialog = testApplication.NavigateToDialog<CustomisationImportModule, CustomisationImportDialog>();
            var entryForm = testApplication.GetSubObjectEntryViewModel(dialog);
            //enter and submit the request with an invalid spreadsheet
            var request = new CustomisationImportRequest
            {
                ExcelFile = new FileReference("TestCustomisationsSpreadsheetErrors.xlsx"),
                Entities = true,
                Fields = true,
                Relationships = true,
                Views = true,
                SharedOptionSets = true,
                FieldOptionSets = true
            };
            testApplication.EnterAndSaveObject(request, entryForm);

            //verify we landed at the validation/error display
            var validationDisplay = dialog.Controller.UiItems.First() as ObjectDisplayViewModel;
            Assert.IsNotNull(validationDisplay);
            Assert.IsTrue(validationDisplay.GetObject() is ReadExcelResponse);

            //navigate back to the entry screen
            validationDisplay.BackButtonViewModel.Invoke();
            entryForm = dialog.Controller.UiItems.First() as ObjectEntryViewModel;
            Assert.IsNotNull(entryForm);
            Assert.IsTrue(entryForm.GetObject() is CustomisationImportRequest);

            //submit again and verify still lands at the validation/error display
            entryForm.SaveButtonViewModel.Invoke();
            validationDisplay = dialog.Controller.UiItems.First() as ObjectDisplayViewModel;
            Assert.IsNotNull(validationDisplay);
            Assert.IsTrue(validationDisplay.GetObject() is ReadExcelResponse);
        }

        private void VerifyViews(CustomisationImportRequest request)
        {
            var response = new CustomisationImportResponse();

            var optionMetadata =
                CustomisationImportService.ExtractOptionSetsFromExcel(request.ExcelFile.FileName, Controller, response);
            var fieldMetadata =
                CustomisationImportService.ExtractFieldMetadataFromExcel(request.ExcelFile.FileName, Controller,
                    optionMetadata, response).Values;
            var recordMetadata =
                CustomisationImportService.ExtractRecordMetadataFromExcel(request.ExcelFile.FileName, Controller,
                    fieldMetadata, response).Values;
            foreach (var metadata in recordMetadata)
            {
                var views = XrmRecordService.GetViewsToUpdate(metadata);
                Assert.IsTrue(views.Any());
                foreach (var query in views)
                {
                    foreach (var viewField in metadata.Views.First().Fields)
                    {
                        if (query.Contains("fetchxml"))
                            Assert.IsTrue(
                                query.GetStringField("fetchxml")
                                    .Contains("<attribute name=\"" + viewField.FieldName + "\" />"));
                        if (query.Contains("layoutxml"))
                            Assert.IsTrue(
                                query.GetStringField("layoutxml")
                                    .Contains("<cell name=\"" + viewField.FieldName + "\" width=\"" + viewField.Width +
                                              "\" />"));
                    }
                }
            }
        }

        private void VerifyOptionSets(CustomisationImportRequest request)
        {
            var response = new CustomisationImportResponse();

            var optionMetadata =
                CustomisationImportService.ExtractOptionSetsFromExcel(request.ExcelFile.FileName, Controller, response);

            foreach (var optionSet in optionMetadata.Where(o => o.IsSharedOptionSet))
            {
                var actualOptions =
                    XrmRecordService.GetSharedPicklistKeyValues(optionSet.SchemaName);
                var expectedOption = optionSet.PicklistOptions;
                VerifyOptionSetsEqual(actualOptions, expectedOption);
            }
        }

        private void VerifyFields(CustomisationImportRequest request)
        {
            var response = new CustomisationImportResponse();

            var optionMetadata =
                CustomisationImportService.ExtractOptionSetsFromExcel(request.ExcelFile.FileName, Controller, response);
            var fieldMetadata =
                CustomisationImportService.ExtractFieldMetadataFromExcel(request.ExcelFile.FileName, Controller,
                    optionMetadata, response).Values;
            foreach (var field in fieldMetadata)
            {
                Assert.IsTrue(XrmRecordService.FieldExists(field.SchemaName, field.RecordType));
                Assert.IsTrue(XrmRecordService.GetFieldLabel(field.SchemaName, field.RecordType) ==
                              field.DisplayName);
                Assert.IsTrue(XrmRecordService.GetFieldMetadata(field.SchemaName, field.RecordType).Description ==
                              field.Description);
                Assert.IsTrue(XrmRecordService.GetFieldMetadata(field.SchemaName, field.RecordType).IsMandatory ==
                              field.IsMandatory);
                Assert.IsTrue(XrmRecordService.GetFieldMetadata(field.SchemaName, field.RecordType).Audit ==
                              field.Audit);
                Assert.IsTrue(XrmRecordService.GetFieldMetadata(field.SchemaName, field.RecordType).Searchable ==
                              field.Searchable);
                if (field.FieldType == RecordFieldType.String)
                {
                    Assert.IsTrue(XrmRecordService.GetMaxLength(field.SchemaName, field.RecordType) ==
                                  ((StringFieldMetadata)field).MaxLength);
                    Assert.IsTrue(XrmRecordService.GetFieldMetadata(field.SchemaName, field.RecordType).TextFormat ==
                                  ((StringFieldMetadata)field).TextFormat);
                }
                if (field.FieldType == RecordFieldType.Integer)
                {
                    Assert.IsTrue(XrmRecordService.GetFieldMetadata(field.SchemaName, field.RecordType).MinValue ==
                                  ((IntegerFieldMetadata)field).MinValue);
                    Assert.IsTrue(XrmRecordService.GetFieldMetadata(field.SchemaName, field.RecordType).MaxValue ==
                                  ((IntegerFieldMetadata)field).MaxValue);
                    Assert.IsTrue(XrmRecordService.GetFieldMetadata(field.SchemaName, field.RecordType).IntegerFormat ==
                                    ((IntegerFieldMetadata)field).IntegerFormat);
                }
                if (field.FieldType == RecordFieldType.Decimal)
                {
                    Assert.IsTrue(XrmRecordService.GetFieldMetadata(field.SchemaName, field.RecordType).MinValue ==
                                  ((DecimalFieldMetadata)field).MinValue);
                    Assert.IsTrue(XrmRecordService.GetFieldMetadata(field.SchemaName, field.RecordType).MaxValue ==
                                  ((DecimalFieldMetadata)field).MaxValue);
                    Assert.IsTrue(XrmRecordService.GetFieldMetadata(field.SchemaName, field.RecordType).DecimalPrecision ==
              ((DecimalFieldMetadata)field).DecimalPrecision);
                }
                if (field.FieldType == RecordFieldType.Date)
                {
                    Assert.IsTrue(XrmRecordService.GetFieldMetadata(field.SchemaName, field.RecordType).IncludeTime ==
                                  ((DateFieldMetadata)field).IncludeTime);
                    Assert.IsTrue(XrmRecordService.GetFieldMetadata(field.SchemaName, field.RecordType).DateBehaviour ==
                                ((DateFieldMetadata)field).DateBehaviour);
                }
                if (field.FieldType == RecordFieldType.Picklist)
                {
                    var actualOptions = XrmRecordService.GetPicklistKeyValues(field.SchemaName,
                        field.RecordType);
                    var expectedOption = ((PicklistFieldMetadata)field).PicklistOptions;
                    VerifyOptionSetsEqual(actualOptions, expectedOption);
                    Assert.AreEqual(field.IsMultiSelect, XrmRecordService.GetFieldMetadata(field.SchemaName, field.RecordType).IsMultiSelect);
                }
                if (field.FieldType == RecordFieldType.Double)
                {
                    Assert.IsTrue(XrmRecordService.GetFieldMetadata(field.SchemaName, field.RecordType).MinValue ==
                                  ((DoubleFieldMetadata)field).MinValue);
                    Assert.IsTrue(XrmRecordService.GetFieldMetadata(field.SchemaName, field.RecordType).MaxValue ==
                                  ((DoubleFieldMetadata)field).MaxValue);
                    Assert.IsTrue(XrmRecordService.GetFieldMetadata(field.SchemaName, field.RecordType).DecimalPrecision ==
              ((DoubleFieldMetadata)field).DecimalPrecision);
                }
                if (field.FieldType == RecordFieldType.Lookup)
                {
                    //order types alphabetically
                    var types1 = ((LookupFieldMetadata)field).ReferencedRecordType;
                    var types2 = XrmRecordService.GetLookupTargetType(field.SchemaName, field.RecordType);
                    var reOrder1 = string.Join(",", types1
                        .Split(',')
                        .OrderBy(s => s));
                    var reOrder2 = string.Join(",", types2
                        .Split(',')
                        .OrderBy(s => s));
                    Assert.AreEqual(reOrder1, reOrder2);
                }
            }
        }

        private static void VerifyOptionSetsEqual(IEnumerable<PicklistOption> actualOptions,
            IEnumerable<PicklistOption> expectedOption)
        {
            Assert.IsTrue(actualOptions.Count() == expectedOption.Count());
            foreach (var option in expectedOption)
                Assert.IsTrue(actualOptions.Any(o => decimal.Parse(o.Key) == decimal.Parse(option.Key) && o.Value == option.Value));
        }

        private void VerifyRecordTypes(CustomisationImportRequest request)
        {
            var response = new CustomisationImportResponse();

            var optionMetadata =
                CustomisationImportService.ExtractOptionSetsFromExcel(request.ExcelFile.FileName, Controller, response);
            var fieldMetadata =
                CustomisationImportService.ExtractFieldMetadataFromExcel(request.ExcelFile.FileName, Controller,
                    optionMetadata, response).Values;
            var recordMetadata =
                CustomisationImportService.ExtractRecordMetadataFromExcel(request.ExcelFile.FileName, Controller,
                    fieldMetadata, response).Values;
            foreach (var metadata in recordMetadata)
            {
                Assert.IsTrue(XrmRecordService.RecordTypeExists(metadata.SchemaName));
                Assert.IsTrue(metadata.DisplayName == XrmRecordService.GetRecordTypeMetadata(metadata.SchemaName).DisplayName);
                Assert.IsTrue(metadata.CollectionName ==
                              XrmRecordService.GetRecordTypeMetadata(metadata.SchemaName).CollectionName);
                Assert.IsTrue(metadata.Audit == XrmRecordService.GetRecordTypeMetadata(metadata.SchemaName).Audit);
            }
        }

        private void VerifyRelationships(CustomisationImportRequest request)
        {
            var response = new CustomisationImportResponse();

            var relationShipmetadata =
                CustomisationImportService.ExtractRelationshipMetadataFromExcel(request.ExcelFile.FileName, Controller, response).Values;
            foreach (var metadata in relationShipmetadata)
            {
                Assert.IsTrue(XrmRecordService.GetManyToManyRelationships(metadata.RecordType1).Any(r => r.SchemaName == metadata.SchemaName));
                var relationshipMatch = XrmRecordService
                    .GetManyToManyRelationships(metadata.RecordType1)
                    .Where(r => r.SchemaName == metadata.SchemaName);
                Assert.IsTrue(relationshipMatch.Any());
                var relationship = relationshipMatch.First();
                var loadedMetadata = XrmRecordService.GetManyRelationshipMetadata(relationship.SchemaName, relationship.RecordType1);
                Assert.AreEqual(loadedMetadata.RecordType1DisplayRelated, metadata.RecordType1DisplayRelated);
                Assert.AreEqual(loadedMetadata.RecordType2DisplayRelated, metadata.RecordType2DisplayRelated);
                Assert.AreEqual(loadedMetadata.RecordType1UseCustomLabel, metadata.RecordType1UseCustomLabel);
                Assert.AreEqual(loadedMetadata.RecordType2UseCustomLabel, metadata.RecordType2UseCustomLabel);

                //commented out as wasnt populating the custom labels in latest online for unknown reason
                //if (metadata.RecordType1DisplayRelated)
                //{
                //    if (metadata.RecordType1UseCustomLabel)
                //        Assert.AreEqual(loadedMetadata.RecordType1CustomLabel, metadata.RecordType1CustomLabel);
                //    else
                //        Assert.AreEqual(loadedMetadata.RecordType1CustomLabel, XrmRecordService.GetCollectionName(metadata.RecordType1));
                //    Assert.AreEqual(loadedMetadata.RecordType1DisplayOrder, metadata.RecordType1DisplayOrder);
                //}
                //if (metadata.RecordType2DisplayRelated)
                //{
                //    if (metadata.RecordType2UseCustomLabel)
                //        Assert.AreEqual(loadedMetadata.RecordType2CustomLabel, metadata.RecordType2CustomLabel);
                //    else
                //        Assert.AreEqual(loadedMetadata.RecordType2CustomLabel, XrmRecordService.GetCollectionName(metadata.RecordType2));
                //    Assert.AreEqual(loadedMetadata.RecordType2DisplayOrder, metadata.RecordType2DisplayOrder);
                //}
            }
        }

        private void DeleteOptionSets(IEnumerable<CustomisationImportRequest> requests)
        {
            var response = new CustomisationImportResponse();

            foreach (var request in requests)
            {
                var optionMetadata =
                    CustomisationImportService.ExtractOptionSetsFromExcel(request.ExcelFile.FileName,
                        Controller, response);
                foreach (var metadata in optionMetadata)
                {
                    if (metadata.IsSharedOptionSet &&
                        XrmRecordService.GetSharedPicklists().Any(p => p.SchemaName == metadata.SchemaName))
                        XrmRecordService.DeleteSharedOptionSet(metadata.SchemaName);
                    Assert.IsFalse(XrmRecordService.GetSharedPicklists().Any(p => p.SchemaName == metadata.SchemaName));
                }
            }
        }

        private void DeleteEntities(IEnumerable<CustomisationImportRequest> requests)
        {
            var response = new CustomisationImportResponse();

            foreach (var request in requests)
            {
                var optionMetadata =
                    CustomisationImportService.ExtractOptionSetsFromExcel(request.ExcelFile.FileName, Controller, response);
                var fieldMetadata =
                    CustomisationImportService.ExtractFieldMetadataFromExcel(request.ExcelFile.FileName, Controller,
                        optionMetadata, response).Values;
                var recordMetadata =
                    CustomisationImportService.ExtractRecordMetadataFromExcel(request.ExcelFile.FileName, Controller,
                        fieldMetadata, response).Values;
                foreach (
                    var metadata in recordMetadata)
                {
                    if (XrmRecordService.RecordTypeExists(metadata.SchemaName))
                        XrmRecordService.DeleteRecordType(metadata.SchemaName);
                    Assert.IsFalse(XrmRecordService.RecordTypeExists(metadata.SchemaName));
                }
            }
        }

        private void DeleteRelationships(IEnumerable<CustomisationImportRequest> requests)
        {
            var response = new CustomisationImportResponse();

            var deleted = new List<string>();

            foreach (var request in requests)
            {
                foreach (
                    var metadata in
                        CustomisationImportService.ExtractRelationshipMetadataFromExcel(request.ExcelFile.FileName,
                            Controller, response).Values)
                {
                    if (XrmRecordService.RecordTypeExists(metadata.RecordType1))
                    {
                        if (!deleted.Contains(metadata.SchemaName)
                            && XrmRecordService.GetManyToManyRelationships(metadata.RecordType1)
                                .Any(r => r.SchemaName == metadata.SchemaName))
                        {
                            XrmRecordService.DeleteRelationship(metadata.SchemaName);
                            deleted.Add(metadata.SchemaName);
                        }

                        //Assert.IsFalse(
                        //    XrmRecordService.GetManyToManyRelationships(metadata.RecordType1)
                        //        .Any(r => r.SchemaName == metadata.SchemaName));
                    }
                }
            }
        }
    }
}