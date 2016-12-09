#region

using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.CustomisationImporter.ImportMetadata;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Spreadsheet;
using JosephM.Xrm.MetadataImportExport;
using System;
using System.Collections.Generic;
using System.Linq;

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
            controller.LogLiteral("Reading excel spreadsheet");

            //todo log read errors for all
            var optionSets = ExtractOptionSetsFromExcel(request.ExcelFile.FileName,
                controller, response);
            var fieldMetadataToImport = ExtractFieldMetadataFromExcel(
                request.ExcelFile.FileName, controller, optionSets, response);
            var recordMetadataToImport =
                ExtractRecordMetadataFromExcel(request.ExcelFile.FileName, controller, fieldMetadataToImport.Values, response);
            var relationshipMetadataToImport =
                ExtractRelationshipMetadataFromExcel(request.ExcelFile.FileName, controller, response);

            if(response.ResponseItemsWithError.Any())
            {
                response.ExcelReadErrors = true;
                return;
            }

            CheckCrmConnection(controller);

            var createdRecordTypes = new List<string>();
            var createdFields = new List<FieldMetadata>();

            if (request.IncludeEntities)
            {
                var importRecordsResponse = ImportRecordTypes(recordMetadataToImport, controller, response);
                createdRecordTypes.AddRange(importRecordsResponse.CreatedRecordTypes);
            }
            if (request.UpdateOptionSets)
                ImportSharedOptionSets(optionSets, controller, response);
            if (request.IncludeFields)
            {
                var importFieldsResponse = ImportFieldTypes(fieldMetadataToImport, controller, response, createdRecordTypes);
                createdFields.AddRange(importFieldsResponse.CreatedFields);
            }
            if (request.UpdateOptionSets)
                ImportFieldOptionSets(recordMetadataToImport.Values, controller, response, createdFields);
            if (request.UpdateViews)
                ImportViews(recordMetadataToImport.Values, controller, response, createdRecordTypes);
            if (request.IncludeRelationships)
                ImportRelationships(relationshipMetadataToImport, controller, response);
            controller.LogLiteral("Publishing Changes");
            RecordService.Publish();
            RecordService.ClearCache();
        }

        public static IDictionary<int,FieldMetadata> ExtractFieldMetadataFromExcel(string excelFile,
            LogController controller,
            IEnumerable<PicklistOptionSet>
                picklistOptionSets, CustomisationImportResponse response)
        {
            var rows = ExcelUtility.SelectPropertyBagsFromExcelTabName(excelFile,
                FieldsTabName);
            var fields = new Dictionary<int,FieldMetadata>();
            foreach (var row in rows)
            {
                if (row.GetColumnNames().Contains(Headings.Fields.Ignore)
                    && row.GetFieldAsBoolean(Headings.Fields.Ignore))
                    continue;

                var fieldSchemaName = row.GetFieldAsString(Headings.Fields.SchemaName);

                try
                {
                    FieldMetadata fieldMetadata = null;
                    var type = row.GetFieldAsEnum<RecordFieldType>(Headings.Fields.FieldType);

                    if (!String.IsNullOrWhiteSpace(fieldSchemaName))
                    {
                        var recordTypeSchemaName = row.GetFieldAsString(Headings.Fields.RecordTypeSchemaName);

                        var displayName = row.GetFieldAsString(Headings.Fields.DisplayName);
                        if (string.IsNullOrWhiteSpace(displayName))
                            throw new NullReferenceException(string.Format("{0} Is Required", Headings.Fields.DisplayName));


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
                                        MinValue = row.GetFieldAsDecimal(Headings.Fields.Minimum),
                                        MaxValue = row.GetFieldAsDecimal(Headings.Fields.Maximum),
                                        DecimalPrecision = row.GetFieldAsInteger(Headings.Fields.DecimalPrecision)
                                    };
                                    break;
                                }
                            case (RecordFieldType.Integer):
                                {
                                    fieldMetadata = new IntegerFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                        displayName)
                                    {
                                        MinValue = row.GetFieldAsInteger(Headings.Fields.Minimum),
                                        MaxValue = row.GetFieldAsInteger(Headings.Fields.Maximum)
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
                                        MinValue = row.GetFieldAsDecimal(Headings.Fields.Minimum),
                                        MaxValue = row.GetFieldAsDecimal(Headings.Fields.Maximum)
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
                                        MinValue = row.GetFieldAsDecimal(Headings.Fields.Minimum),
                                        MaxValue = row.GetFieldAsDecimal(Headings.Fields.Maximum),
                                        DecimalPrecision = row.GetFieldAsInteger(Headings.Fields.DecimalPrecision)
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
                            case (RecordFieldType.Customer):
                                {
                                    fieldMetadata = new CustomerFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                        displayName)
                                    {
                                        DisplayInRelated = row.GetFieldAsBoolean(Headings.Fields.DisplayInRelated)
                                    };
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
                        fields.Add(row.Index + 1, fieldMetadata);
                    }
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(row.Index + 1, FieldsTabName, fieldSchemaName, ex);
                }
            }
            return fields;
        }

        private void ImportFieldOptionSets(IEnumerable<RecordMetadata> recordMetadata, LogController controller,
            CustomisationImportResponse response, IEnumerable<FieldMetadata> createdFields)
        {
            var picklistFields = recordMetadata
                .SelectMany(m => m.Fields)
                .Where(
                    f => f is PicklistFieldMetadata && !((PicklistFieldMetadata)f).PicklistOptionSet.IsSharedOptionSet)
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
                        var updated = !createdFields.Contains(field);
                        response.AddResponseItem("Field Option Set", field.SchemaName, updated);
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
            var sharedOptionSets = optionSets
                .Where(o => o.IsSharedOptionSet)
                .Where(o => o.PicklistOptions.Any()
                && (o.PicklistOptions.Count() > 1 || o.PicklistOptions.First().Key != "-1"))
                .ToArray();
            var numberToDo = sharedOptionSets.Count();
            var numberCompleted = 0;
            for (var i = 0; i < sharedOptionSets.Count(); i++)
            {
                var sharedOptionSet = sharedOptionSets[i];
                try
                {
                    controller.UpdateProgress(numberCompleted++, numberToDo, "Importing Shared Option Sets");
                    var isUpdate = RecordService.GetSharedPicklists().Any(p => p.SchemaName == sharedOptionSet.SchemaName);
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
            CustomisationImportResponse response, IEnumerable<string> createdRecordTypes)
        {
            var numberToDo = metadata.Count();
            var numberCompleted = 0;
            for (var i = 0; i < metadata.Count(); i++)
            {
                var recordMetadata = metadata.ElementAt(i);
                try
                {
                    controller.UpdateProgress(numberCompleted++, numberToDo, "Importing Views");
                    var isUpdate = !createdRecordTypes.Contains(recordMetadata.SchemaName);
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

        private void ImportRelationships(IDictionary<int, Many2ManyRelationshipMetadata> metadata, LogController controller,
            CustomisationImportResponse response)
        {
            var numberToDo = metadata.Count();
            var numberCompleted = 0;
            for (var i = 0; i < metadata.Count(); i++)
            {
                var keyValue = metadata.ElementAt(i);
                var excelRow = keyValue.Key;
                var recordMetadata = keyValue.Value;
                try
                {
                    controller.UpdateProgress(numberCompleted++, numberToDo, string.Format("Importing {0}", RelationshipTabName));
                    var isUpdate = RecordService.GetManyToManyRelationships(recordMetadata.RecordType1).Any(r => r.SchemaName == recordMetadata.SchemaName);
                    RecordService.CreateOrUpdate(recordMetadata);
                    response.AddResponseItem(excelRow, RelationshipTabName, recordMetadata.SchemaName, isUpdate);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(excelRow, RelationshipTabName, recordMetadata.SchemaName, ex);
                }
            }
        }

        private ImportFieldsResponse ImportFieldTypes(IDictionary<int, FieldMetadata> metadata, LogController controller,
            CustomisationImportResponse response, IEnumerable<string> createdRecordTypes)
        {
            var importFieldsResponse = new ImportFieldsResponse();

            var numberToDo = metadata.Count();
            var numberCompleted = 0;
            for (var i = 0; i < metadata.Count(); i++)
            {
                var keyValue = metadata.ElementAt(i);
                var excelRow = keyValue.Key;
                var field = keyValue.Value;
                try
                {
                    controller.UpdateProgress(numberCompleted++, numberToDo, string.Format("Importing {0}", FieldsTabName));

                    //If this is a primary field and the record type was created this import
                    //then the field has already been created when the record type was created
                    if (field is StringFieldMetadata
                        && ((StringFieldMetadata)field).IsPrimaryField
                        && createdRecordTypes.Contains(field.RecordType))
                    {
                        importFieldsResponse.AddCreatedField(field);
                        continue;
                    }

                    var isUpdate = !createdRecordTypes.Contains(field.RecordType)
                        && RecordService.FieldExists(field.SchemaName, field.RecordType);

                    RecordService.CreateOrUpdate(field, field.RecordType);
                    response.AddResponseItem(excelRow, FieldsTabName, field.SchemaName, isUpdate);
                    if (!isUpdate)
                        importFieldsResponse.AddCreatedField(field);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(excelRow, FieldsTabName, field.SchemaName, ex);
                }
            }
            return importFieldsResponse;
        }

        private ImportRecordTypesResponse ImportRecordTypes(IDictionary<int, RecordMetadata> metadata, LogController controller,
            CustomisationImportResponse response)
        {
            var thisResponse = new ImportRecordTypesResponse();

            var numberToDo = metadata.Count();
            var numberCompleted = 0;
            for (var i = 0; i < metadata.Count(); i++)
            {
                var keyValue = metadata.ElementAt(i);
                var excelRow = keyValue.Key;
                var recordMetadata = keyValue.Value;
                try
                {
                    controller.UpdateProgress(numberCompleted++, numberToDo, string.Format("Importing {0}", EntityTabName));
                    var isUpdate = RecordService.RecordTypeExists(recordMetadata.SchemaName);
                    RecordService.CreateOrUpdate(recordMetadata);
                    if (!isUpdate)
                    {
                        thisResponse.AddCreatedRecordType(recordMetadata.SchemaName);
                        response.AddResponseItem(FieldsTabName, recordMetadata.GetPrimaryFieldMetadata().SchemaName, false);
                    }
                    response.AddResponseItem(excelRow, EntityTabName, recordMetadata.SchemaName, isUpdate);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(excelRow, EntityTabName, recordMetadata.SchemaName, ex);
                }
            }
            return thisResponse;
        }

        /// <summary>
        ///     Reads a set of mappings from the Excel file which contains the entity and field mapping metadata
        /// </summary>
        internal static IDictionary<int, Many2ManyRelationshipMetadata> ExtractRelationshipMetadataFromExcel(string excelFile,
            LogController controller, CustomisationImportResponse response)
        {
            var result = new Dictionary<int, Many2ManyRelationshipMetadata>();

            var rows = ExcelUtility.SelectPropertyBagsFromExcelTabName(excelFile,
                RelationshipTabName);
            //For each row log errors
            foreach (var row in rows)
            {
                if (row.GetColumnNames().Contains(Headings.Relationships.Ignore)
                    && row.GetFieldAsBoolean(Headings.Relationships.Ignore))
                    continue;

                var relationshipName = row.GetFieldAsString(Headings.Relationships.RelationshipName);
                if (!string.IsNullOrWhiteSpace(relationshipName))
                {
                    try
                    {
                        //create the table to enitty mapping for the sheet
                        var mapping = new Many2ManyRelationshipMetadata
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
                        if (string.IsNullOrWhiteSpace(mapping.RecordType1))
                            throw new NullReferenceException(string.Format("{0} Is Required", Headings.Relationships.RecordType1));
                        if (string.IsNullOrWhiteSpace(mapping.RecordType2))
                            throw new NullReferenceException(string.Format("{0} Is Required", Headings.Relationships.RecordType2));
                        result.Add(row.Index + 1, mapping);
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(row.Index +1, RelationshipTabName, relationshipName, ex);
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///     Reads a set of mappings from the Excel file which contains the entity and field mapping metadata
        /// </summary>
        internal static IDictionary<int, RecordMetadata> ExtractRecordMetadataFromExcel(string excelFile,
            LogController controller,
            IEnumerable<FieldMetadata>
                fieldMetadata, CustomisationImportResponse response)
        {
            var result = new Dictionary<int, RecordMetadata>();

            var rows = ExcelUtility.SelectPropertyBagsFromExcelTabName(excelFile,
                EntityTabName);
            //For each row
            foreach (var row in rows)
            {
                if (row.GetColumnNames().Contains(Headings.RecordTypes.Ignore)
                    && row.GetFieldAsBoolean(Headings.RecordTypes.Ignore))
                    continue;

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
                            CollectionName = row.GetFieldAsString(Headings.RecordTypes.DisplayCollectionName),
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
                        mapping.Views = new[] { GetView(mapping.SchemaName, excelFile, controller, response) };

                        if (string.IsNullOrWhiteSpace(mapping.DisplayName))
                            throw new NullReferenceException(string.Format("{0} Is Required", Headings.RecordTypes.DisplayName));

                        result.Add(row.Index + 1, mapping);
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(row.Index + 1, EntityTabName, schemaName, ex);
                    }
                }
            }

            return result;
        }

        public static IEnumerable<PicklistOptionSet> ExtractOptionSetsFromExcel(string excelFile,
            LogController controller, CustomisationImportResponse response)
        {
            var result = new List<PicklistOptionSet>();
            try
            {
                var rows = ExcelUtility.SelectPropertyBagsFromExcelTabName(excelFile,
                    OptionSetsTabName);

                rows = rows.Where(row => !row.GetColumnNames().Contains(Headings.OptionSets.Ignore)
                                         || !row.GetFieldAsBoolean(Headings.OptionSets.Ignore))
                                         .ToArray();

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
                            .Select(r => new ImportPicklistOption(r.GetFieldAsString(Headings.OptionSets.Index), r.GetFieldAsString(Headings.OptionSets.Label), r.Index))
                            .ToArray();

                        foreach (var option in options)
                        {
                            try
                            {
                                if (isShared && string.IsNullOrWhiteSpace(schemaName))
                                    throw new NullReferenceException(string.Format("{0} Is Required", Headings.OptionSets.SchemaName));
                                if (string.IsNullOrWhiteSpace(option.Key))
                                    throw new NullReferenceException(string.Format("{0} Is Required", Headings.OptionSets.Index));
                                if (string.IsNullOrWhiteSpace(option.Value))
                                    throw new NullReferenceException(string.Format("{0} Is Required", Headings.OptionSets.Label));
                                if (option.Value.StartsWith("TXT_"))
                                        option.Value = option.Value.Substring(4);
                            }
                            catch(Exception ex)
                            {
                                response.AddResponseItem(option.ExcelRow, OptionSetsTabName, option.Value, ex);
                            }
                        }

                        var picklistOptionSet = new PicklistOptionSet(options, isShared, schemaName, thisOptionSetName);
                        result.Add(picklistOptionSet);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error loading option set " + thisOptionSetName, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading option sets from excel", ex);
            }
            return result;
        }

        private static ViewMetadata GetView(string recordType, string excelFile, LogController controller, CustomisationImportResponse response)
        {
            var fieldRows = ExcelUtility.SelectPropertyBagsFromExcelTabName(excelFile,
                FieldsTabName);

            var viewFields = new List<ViewField>();
            foreach (
                var row in
                    fieldRows.Where(r => r.GetFieldAsString(Headings.Fields.RecordTypeSchemaName) == recordType))
            {
                var fieldName = row.GetFieldAsString(Headings.Fields.SchemaName);
                try
                {
                    var viewOrderString = row.GetFieldAsString(Headings.Fields.ViewOrder);
                    if (!string.IsNullOrWhiteSpace(viewOrderString))
                    {
                        var order = row.GetFieldAsInteger(Headings.Fields.ViewOrder);
                        if (order >= 0)
                        {
                            var viewWidth = string.IsNullOrWhiteSpace(row.GetFieldAsString("View Width"))
                                ? 100
                                : row.GetFieldAsInteger("View Width");
                            var viewField = new ViewField(fieldName, order
                                , viewWidth);
                            viewFields.Add(viewField);
                        }
                    }
                }
                catch(Exception ex)
                {
                    response.AddResponseItem(row.Index + 1, "View Field", string.Format("{0}.{1}", recordType, fieldName), ex);
                }
            }

            return new ViewMetadata(viewFields);
        }
    }
}