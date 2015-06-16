#region

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.FieldType;
using JosephM.CustomisationImporter.Service;
using JosephM.Record.Metadata;
using JosephM.Record.Xrm.Test;

#endregion

namespace JosephM.CustomisationImporter.Test
{
    [TestClass]
    public class TestCustomisationImportImport : XrmRecordTest
    {
        [TestMethod]
        [DeploymentItem("TestCustomisations.xls")]
        [DeploymentItem("TestCustomisationsUpdate.xls")]
        public void ExecuteTestImport()
        {
            return;

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
                Assert.IsFalse(response.HasError);
                ClearCache();

                VerifyRelationships(request);
                VerifyRecordTypes(request);
                VerifyFields(request);
                VerifyOptionSets(request);
                VerifyViews(request);
            }
        }

        private void VerifyViews(CustomisationImportRequest request)
        {
            var optionMetadata =
                CustomisationImportService.ExtractOptionSetsFromExcel(request.ExcelFile.FileName, Controller);
            var fieldMetadata =
                CustomisationImportService.ExtractFieldMetadataFromExcel(request.ExcelFile.FileName, Controller,
                    optionMetadata);
            var recordMetadata =
                CustomisationImportService.ExtractRecordMetadataFromExcel(request.ExcelFile.FileName, Controller,
                    fieldMetadata);
            foreach (var metadata in recordMetadata)
            {
                var viewNamesToUpdate = XrmRecordService.GetViewNamesToUpdate(metadata);
                var views = XrmRecordService.GetWhere("savedquery", "returnedtypecode",
                    metadata.SchemaName);
                foreach (var query in views.Where(sq => viewNamesToUpdate.Contains(sq.GetStringField("name"))))
                {
                    foreach (var viewField in metadata.Views.First().Fields)
                    {
                        if (query.ContainsField("fetchxml"))
                            Assert.IsTrue(
                                query.GetStringField("fetchxml")
                                    .Contains("<attribute name=\"" + viewField.FieldName + "\" />"));
                        if (query.ContainsField("layoutxml"))
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
            var optionMetadata =
                CustomisationImportService.ExtractOptionSetsFromExcel(request.ExcelFile.FileName, Controller);

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
            var optionMetadata =
                CustomisationImportService.ExtractOptionSetsFromExcel(request.ExcelFile.FileName, Controller);
            var fieldMetadata =
                CustomisationImportService.ExtractFieldMetadataFromExcel(request.ExcelFile.FileName, Controller,
                    optionMetadata);
            foreach (var field in fieldMetadata)
            {
                Assert.IsTrue(XrmRecordService.FieldExists(field.SchemaName, field.RecordType));
                Assert.IsTrue(XrmRecordService.GetFieldLabel(field.SchemaName, field.RecordType) ==
                              field.DisplayName);
                Assert.IsTrue(XrmRecordService.GetFieldDescription(field.SchemaName, field.RecordType) ==
                              field.Description);
                Assert.IsTrue(XrmRecordService.IsMandatory(field.SchemaName, field.RecordType) ==
                              field.IsMandatory);
                Assert.IsTrue(XrmRecordService.IsFieldAuditOn(field.SchemaName, field.RecordType) ==
                              field.Audit);
                Assert.IsTrue(XrmRecordService.IsFieldSearchable(field.SchemaName, field.RecordType) ==
                              field.Searchable);
                if (field.FieldType == RecordFieldType.String)
                {
                    Assert.IsTrue(XrmRecordService.GetMaxLength(field.SchemaName, field.RecordType) ==
                                  ((StringFieldMetadata) field).MaxLength);
                    Assert.IsTrue(XrmRecordService.GetTextFormat(field.SchemaName, field.RecordType) ==
                                  ((StringFieldMetadata) field).TextFormat);
                }
                if (field.FieldType == RecordFieldType.Integer)
                {
                    Assert.IsTrue(XrmRecordService.GetMinIntValue(field.SchemaName, field.RecordType) ==
                                  ((IntegerFieldMetadata) field).Minimum);
                    Assert.IsTrue(XrmRecordService.GetMaxIntValue(field.SchemaName, field.RecordType) ==
                                  ((IntegerFieldMetadata) field).Maximum);
                }
                if (field.FieldType == RecordFieldType.Decimal)
                {
                    Assert.IsTrue(XrmRecordService.GetMinDecimalValue(field.SchemaName, field.RecordType) ==
                                  ((DecimalFieldMetadata) field).Minimum);
                    Assert.IsTrue(XrmRecordService.GetMaxDecimalValue(field.SchemaName, field.RecordType) ==
                                  ((DecimalFieldMetadata) field).Maximum);
                    Assert.IsTrue(XrmRecordService.GetDecimalPrecision(field.SchemaName, field.RecordType) ==
              ((DecimalFieldMetadata)field).DecimalPrecision);
                }
                if (field.FieldType == RecordFieldType.Date)
                {
                    Assert.IsTrue(XrmRecordService.IsDateIncludeTime(field.SchemaName, field.RecordType) ==
                                  ((DateFieldMetadata) field).IncludeTime);
                }
                if (field.FieldType == RecordFieldType.Picklist)
                {
                    var actualOptions = XrmRecordService.GetPicklistKeyValues(field.SchemaName,
                        field.RecordType);
                    var expectedOption = ((PicklistFieldMetadata) field).PicklistOptions;
                    VerifyOptionSetsEqual(actualOptions, expectedOption);
                }
                if (field.FieldType == RecordFieldType.Double)
                {
                    Assert.IsTrue(XrmRecordService.GetMinDoubleValue(field.SchemaName, field.RecordType) ==
                                  ((DoubleFieldMetadata)field).Minimum);
                    Assert.IsTrue(XrmRecordService.GetMaxDoubleValue(field.SchemaName, field.RecordType) ==
                                  ((DoubleFieldMetadata)field).Maximum);
                    Assert.IsTrue(XrmRecordService.GetDecimalPrecision(field.SchemaName, field.RecordType) ==
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
            var optionMetadata =
                CustomisationImportService.ExtractOptionSetsFromExcel(request.ExcelFile.FileName, Controller);
            var fieldMetadata =
                CustomisationImportService.ExtractFieldMetadataFromExcel(request.ExcelFile.FileName, Controller,
                    optionMetadata);
            var recordMetadata =
                CustomisationImportService.ExtractRecordMetadataFromExcel(request.ExcelFile.FileName, Controller,
                    fieldMetadata);
            foreach (var metadata in recordMetadata)
            {
                Assert.IsTrue(XrmRecordService.RecordTypeExists(metadata.SchemaName));
                Assert.IsTrue(metadata.DisplayName == XrmRecordService.GetDisplayName(metadata.SchemaName));
                Assert.IsTrue(metadata.DisplayCollectionName ==
                              XrmRecordService.GetCollectionName(metadata.SchemaName));
                Assert.IsTrue(metadata.Audit == XrmRecordService.IsAuditOn(metadata.SchemaName));
            }
        }

        private void VerifyRelationships(CustomisationImportRequest request)
        {
            var relationShipmetadata =
                CustomisationImportService.ExtractRelationshipMetadataFromExcel(request.ExcelFile.FileName, Controller);
            foreach (var metadata in relationShipmetadata)
            {
                Assert.IsTrue(XrmRecordService.RelationshipExists(metadata.SchemaName));
                var relationshipMatch = XrmRecordService
                    .GetManyToManyRelationships(metadata.RecordType1)
                    .Where(r => r.SchemaName == metadata.SchemaName);
                Assert.IsTrue(relationshipMatch.Any());
                var relationship = relationshipMatch.First();
                Assert.AreEqual(XrmRecordService.IsDisplayRelated(relationship, false), metadata.RecordType1DisplayRelated);
                Assert.AreEqual(XrmRecordService.IsDisplayRelated(relationship, true), metadata.RecordType2DisplayRelated);
                Assert.AreEqual(XrmRecordService.IsCustomLabel(relationship, false), metadata.RecordType1UseCustomLabel);
                Assert.AreEqual(XrmRecordService.IsCustomLabel(relationship, true), metadata.RecordType2UseCustomLabel);
                if (metadata.RecordType1DisplayRelated)
                {
                    if (metadata.RecordType1UseCustomLabel)
                        Assert.AreEqual(XrmRecordService.GetRelationshipLabel(relationship, false), metadata.RecordType1CustomLabel);
                    else
                        Assert.AreEqual(XrmRecordService.GetRelationshipLabel(relationship, false), XrmRecordService.GetCollectionName(metadata.RecordType1));
                    Assert.AreEqual(XrmRecordService.GetDisplayOrder(relationship, false), metadata.RecordType1DisplayOrder);
                }
                if (metadata.RecordType2DisplayRelated)
                {
                    if (metadata.RecordType2UseCustomLabel)
                        Assert.AreEqual(XrmRecordService.GetRelationshipLabel(relationship, true), metadata.RecordType2CustomLabel);
                    else
                        Assert.AreEqual(XrmRecordService.GetRelationshipLabel(relationship, true), XrmRecordService.GetCollectionName(metadata.RecordType2));
                    Assert.AreEqual(XrmRecordService.GetDisplayOrder(relationship, true), metadata.RecordType2DisplayOrder);
                }
            }
        }

        private void DeleteOptionSets(IEnumerable<CustomisationImportRequest> requests)
        {
            foreach (var request in requests)
            {
                var optionMetadata =
                    CustomisationImportService.ExtractOptionSetsFromExcel(request.ExcelFile.FileName,
                        Controller);
                foreach (var metadata in optionMetadata)
                {
                    if (metadata.IsSharedOptionSet &&
                        XrmRecordService.SharedOptionSetExists(metadata.SchemaName))
                        XrmRecordService.DeleteSharedOptionSet(metadata.SchemaName);
                    Assert.IsFalse(XrmRecordService.SharedOptionSetExists(metadata.SchemaName));
                }
            }
        }

        private void DeleteEntities(IEnumerable<CustomisationImportRequest> requests)
        {
            foreach (var request in requests)
            {
                var optionMetadata =
                    CustomisationImportService.ExtractOptionSetsFromExcel(request.ExcelFile.FileName, Controller);
                var fieldMetadata =
                    CustomisationImportService.ExtractFieldMetadataFromExcel(request.ExcelFile.FileName, Controller,
                        optionMetadata);
                var recordMetadata =
                    CustomisationImportService.ExtractRecordMetadataFromExcel(request.ExcelFile.FileName, Controller,
                        fieldMetadata);
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
            foreach (var request in requests)
            {
                foreach (
                    var metadata in
                        CustomisationImportService.ExtractRelationshipMetadataFromExcel(request.ExcelFile.FileName,
                            Controller))
                {
                    if (XrmRecordService.RelationshipExists(metadata.SchemaName))
                        XrmRecordService.DeleteRelationship(metadata.SchemaName);
                    Assert.IsFalse(XrmRecordService.RelationshipExists(metadata.SchemaName));
                }
            }
        }
    }
}