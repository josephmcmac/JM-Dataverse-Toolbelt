﻿#region

using JosephM.Core.FieldType;
using JosephM.CustomisationImporter.Prism;
using JosephM.CustomisationImporter.Service;
using JosephM.Prism.XrmModule.Test;
using JosephM.Record.Extentions;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Xrm.Schema;
using Microsoft.Xrm.Sdk;
using JosephM.Xrm;
using JosephM.Record.IService;

#endregion

namespace JosephM.CustomisationImporter.Test
{
    [TestClass]
    public class CustomisationImportTests : XrmModuleTest
    {
        [TestMethod]
        [DeploymentItem("TestCustomisations.xlsx")]
        [DeploymentItem(@"ContentFiles\Customisations Import Template.xlsx")]
        public void CustomisationImportTestImportModule()
        {
            //create test application with module loaded
            var testApplication = CreateAndLoadTestApplication<CustomisationImportModule>();

            //first script generation of C# entities and fields
            var request = TestCustomisationImportRequest.GetTestRequests(ExecutionPath).First();

            var response = testApplication.NavigateAndProcessDialog<CustomisationImportModule, XrmCustomisationImportDialog, CustomisationImportResponse>(request);
            Assert.IsFalse(response.HasError);
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

            var response = importService.Execute(request, Controller);
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

            response = importService.Execute(request, Controller);
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

            var importService =
                new XrmCustomisationImportService(XrmRecordService);

            foreach (var request in requests)
            {
                var response = importService.Execute(request, Controller);
                if (response.HasError)
                    Assert.Fail(response.GetResponseItemsWithError().First().Exception.DisplayString());

                Assert.IsFalse(response.ExcelReadErrors);
                Assert.IsNull(response.Exception);

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

            var importService =
                new XrmCustomisationImportService(XrmRecordService);

            var response = importService.Execute(request, Controller);

            Assert.IsTrue(response.ExcelReadErrors);
            Assert.IsNull(response.Exception);
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
            }
        }

        private static void VerifyOptionSetsEqual(IEnumerable<PicklistOption> actualOptions,
            IEnumerable<PicklistOption> expectedOption)
        {
            Assert.IsTrue(actualOptions.Count() == expectedOption.Count());
            foreach (var option in expectedOption)
                Assert.IsTrue(actualOptions.Any(o => o.Key == option.Key && o.Value == option.Value));
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
                if (metadata.RecordType1DisplayRelated)
                {
                    if (metadata.RecordType1UseCustomLabel)
                        Assert.AreEqual(loadedMetadata.RecordType1CustomLabel, metadata.RecordType1CustomLabel);
                    else
                        Assert.AreEqual(loadedMetadata.RecordType1CustomLabel, XrmRecordService.GetCollectionName(metadata.RecordType1));
                    Assert.AreEqual(loadedMetadata.RecordType1DisplayOrder, metadata.RecordType1DisplayOrder);
                }
                if (metadata.RecordType2DisplayRelated)
                {
                    if (metadata.RecordType2UseCustomLabel)
                        Assert.AreEqual(loadedMetadata.RecordType2CustomLabel, metadata.RecordType2CustomLabel);
                    else
                        Assert.AreEqual(loadedMetadata.RecordType2CustomLabel, XrmRecordService.GetCollectionName(metadata.RecordType2));
                    Assert.AreEqual(loadedMetadata.RecordType2DisplayOrder, metadata.RecordType2DisplayOrder);
                }
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

            foreach (var request in requests)
            {
                foreach (
                    var metadata in
                        CustomisationImportService.ExtractRelationshipMetadataFromExcel(request.ExcelFile.FileName,
                            Controller, response).Values)
                {
                    if (XrmRecordService.RecordTypeExists(metadata.RecordType1))
                    {
                        if (
                            XrmRecordService.GetManyToManyRelationships(metadata.RecordType1)
                                .Any(r => r.SchemaName == metadata.SchemaName))
                            XrmRecordService.DeleteRelationship(metadata.SchemaName);
                        Assert.IsFalse(
                            XrmRecordService.GetManyToManyRelationships(metadata.RecordType1)
                                .Any(r => r.SchemaName == metadata.SchemaName));
                    }
                }
            }
        }
    }
}