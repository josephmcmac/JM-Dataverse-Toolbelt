#region

using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Spreadsheet;
using JosephM.Xrm.MetadataImportExport;

#endregion

namespace JosephM.CustomisationImporter.Service
{
    public class CustomisationImportService :
        ServiceBase<CustomisationImportRequest, CustomisationImportResponse, CustomisationImportResponseItem>
    {
        private const string EntityTabName = "Record Types";
        private const string RelationshipTabName = "Relationships";
        private const string OptionSetsTabName = "Option Sets";
        private const string FieldsTabName = "Fields";

        public CustomisationImportService(IRecordService recordService)
        {
            RecordService = recordService;
        }

        private IRecordService RecordService { get; set; }

        public override void ExecuteExtention(CustomisationImportRequest request, CustomisationImportResponse response,
            LogController controller)
        {
            controller.LogLiteral("Reading from excel");
            var optionSets = ExtractOptionSetsFromExcel(request.ExcelFile.FileName,
                controller);
            var fieldMetadataToImport = ExtractFieldMetadataFromExcel(
                request.ExcelFile.FileName, controller, optionSets);
            var recordMetadataToImport =
                ExtractRecordMetadataFromExcel(request.ExcelFile.FileName, controller, fieldMetadataToImport);
            var relationshipMetadataToImport =
                ExtractRelationshipMetadataFromExcel(request.ExcelFile.FileName, controller);

            CheckCrmConnection(controller);

            if (request.IncludeEntities)
                ImportRecordTypes(recordMetadataToImport, controller, response);
            if (request.UpdateOptionSets)
                ImportSharedOptionSets(optionSets, controller, response);
            if (request.IncludeFields)
                ImportFieldTypes(fieldMetadataToImport, controller, response);
            if (request.UpdateOptionSets)
                ImportFieldOptionSets(recordMetadataToImport, controller, response);
            if (request.UpdateViews)
                ImportViews(recordMetadataToImport, controller, response);
            if (request.IncludeRelationships)
                ImportRelationships(relationshipMetadataToImport, controller, response);
            controller.LogLiteral("Publishing Changes");
            RecordService.PublishAll();
            RecordService.ClearCache();
        }

        public static IEnumerable<FieldMetadata> ExtractFieldMetadataFromExcel(string excelFile,
            LogController controller,
            IEnumerable<PicklistOptionSet>
                picklistOptionSets)
        {
            var rows = ExcelUtility.SelectPropertyBagsFromExcelTabName(excelFile,
                FieldsTabName);
            var fields = new List<FieldMetadata>();
            foreach (var row in rows)
            {
                FieldMetadata fieldMetadata = null;
                var type = row.GetFieldAsEnum<RecordFieldType>(Headings.Fields.FieldType);

                var fieldSchemaName = row.GetFieldAsString(Headings.Fields.SchemaName);
                if (!String.IsNullOrWhiteSpace(fieldSchemaName))
                {
                    var recordTypeSchemaName = row.GetFieldAsString(Headings.Fields.RecordTypeSchemaName);
                    try
                    {
                        var displayName = row.GetFieldAsString(Headings.Fields.DisplayName);
                        switch (type)
                        {
                            case (RecordFieldType.Boolean):
                            {
                                fieldMetadata = new BooleanFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                    displayName);
                                break;
                            }
                            case (RecordFieldType.Date):
                            {
                                fieldMetadata = new DateFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                    displayName)
                                {
                                    IncludeTime = row.GetFieldAsBoolean(Headings.Fields.IncludeTime)
                                };
                                break;
                            }
                            case (RecordFieldType.Decimal):
                            {
                                fieldMetadata = new DecimalFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                    displayName)
                                {
                                    Minimum = row.GetFieldAsDecimal(Headings.Fields.Minimum),
                                    Maximum = row.GetFieldAsDecimal(Headings.Fields.Maximum)
                                };
                                break;
                            }
                            case (RecordFieldType.Integer):
                            {
                                fieldMetadata = new IntegerFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                    displayName)
                                {
                                    Minimum = row.GetFieldAsInteger(Headings.Fields.Minimum),
                                    Maximum = row.GetFieldAsInteger(Headings.Fields.Maximum)
                                };
                                break;
                            }
                            case (RecordFieldType.Lookup):
                            {
                                fieldMetadata = new LookupFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                    displayName,
                                    row.GetFieldAsString(Headings.Fields.ReferencedRecordType))
                                {
                                    DisplayInRelated = row.GetFieldAsBoolean(Headings.Fields.DisplayInRelated)
                                };
                                break;
                            }
                            case (RecordFieldType.Money):
                            {
                                fieldMetadata = new MoneyFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                    displayName)
                                {
                                    Minimum = row.GetFieldAsDouble(Headings.Fields.Minimum),
                                    Maximum = row.GetFieldAsDouble(Headings.Fields.Maximum)
                                };
                                break;
                            }
                            case (RecordFieldType.Picklist):
                            {
                                var optionSetName = row.GetFieldAsString(Headings.Fields.PicklistOptions);
                                var optionSet = new PicklistOptionSet();
                                if (!string.IsNullOrWhiteSpace(optionSetName))
                                {
                                    if (picklistOptionSets.Any(p => p.DisplayName == optionSetName))
                                        optionSet = picklistOptionSets.First(p => p.DisplayName == optionSetName);
                                }
                                fieldMetadata = new PicklistFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                    displayName, optionSet);
                                break;
                            }
                            case (RecordFieldType.String):
                            {
                                fieldMetadata = new StringFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                    displayName)
                                {
                                    MaxLength = row.GetFieldAsInteger(Headings.Fields.MaxLength),
                                    TextFormat =
                                        string.IsNullOrWhiteSpace(row.GetFieldAsString(Headings.Fields.TextFormat))
                                            ? TextFormat.Text
                                            : row.GetFieldAsEnum<TextFormat>(Headings.Fields.TextFormat),
                                    IsPrimaryField = row.GetFieldAsBoolean(Headings.Fields.IsPrimaryField)
                                };
                                break;
                            }
                            case RecordFieldType.Memo:
                            {
                                fieldMetadata = new MemoFieldMetadata(recordTypeSchemaName, fieldSchemaName, displayName)
                                {
                                    MaxLength = row.GetFieldAsInteger(Headings.Fields.MaxLength)
                                };
                                break;
                            }
                            case RecordFieldType.Status:
                            {
                                fieldMetadata = new StatusFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                    displayName, null);
                                break;
                            }
                            case (RecordFieldType.Double):
                            {
                                fieldMetadata = new DoubleFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                    displayName)
                                {
                                    Minimum = row.GetFieldAsDouble(Headings.Fields.Minimum),
                                    Maximum = row.GetFieldAsDouble(Headings.Fields.Maximum)
                                };
                                break;
                            }
                            case (RecordFieldType.Uniqueidentifier):
                            {
                                fieldMetadata = new UniqueidentifierFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                    displayName);
                                break;
                            }
                            case (RecordFieldType.State):
                            {
                                fieldMetadata = new StateFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                    displayName);
                                break;
                            }
                            case (RecordFieldType.BigInt):
                            {
                                fieldMetadata = new BigIntFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                    displayName);
                                break;
                            }
                            default:
                            {
                                fieldMetadata = new AnyFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                    displayName, type);
                                break;
                            }
                        }

                        fieldMetadata.Description = row.GetFieldAsString(Headings.Fields.Description);
                        fieldMetadata.IsMandatory = row.GetFieldAsBoolean(Headings.Fields.IsMandatory);
                        fieldMetadata.Audit = row.GetFieldAsBoolean(Headings.Fields.Audit);
                        fieldMetadata.Searchable = row.GetFieldAsBoolean(Headings.Fields.Searchable);
                        fields.Add(fieldMetadata);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            "Error retrieving field from excel\nRecord Type: " + recordTypeSchemaName + "\nField: " +
                            fieldSchemaName, ex);
                    }
                }
            }
            return fields;
        }

        private void ImportFieldOptionSets(IEnumerable<RecordMetadata> recordMetadata, LogController controller,
            CustomisationImportResponse response)
        {
            var picklistFields = recordMetadata
                .SelectMany(m => m.Fields)
                .Where(
                    f => f is PicklistFieldMetadata && !((PicklistFieldMetadata) f).PicklistOptionSet.IsSharedOptionSet)
                .Cast<PicklistFieldMetadata>()
                .ToArray();
            var numberToDo = picklistFields.Count();
            var numberCompleted = 0;
            for (var i = 0; i < picklistFields.Count(); i++)
            {
                var field = picklistFields[i];
                if (field.PicklistOptionSet != null && field.PicklistOptionSet.PicklistOptions.Any())
                {
                    try
                    {
                        controller.UpdateProgress(numberCompleted++, numberToDo, "Importing Field Option Sets");
                        var entityType = recordMetadata.First(rm => rm.Fields.Contains(field)).SchemaName;
                        RecordService.UpdateFieldOptionSet(entityType, field.SchemaName,
                            field.PicklistOptionSet);
                        response.AddResponseItem("Field Option Set", field.SchemaName, true);
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem("Field Option Set", field.SchemaName, ex);
                    }
                }
            }
        }

        private void ImportSharedOptionSets(IEnumerable<PicklistOptionSet> optionSets, LogController controller,
            CustomisationImportResponse importResponse)
        {
            var sharedOptionSets = optionSets.Where(o => o.IsSharedOptionSet).ToArray();
            var numberToDo = sharedOptionSets.Count();
            var numberCompleted = 0;
            for (var i = 0; i < sharedOptionSets.Count(); i++)
            {
                var sharedOptionSet = sharedOptionSets[i];
                try
                {
                    controller.UpdateProgress(numberCompleted++, numberToDo, "Importing Shared Option Sets");
                    var isUpdate = RecordService.SharedOptionSetExists(sharedOptionSet.SchemaName);
                    RecordService.CreateOrUpdateSharedOptionSet(sharedOptionSet);

                    importResponse.AddResponseItem("Shared Option Set", sharedOptionSet.SchemaName, isUpdate);
                }
                catch (Exception ex)
                {
                    importResponse.AddResponseItem("Shared Option Set", sharedOptionSet.SchemaName, ex);
                }
            }
        }

        private void ImportViews(IEnumerable<RecordMetadata> metadata, LogController controller,
            CustomisationImportResponse response)
        {
            var numberToDo = metadata.Count();
            var numberCompleted = 0;
            for (var i = 0; i < metadata.Count(); i++)
            {
                var recordMetadata = metadata.ElementAt(i);
                try
                {
                    controller.UpdateProgress(numberCompleted++, numberToDo, "Importing views");
                    var isUpdate = RecordService.RecordTypeExists(recordMetadata.SchemaName);
                    if (recordMetadata.Views.Any(f => f.Fields.Any(g => g.Order >= 0 && g.Width > 0)))
                        RecordService.UpdateViews(recordMetadata);
                    else
                        continue;
                    response.AddResponseItem("Entity Views", recordMetadata.SchemaName, isUpdate);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem("Entity Views", recordMetadata.SchemaName, ex);
                }
            }
        }

        private void CheckCrmConnection(LogController controller)
        {
            controller.LogLiteral("Verifying Connection");
            RecordService.VerifyConnection();
        }

        private void ImportRelationships(IEnumerable<RelationshipMetadata> metadata, LogController controller,
            CustomisationImportResponse response)
        {
            var numberToDo = metadata.Count();
            var numberCompleted = 0;
            for (var i = 0; i < metadata.Count(); i++)
            {
                var recordMetadata = metadata.ElementAt(i);
                try
                {
                    controller.UpdateProgress(numberCompleted++, numberToDo, "Importing relationships");
                    var isUpdate = RecordService.RelationshipExists(recordMetadata.SchemaName);
                    RecordService.CreateOrUpdate(recordMetadata);
                    response.AddResponseItem("Relationship", recordMetadata.SchemaName, isUpdate);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem("Relationship", recordMetadata.SchemaName, ex);
                }
            }
        }

        private void ImportFieldTypes(IEnumerable<FieldMetadata> metadata, LogController controller,
            CustomisationImportResponse response)
        {
            var numberToDo = metadata.Count();
            var numberCompleted = 0;
            for (var i = 0; i < metadata.Count(); i++)
            {
                var field = metadata.ElementAt(i);
                try
                {
                    controller.UpdateProgress(numberCompleted++, numberToDo, "Importing fields");
                    var isUpdate = RecordService.FieldExists(field.SchemaName, field.RecordType);
                    RecordService.CreateOrUpdate(field, field.RecordType);
                    response.AddResponseItem("Field", field.SchemaName, isUpdate);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem("Field", field.SchemaName, ex);
                }
            }
        }

        private void ImportRecordTypes(IEnumerable<RecordMetadata> metadata, LogController controller,
            CustomisationImportResponse response)
        {
            var numberToDo = metadata.Count();
            var numberCompleted = 0;
            for (var i = 0; i < metadata.Count(); i++)
            {
                var recordMetadata = metadata.ElementAt(i);
                try
                {
                    controller.UpdateProgress(numberCompleted++, numberToDo, "Importing record types");
                    var isUpdate = RecordService.RecordTypeExists(recordMetadata.SchemaName);
                    RecordService.CreateOrUpdate(recordMetadata);
                    if (!isUpdate)
                        response.AddResponseItem("Field", recordMetadata.GetPrimaryFieldMetadata().SchemaName, false);
                    response.AddResponseItem("Record Type", recordMetadata.SchemaName, isUpdate);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem("Record Type", recordMetadata.SchemaName, ex);
                }
            }
        }

        /// <summary>
        ///     Reads a set of mappings from the Excel file which contains the entity and field mapping metadata
        /// </summary>
        internal static IEnumerable<RelationshipMetadata> ExtractRelationshipMetadataFromExcel(string excelFile,
            LogController controller)
        {
            var result = new List<RelationshipMetadata>();

            var rows = ExcelUtility.SelectPropertyBagsFromExcelTabName(excelFile,
                RelationshipTabName);
            //For each row
            foreach (var row in rows)
            {
                var relationshipName = row.GetFieldAsString(Headings.Relationships.RelationshipName);
                if (!string.IsNullOrWhiteSpace(relationshipName))
                {
                    try
                    {
                        //create the table to enitty mapping for the sheet
                        var mapping = new RelationshipMetadata
                        {
                            SchemaName = relationshipName,
                            RecordType1 = row.GetFieldAsString(Headings.Relationships.RecordType1),
                            RecordType2 = row.GetFieldAsString(Headings.Relationships.RecordType2),
                            RecordType1DisplayRelated = row.GetFieldAsBoolean(Headings.Relationships.RecordType1DisplayRelated),
                            RecordType2DisplayRelated = row.GetFieldAsBoolean(Headings.Relationships.RecordType2DisplayRelated),
                            RecordType1UseCustomLabel = row.GetFieldAsBoolean(Headings.Relationships.RecordType1UseCustomLabel),
                            RecordType2UseCustomLabel = row.GetFieldAsBoolean(Headings.Relationships.RecordType2UseCustomLabel),
                            RecordType1CustomLabel = row.GetFieldAsString(Headings.Relationships.RecordType1CustomLabel),
                            RecordType2CustomLabel = row.GetFieldAsString(Headings.Relationships.RecordType2CustomLabel),
                            RecordType1DisplayOrder = row.GetFieldAsBoolean(Headings.Relationships.RecordType1DisplayRelated) ? row.GetFieldAsInteger(Headings.Relationships.RecordType1DisplayOrder) : -1,
                            RecordType2DisplayOrder = row.GetFieldAsBoolean(Headings.Relationships.RecordType2DisplayRelated) ? row.GetFieldAsInteger(Headings.Relationships.RecordType2DisplayOrder) : -1
                        };
                        result.Add(mapping);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error reading relationship from excel: " + relationshipName, ex);
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///     Reads a set of mappings from the Excel file which contains the entity and field mapping metadata
        /// </summary>
        internal static IEnumerable<RecordMetadata> ExtractRecordMetadataFromExcel(string excelFile,
            LogController controller,
            IEnumerable<FieldMetadata>
                fieldMetadata)
        {
            var result = new List<RecordMetadata>();

            var rows = ExcelUtility.SelectPropertyBagsFromExcelTabName(excelFile,
                EntityTabName);
            //For each row
            foreach (var row in rows)
            {
                var schemaName = row.GetFieldAsString(Headings.RecordTypes.SchemaName);
                if (!string.IsNullOrWhiteSpace(schemaName))
                {
                    try
                    {
                        //create the table to enitty mapping for the sheet
                        var mapping = new RecordMetadata
                        {
                            SchemaName = schemaName,
                            DisplayName = row.GetFieldAsString(Headings.RecordTypes.DisplayName),
                            DisplayCollectionName = row.GetFieldAsString(Headings.RecordTypes.DisplayCollectionName),
                            Description = row.GetFieldAsString(Headings.RecordTypes.Description),
                            Audit = row.GetFieldAsBoolean(Headings.RecordTypes.Audit),
                            IsActivityType = row.GetFieldAsBoolean(Headings.RecordTypes.IsActivityType),
                            Notes = row.GetFieldAsBoolean(Headings.RecordTypes.Notes),
                            Activities = row.GetFieldAsBoolean(Headings.RecordTypes.Activities),
                            Connections = row.GetFieldAsBoolean(Headings.RecordTypes.Connections),
                            MailMerge = row.GetFieldAsBoolean(Headings.RecordTypes.MailMerge),
                            Queues = row.GetFieldAsBoolean(Headings.RecordTypes.Queues)
                        };
                        mapping.Fields = fieldMetadata.Where(f => f.RecordType == mapping.SchemaName).ToArray();
                        mapping.Views = new[] {GetView(mapping.SchemaName, excelFile, controller)};
                        result.Add(mapping);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error reading entity from excel: " + schemaName, ex);
                    }
                }
            }

            return result;
        }

        public static IEnumerable<PicklistOptionSet> ExtractOptionSetsFromExcel(string excelFile,
            LogController controller)
        {
            var result = new List<PicklistOptionSet>();
            try
            {
                var rows = ExcelUtility.SelectPropertyBagsFromExcelTabName(excelFile,
                    OptionSetsTabName);

                var optionSetNames = rows
                    .Where(r => !string.IsNullOrWhiteSpace(r.GetFieldAsString("Option Set Name")))
                    .Select(r => r.GetFieldAsString("Option Set Name"))
                    .Distinct()
                    .ToArray();

                foreach (var optionSetName in optionSetNames)
                {
                    var thisOptionSetName = optionSetName;
                    try
                    {
                        var setDetailRow = rows
                            .First(r => r.GetFieldAsString(Headings.OptionSets.OptionSetName) == thisOptionSetName);
                        var schemaName = setDetailRow.GetFieldAsString(Headings.OptionSets.SchemaName);
                        var isShared = setDetailRow.GetFieldAsBoolean(Headings.OptionSets.IsSharedOptionSet);

                        var options = rows
                            .Where(r => r.GetFieldAsString(Headings.OptionSets.OptionSetName) == thisOptionSetName)
                            .Select(r => new PicklistOption(r.GetFieldAsString(Headings.OptionSets.Index), r.GetFieldAsString(Headings.OptionSets.Label)))
                            .ToArray();

                        foreach (var option in options)
                        {
                            if (option.Value.StartsWith("TXT_"))
                                option.Value = option.Value.Substring(4);
                        }

                        var picklistOptionSet = new PicklistOptionSet(options, isShared, schemaName, thisOptionSetName);
                        result.Add(picklistOptionSet);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error retrieving option set " + thisOptionSetName, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading option sets from excel", ex);
            }
            return result;
        }

        private static ViewMetadata GetView(string recordType, string excelFile, LogController controller)
        {
            var fieldRows = ExcelUtility.SelectPropertyBagsFromExcelTabName(excelFile,
                FieldsTabName);

            var viewFields = new List<ViewField>();
            foreach (
                var row in
                    fieldRows.Where(r => r.GetFieldAsString("Record Type Schema Name") == recordType))
            {
                var viewOrderString = row.GetFieldAsString("View Order");
                if (!string.IsNullOrWhiteSpace(viewOrderString))
                {
                    var viewWidth = string.IsNullOrWhiteSpace(row.GetFieldAsString("View Width"))
                        ? 100
                        : row.GetFieldAsInteger("View Width");
                    var viewField = new ViewField(row.GetFieldAsString("Schema Name"),
                        row.GetFieldAsInteger("View Order"), viewWidth);
                    viewFields.Add(viewField);
                }
            }

            return new ViewMetadata(viewFields);
        }
    }
}