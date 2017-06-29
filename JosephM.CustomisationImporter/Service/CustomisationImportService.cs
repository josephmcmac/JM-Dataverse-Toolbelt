#region

using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.CustomisationImporter.ImportMetadata;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Spreadsheet;
using JosephM.Xrm.MetadataImportExport;
using JosephM.Xrm.Schema;
using Microsoft.Crm.Sdk.Messages;
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

        public IRecordService RecordService { get; set; }

        public override void ExecuteExtention(CustomisationImportRequest request, CustomisationImportResponse response,
            LogController controller)
        {
            controller.LogLiteral("Reading excel spreadsheet");

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

            if (request.Entities)
            {
                var importRecordsResponse = ImportRecordTypes(recordMetadataToImport, controller, response);
                createdRecordTypes.AddRange(importRecordsResponse.CreatedRecordTypes);
            }
            if (request.SharedOptionSets)
                ImportSharedOptionSets(optionSets, controller, response);
            if (request.Fields)
            {
                var importFieldsResponse = ImportFieldTypes(fieldMetadataToImport, controller, response, createdRecordTypes);
                createdFields.AddRange(importFieldsResponse.CreatedFields);
            }
            if (request.FieldOptionSets)
                ImportFieldOptionSets(fieldMetadataToImport.Values, controller, response, createdFields);
            if (request.Views)
                ImportViews(recordMetadataToImport.Values, controller, response, createdRecordTypes);
            if (request.Relationships)
                ImportRelationships(relationshipMetadataToImport, controller, response);
            controller.LogLiteral("Publishing Changes");
            RecordService.Publish();

            if (request.AddToSolution)
            {
                var solutionId = request.Solution.Id;
                var solution = RecordService.Get(Entities.solution, solutionId);
                var allImportedResponseItems = response.ResponseItems
                    .Where(ri => !ri.HasError)
                    .Select(ri => ri.Metadata)
                    .ToArray();
                //option sets
                Func<object, IMetadata> getMetadata = (object item) => RecordService.GetSharedPicklist(((PicklistOptionSet)item).SchemaName);
                var theseObjectsToAdd = allImportedResponseItems
                    .Where(m => m is PicklistOptionSet)
                    .Cast<PicklistOptionSet>()
                    .Where(p => p.IsSharedOptionSet)
                    .Cast<IMetadata>()
                    .Distinct()
                    .ToArray();
                AddComponentsToSolution(OptionSets.SolutionComponent.ObjectTypeCode.OptionSet, theseObjectsToAdd, getMetadata, response, solution, controller);
                //record types
                getMetadata = (object item) => RecordService.GetRecordTypeMetadata(((RecordMetadata)item).SchemaName);
                theseObjectsToAdd = allImportedResponseItems
                    .Where(m => m is RecordMetadata)
                    .Cast<IMetadata>()
                    .Distinct()
                    .ToArray();
                AddComponentsToSolution(OptionSets.SolutionComponent.ObjectTypeCode.Entity, theseObjectsToAdd, getMetadata, response, solution, controller);
                //fields
                getMetadata = (object item) => RecordService.GetRecordTypeMetadata(((FieldMetadata)item).RecordType);
                theseObjectsToAdd = allImportedResponseItems
                    .Where(m => m is FieldMetadata)
                    .Cast<IMetadata>()
                    .Distinct()
                    .ToArray();
                AddComponentsToSolution(OptionSets.SolutionComponent.ObjectTypeCode.Entity, theseObjectsToAdd, getMetadata, response, solution, controller);
                //relationships side 1
                getMetadata = (object item) => RecordService.GetRecordTypeMetadata(((Many2ManyRelationshipMetadata)item).RecordType1);
                theseObjectsToAdd = allImportedResponseItems
                    .Where(m => m is Many2ManyRelationshipMetadata)
                    .Cast<IMetadata>()
                    .Distinct()
                    .ToArray();
                AddComponentsToSolution(OptionSets.SolutionComponent.ObjectTypeCode.Entity, theseObjectsToAdd, getMetadata, response, solution, controller);
                //relationships side 2
                getMetadata = (object item) => RecordService.GetRecordTypeMetadata(((Many2ManyRelationshipMetadata)item).RecordType2);
                theseObjectsToAdd = allImportedResponseItems
                    .Where(m => m is Many2ManyRelationshipMetadata)
                    .Cast<IMetadata>()
                    .Distinct()
                    .ToArray();
                AddComponentsToSolution(OptionSets.SolutionComponent.ObjectTypeCode.Entity, theseObjectsToAdd, getMetadata, response, solution, controller);
                //field options
                getMetadata = (object item) => RecordService.GetRecordTypeMetadata(((ImportPicklistFieldOptions)item).FieldMetadata.RecordType);
                theseObjectsToAdd = allImportedResponseItems
                    .Where(m => m is ImportPicklistFieldOptions)
                    .Cast<IMetadata>()
                    .Distinct()
                    .ToArray();
                AddComponentsToSolution(OptionSets.SolutionComponent.ObjectTypeCode.Entity, theseObjectsToAdd, getMetadata, response, solution, controller);
                //views
                getMetadata = (object item) => RecordService.GetRecordTypeMetadata(((ImportViews)item).SchemaName);
                theseObjectsToAdd = allImportedResponseItems
                    .Where(m => m is ImportViews)
                    .Cast<IMetadata>()
                    .Distinct()
                    .ToArray();
                AddComponentsToSolution(OptionSets.SolutionComponent.ObjectTypeCode.Entity, theseObjectsToAdd, getMetadata, response, solution, controller);

            }

            RecordService.ClearCache();
        }

        private void AddComponentsToSolution(int componentType, IEnumerable<IMetadata> objectsToAdd, Func<object, IMetadata> getMetadata, CustomisationImportResponse response, IRecord solution, LogController controller)
        {
            var xrmService = ((XrmRecordService)RecordService).XrmService;
            var currentComponentIds = RecordService.RetrieveAllAndClauses(Entities.solutioncomponent, new[]
                {
                            new Condition(Fields.solutioncomponent_.componenttype, ConditionType.Equal, componentType),
                            new Condition(Fields.solutioncomponent_.solutionid, ConditionType.Equal, solution.Id)
                        }, null)
                        .Select(r => r.GetIdField(Fields.solutioncomponent_.objectid))
                        .ToList();
            var count = objectsToAdd.Count();
            var done = 0;
            foreach (var item in objectsToAdd)
            {
                controller.UpdateProgress(done++, count, string.Format("Adding items to solution for imported {0}", item.GetType().GetDisplayName()));
                var metadata = getMetadata(item);
                try
                {
                    if (!currentComponentIds.Contains(metadata.MetadataId))
                    {
                        var addRequest = new AddSolutionComponentRequest()
                        {
                            AddRequiredComponents = false,
                            ComponentType = componentType,
                            ComponentId = new Guid(metadata.MetadataId),
                            SolutionUniqueName = solution.GetStringField(Fields.solution_.uniquename)
                        };
                       xrmService.Execute(addRequest);
                       currentComponentIds.Add(metadata.MetadataId);
                    }
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(item, ex);
                }
            }
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

                FieldMetadata fieldMetadata = null;
                try
                {

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
                                        displayName);
                                    fieldMetadata.IncludeTime = row.GetFieldAsBoolean(Headings.Fields.IncludeTime);
                                    var dateBehaviour = !row.GetColumnNames().Contains(Headings.Fields.DateBehaviour) || string.IsNullOrWhiteSpace(row.GetFieldAsString(Headings.Fields.DateBehaviour))
                                            ? "UserLocal"
                                            : row.GetFieldAsString(Headings.Fields.DateBehaviour);
                                    fieldMetadata.DateBehaviour = dateBehaviour;
                                    break;
                                }
                            case (RecordFieldType.Decimal):
                                {
                                    fieldMetadata = new DecimalFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                        displayName);
                                    fieldMetadata.MinValue = row.GetFieldAsDecimal(Headings.Fields.Minimum);
                                        fieldMetadata.MaxValue = row.GetFieldAsDecimal(Headings.Fields.Maximum);
                                        fieldMetadata.DecimalPrecision = row.GetFieldAsInteger(Headings.Fields.DecimalPrecision);
                                    break;
                                }
                            case (RecordFieldType.Integer):
                                {
                                    fieldMetadata = new IntegerFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                        displayName);
                            fieldMetadata.MinValue = row.GetFieldAsInteger(Headings.Fields.Minimum);
                                        fieldMetadata.MaxValue = row.GetFieldAsInteger(Headings.Fields.Maximum);
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
                                        displayName);
                                    fieldMetadata.MinValue = row.GetFieldAsDecimal(Headings.Fields.Minimum);
                                    fieldMetadata.MaxValue = row.GetFieldAsDecimal(Headings.Fields.Maximum);
                                    break;
                                }
                            case (RecordFieldType.Picklist):
                                {
                                    fieldMetadata = new PicklistFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                        displayName);
                                    var pFieldMetadata = (PicklistFieldMetadata)fieldMetadata;
                                    var optionSetName = row.GetFieldAsString(Headings.Fields.PicklistOptions);
                                    var optionSet = new PicklistOptionSet();
                                    if (!string.IsNullOrWhiteSpace(optionSetName))
                                    {
                                        if (picklistOptionSets.Any(p => p.DisplayName == optionSetName))
                                            optionSet = picklistOptionSets.First(p => p.DisplayName == optionSetName);
                                    }
                                    pFieldMetadata.PicklistOptionSet = optionSet;
                                    break;
                                }
                            case (RecordFieldType.String):
                                {
                                    fieldMetadata = new StringFieldMetadata(recordTypeSchemaName, fieldSchemaName,
                                        displayName)
                                    {
                                        IsPrimaryField = row.GetFieldAsBoolean(Headings.Fields.IsPrimaryField)
                                    };
                                    fieldMetadata.MaxLength = row.GetFieldAsInteger(Headings.Fields.MaxLength);
                                    fieldMetadata.TextFormat =
                                        string.IsNullOrWhiteSpace(row.GetFieldAsString(Headings.Fields.TextFormat))
                                            ? TextFormat.Text
                                            : row.GetFieldAsEnum<TextFormat>(Headings.Fields.TextFormat);
                                    break;
                                }
                            case RecordFieldType.Memo:
                                {
                                    fieldMetadata = new MemoFieldMetadata(recordTypeSchemaName, fieldSchemaName, displayName);
                                    fieldMetadata.MaxLength = row.GetFieldAsInteger(Headings.Fields.MaxLength);
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
                                        displayName);
                                    fieldMetadata.MinValue = row.GetFieldAsDecimal(Headings.Fields.Minimum);
                                    fieldMetadata.MaxValue = row.GetFieldAsDecimal(Headings.Fields.Maximum);
                                    fieldMetadata.DecimalPrecision = row.GetFieldAsInteger(Headings.Fields.DecimalPrecision);
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
                    response.AddResponseItem(row.Index + 1, fieldMetadata, ex);
                }
            }
            return fields;
        }

        private void ImportFieldOptionSets(IEnumerable<FieldMetadata> fieldMetadata, LogController controller,
            CustomisationImportResponse response, IEnumerable<FieldMetadata> createdFields)
        {
            var picklistFields = fieldMetadata
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
                    var importFieldOptions = new ImportPicklistFieldOptions(field);
                    try
                    {
                        controller.UpdateProgress(numberCompleted++, numberToDo, "Importing Field Option Sets");
                        var entityType = field.RecordType;
                        RecordService.UpdateFieldOptionSet(entityType, field.SchemaName,
                            field.PicklistOptionSet);
                        var updated = !createdFields.Contains(field);
                        response.AddResponseItem(importFieldOptions, updated);
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(importFieldOptions, ex);
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

                    importResponse.AddResponseItem(sharedOptionSet, isUpdate);
                }
                catch (Exception ex)
                {
                    importResponse.AddResponseItem(sharedOptionSet, ex);
                }
            }
        }

        private void ImportViews(IEnumerable<ImportRecordMetadata> metadata, LogController controller,
            CustomisationImportResponse response, IEnumerable<string> createdRecordTypes)
        {
            var numberToDo = metadata.Count();
            var numberCompleted = 0;
            for (var i = 0; i < metadata.Count(); i++)
            {
                var recordMetadata = metadata.ElementAt(i);
                var viewImport = new ImportViews(recordMetadata);
                try
                {
                    controller.UpdateProgress(numberCompleted++, numberToDo, "Importing Views");
                    var isUpdate = !createdRecordTypes.Contains(recordMetadata.SchemaName);
                    if (recordMetadata.Views.Any(f => f.Fields.Any(g => g.Order >= 0 && g.Width > 0)))
                    {
                        RecordService.UpdateViews(recordMetadata);
                        recordMetadata.ViewUpdated = true;
                    }
                    else
                        continue;
                    response.AddResponseItem(viewImport, isUpdate);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(viewImport, ex);
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
                    response.AddResponseItem(excelRow, recordMetadata, isUpdate);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(excelRow, recordMetadata, ex);
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
                    response.AddResponseItem(excelRow, field, isUpdate);
                    if (!isUpdate)
                        importFieldsResponse.AddCreatedField(field);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(excelRow, field, ex);
                }
            }
            return importFieldsResponse;
        }

        private ImportRecordTypesResponse ImportRecordTypes(IDictionary<int, ImportRecordMetadata> metadata, LogController controller,
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
                        response.AddResponseItem(recordMetadata.GetPrimaryFieldMetadata(), false);
                    }
                    response.AddResponseItem(excelRow, recordMetadata, isUpdate);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(excelRow, recordMetadata, ex);
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
                    var mapping = new Many2ManyRelationshipMetadata();
                    try
                    {
                        mapping.SchemaName = relationshipName;
                        mapping.RecordType1 = row.GetFieldAsString(Headings.Relationships.RecordType1);
                        mapping.RecordType2 = row.GetFieldAsString(Headings.Relationships.RecordType2);
                        mapping.RecordType1DisplayRelated = row.GetFieldAsBoolean(Headings.Relationships.RecordType1DisplayRelated);
                        mapping.RecordType2DisplayRelated = row.GetFieldAsBoolean(Headings.Relationships.RecordType2DisplayRelated);
                        mapping.RecordType1UseCustomLabel = row.GetFieldAsBoolean(Headings.Relationships.RecordType1UseCustomLabel);
                        mapping.RecordType2UseCustomLabel = row.GetFieldAsBoolean(Headings.Relationships.RecordType2UseCustomLabel);
                        mapping.RecordType1CustomLabel = row.GetFieldAsString(Headings.Relationships.RecordType1CustomLabel);
                        mapping.RecordType2CustomLabel = row.GetFieldAsString(Headings.Relationships.RecordType2CustomLabel);
                        mapping.RecordType1DisplayOrder = row.GetFieldAsBoolean(Headings.Relationships.RecordType1DisplayRelated) ? row.GetFieldAsInteger(Headings.Relationships.RecordType1DisplayOrder) : -1;
                        mapping.RecordType2DisplayOrder = row.GetFieldAsBoolean(Headings.Relationships.RecordType2DisplayRelated) ? row.GetFieldAsInteger(Headings.Relationships.RecordType2DisplayOrder) : -1;
                        if (string.IsNullOrWhiteSpace(mapping.RecordType1))
                            throw new NullReferenceException(string.Format("{0} Is Required", Headings.Relationships.RecordType1));
                        if (string.IsNullOrWhiteSpace(mapping.RecordType2))
                            throw new NullReferenceException(string.Format("{0} Is Required", Headings.Relationships.RecordType2));
                        result.Add(row.Index + 1, mapping);
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(row.Index +1, mapping, ex);
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///     Reads a set of mappings from the Excel file which contains the entity and field mapping metadata
        /// </summary>
        internal static IDictionary<int, ImportRecordMetadata> ExtractRecordMetadataFromExcel(string excelFile,
            LogController controller,
            IEnumerable<FieldMetadata>
                fieldMetadata, CustomisationImportResponse response)
        {
            var result = new Dictionary<int, ImportRecordMetadata>();

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
                    var mapping = new ImportRecordMetadata();
                    try
                    {
                        mapping.SchemaName = schemaName;
                        mapping.DisplayName = row.GetFieldAsString(Headings.RecordTypes.DisplayName);
                        mapping.CollectionName = row.GetFieldAsString(Headings.RecordTypes.DisplayCollectionName);
                        mapping.Description = row.GetFieldAsString(Headings.RecordTypes.Description);
                        mapping.Audit = row.GetFieldAsBoolean(Headings.RecordTypes.Audit);
                        mapping.IsActivityType = row.GetFieldAsBoolean(Headings.RecordTypes.IsActivityType);
                        mapping.Notes = row.GetFieldAsBoolean(Headings.RecordTypes.Notes);
                        mapping.Activities = row.GetFieldAsBoolean(Headings.RecordTypes.Activities);
                        mapping.Connections = row.GetFieldAsBoolean(Headings.RecordTypes.Connections);
                        mapping.MailMerge = row.GetFieldAsBoolean(Headings.RecordTypes.MailMerge);
                        mapping.Queues = row.GetFieldAsBoolean(Headings.RecordTypes.Queues);

                        mapping.Fields = fieldMetadata.Where(f => f.RecordType == mapping.SchemaName).ToArray();
                        mapping.Views = new[] { GetView(mapping, excelFile, controller, response) };

                        if (string.IsNullOrWhiteSpace(mapping.DisplayName))
                            throw new NullReferenceException(string.Format("{0} Is Required", Headings.RecordTypes.DisplayName));

                        result.Add(row.Index + 1, mapping);
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(row.Index + 1, mapping, ex);
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
                    var picklistOptionSet = new PicklistOptionSet();
                    try
                    {
                        var setDetailRow = rows
                            .First(r => r.GetFieldAsString(Headings.OptionSets.OptionSetName) == thisOptionSetName);
                        var schemaName = setDetailRow.GetFieldAsString(Headings.OptionSets.SchemaName);
                        picklistOptionSet.SchemaName = schemaName;
                        var isShared = setDetailRow.GetFieldAsBoolean(Headings.OptionSets.IsSharedOptionSet);
                        picklistOptionSet.IsSharedOptionSet = isShared;
                        picklistOptionSet.DisplayName = thisOptionSetName;
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
                                response.AddResponseItem(option.ExcelRow, picklistOptionSet, ex);
                            }
                        }
                        picklistOptionSet.PicklistOptions = options;
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

        private static ViewMetadata GetView(RecordMetadata recordMetadata, string excelFile, LogController controller, CustomisationImportResponse response)
        {
            var recordType = recordMetadata.SchemaName;
            var fieldRows = ExcelUtility.SelectPropertyBagsFromExcelTabName(excelFile,
                FieldsTabName);

            var viewFields = new List<ViewField>();
            try
            {
                foreach (
                var row in
                    fieldRows.Where(r => r.GetFieldAsString(Headings.Fields.RecordTypeSchemaName) == recordType))
                {
                    var fieldName = row.GetFieldAsString(Headings.Fields.SchemaName);

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
            }
            catch (Exception ex)
            {
                //todo fix this maybe should be a view import error
                response.AddResponseItem(recordMetadata, ex);
            }
            return new ViewMetadata(viewFields);
        }
    }
}