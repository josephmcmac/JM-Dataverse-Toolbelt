using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Xrm.CustomisationExporter
{
    public class CustomisationExporterService :
        ServiceBase<CustomisationExporterRequest, CustomisationExporterResponse, CustomisationExporterResponseItem>
    {
        public CustomisationExporterService(XrmRecordService service)
        {
            Service = service;
        }

        private IRecordService Service { get; set; }

        public override void ExecuteExtention(CustomisationExporterRequest request,
            CustomisationExporterResponse response,
            LogController controller)
        {
            response.Folder = request.SaveToFolder.FolderPath;
            controller.LogLiteral("Loading Metadata");
            ProcessForEntities(request, response, controller);
            ProcessForFields(request, response, controller);
            ProcessForRelationships(request, response, controller);
            ProcessForOptionSets(request, response, controller);
        }

        private void ProcessForRelationships(CustomisationExporterRequest request, CustomisationExporterResponse response,
            LogController controller)
        {
            if (request.ExportRelationships)
            {
                var allRelationship = new List<RelationshipExport>();
                var types = GetRecordTypesToExport(request).OrderBy(t => Service.GetDisplayName(t)).ToArray();
                var count = types.Count();
                var manyToManyDone = new List<string>();
                for (var i = 0; i < count; i++)
                {
                    var thisType = types.ElementAt(i);
                    var thisTypeLabel = Service.GetDisplayName(thisType);
                    controller.UpdateProgress(i, count, "Exporting Relationships For " + thisTypeLabel);
                    try
                    {
                        var relationships = Service.GetManyToManyRelationships(thisType);
                        for (var j = 0; j < relationships.Count(); j++)
                        {
                            var relationship = relationships.ElementAt(j);
                            try
                            {
                                if (relationship.Entity1LogicalName == thisType
                                    || (!request.DuplicateManyToManyRelationshipSides && !manyToManyDone.Contains(relationship.SchemaName))
                                    )
                                {
                                    allRelationship.Add(new RelationshipExport(relationship.SchemaName,
                                        relationship.Entity1LogicalName, relationship.Entity2LogicalName,
                                        Service.IsCustomRelationship(relationship.SchemaName), Service.IsDisplayRelated(relationship, false), Service.IsDisplayRelated(relationship, true)
                                        , relationship.Entity1IntersectAttribute, relationship.Entity2IntersectAttribute, RelationshipExport.RelationshipType.ManyToMany, Service.IsCustomLabel(relationship, false), Service.IsCustomLabel(relationship, true), Service.GetRelationshipLabel(relationship, false), Service.GetRelationshipLabel(relationship, true)
                                        , Service.GetDisplayOrder(relationship, false), Service.GetDisplayOrder(relationship, true)));
                                    manyToManyDone.Add(relationship.SchemaName);
                                }
                                if (relationship.Entity2LogicalName == thisType
                                    && (request.DuplicateManyToManyRelationshipSides
                                    || (!manyToManyDone.Contains(relationship.SchemaName))))
                                {
                                    allRelationship.Add(new RelationshipExport(relationship.SchemaName,
                                        relationship.Entity2LogicalName, relationship.Entity1LogicalName,
                                        Service.IsCustomRelationship(relationship.SchemaName), Service.IsDisplayRelated(relationship, true), Service.IsDisplayRelated(relationship, false)
                                        , relationship.Entity2IntersectAttribute, relationship.Entity1IntersectAttribute, RelationshipExport.RelationshipType.ManyToMany, Service.IsCustomLabel(relationship, true), Service.IsCustomLabel(relationship, false), Service.GetRelationshipLabel(relationship, true), Service.GetRelationshipLabel(relationship, false)
                                        , Service.GetDisplayOrder(relationship, true), Service.GetDisplayOrder(relationship, false)));
                                    manyToManyDone.Add(relationship.SchemaName);
                                }
                            }
                            catch (Exception ex)
                            {
                                response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Relationship",
                                    relationship.SchemaName, ex));
                            }
                        }
                        if (request.IncludeOneToManyRelationships)
                        {
                            var oneTorelationships = Service.GetOneToManyRelationships(thisType);
                            for (var j = 0; j < oneTorelationships.Count(); j++)
                            {
                                var relationship = oneTorelationships.ElementAt(j);
                                try
                                {
                                    allRelationship.Add(new RelationshipExport(relationship.SchemaName,
                                        relationship.ReferencedEntity, relationship.ReferencingEntity,
                                        Service.IsCustomField(relationship.ReferencingAttribute,
                                            relationship.ReferencingEntity), false,
                                        Service.IsDisplayRelated(relationship)
                                        ,null, relationship.ReferencingAttribute,
                                        RelationshipExport.RelationshipType.OneToMany, false, Service.IsCustomLabel(relationship), null, Service.GetRelationshipLabel(relationship)
                                        , 0, Service.GetDisplayOrder(relationship)));
                                }
                                catch (Exception ex)
                                {
                                    response.AddResponseItem(
                                        new CustomisationExporterResponseItem("Error Exporting Relationship",
                                            relationship.SchemaName, ex));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Relationships",
                            thisType, ex));
                    }
                }
                var fileName = "RelationshipExport_" + DateTime.Now.ToFileTime() + ".csv";
                CsvUtility.CreateCsv(request.SaveToFolder.FolderPath, fileName, allRelationship);
                response.RelationshipsFileName = fileName;
                response.Folder = request.SaveToFolder.FolderPath;
            }
        }

        private static IEnumerable<RecordFieldType> OptionSetTypes
        {
            get { return new[] { RecordFieldType.Picklist, RecordFieldType.Status }; }
        }

        private void ProcessForOptionSets(CustomisationExporterRequest request, CustomisationExporterResponse response, LogController controller)
        {
            if (request.ExportOptionSets)
            {
                var allOptions = new List<OptionExport>();
                var types = GetRecordTypesToExport(request).OrderBy(t => Service.GetDisplayName(t)).ToArray();
                var count = types.Count();
                for (var i = 0; i < count; i++)
                {
                    var thisType = types.ElementAt(i);
                    var thisTypeLabel = Service.GetDisplayName(thisType);
                    controller.UpdateProgress(i, count, "Exporting Options For " + thisTypeLabel);
                    try
                    {
                        var fields =
                            Service.GetFields(thisType)
                                .Where(f => !Service.GetFieldLabel(f, thisType).IsNullOrWhiteSpace())
                                .Where(f => OptionSetTypes.Contains(Service.GetFieldType(f, thisType)))
                                .ToArray();
                        var numberOfFields = fields.Count();
                        for (var j = 0; j < numberOfFields; j++)
                        {
                            var field = fields.ElementAt(j);
                            var fieldLabel = Service.GetFieldLabel(field, thisType);
                            try
                            {
                                var keyValues = Service.GetPicklistKeyValues(field, thisType);
                                foreach (var keyValue in keyValues)
                                {
                                    allOptions.Add(new OptionExport(thisTypeLabel, thisType,
                                        fieldLabel, field, keyValue.Key, keyValue.Value, false, null, JoinTypeAndFieldName(field, thisType)));
                                }
                            }
                            catch (Exception ex)
                            {
                                response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Options For Field",
                                    field, ex));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Record Type Options",
                            thisType, ex));
                    }
                }
                if (request.ExportSharedOptionSets)
                {
                    var sets = Service.GetSharedOptionSetNames();
                    var countSets = sets.Count();
                    for (var i = 0; i < countSets; i++)
                    {
                        var thisSet = sets.ElementAt(i);
                        controller.UpdateProgress(i, countSets, "Exporting Share Option Sets");
                        try
                        {
                            var options = Service.GetSharedOptionSetKeyValues(thisSet);
                            var label = Service.GetSharedPicklistDisplayName(thisSet);
                            foreach (var option in options)
                            {
                                allOptions.Add(new OptionExport(null, null,
                                            null, null, option.Key, option.Value, true, thisSet, label));
                            }
                        }
                        catch (Exception ex)
                        {
                            response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Shared Option Set",
                                thisSet, ex));
                        }
                    }
                }
                var fileName = "OptionsExport_" + DateTime.Now.ToFileTime() + ".csv";
                CsvUtility.CreateCsv(request.SaveToFolder.FolderPath, fileName, allOptions);
                response.OptionSetsFileName = fileName;
                response.Folder = request.SaveToFolder.FolderPath;
            }
        }

        private void ProcessForFields(CustomisationExporterRequest request, CustomisationExporterResponse response,
            LogController controller)
        {
            if (request.ExportFields)
            {
                var allFields = new List<FieldExport>();
                var types = GetRecordTypesToExport(request).OrderBy(t => Service.GetDisplayName(t)).ToArray();
                var count = types.Count();
                for (var i = 0; i < count; i++)
                {
                    var thisType = types.ElementAt(i);
                    var thisTypeLabel = Service.GetDisplayName(thisType);
                    var primaryField = Service.GetPrimaryField(thisType);
                    controller.UpdateProgress(i, count, "Exporting Fields For " + thisTypeLabel);
                    try
                    {
                        var fields =
                            Service.GetFields(thisType)
                                .Where(f => !Service.GetFieldLabel(f, thisType).IsNullOrWhiteSpace())
                                .ToArray();
                        var numberOfFields = fields.Count();
                        for (var j = 0; j < numberOfFields; j++)
                        {
                            var field = fields.ElementAt(j);
                            var fieldLabel = Service.GetFieldLabel(field, thisType);
                            try
                            {
                                var thisFieldType = Service.GetFieldType(field, thisType);
                                var displayRelated = Service.IsLookup(field, thisType) && Service.IsFieldDisplayRelated(field, thisType);
                                var picklist = thisFieldType == RecordFieldType.Picklist
                                    ? CreatePicklistName(field, thisType)
                                    : "N/A";
                                string referencedType = "N/A";
                                if (Service.IsCustomer(thisType, field))
                                {
                                    referencedType = "account,contact";
                                }
                                else if (field == "ownerid")
                                {
                                    referencedType = "systemuser,team";
                                }
                                else if (Service.IsLookup(field, thisType))
                                {
                                    referencedType = Service.GetLookupTargetType(field, thisType);
                                }
                                var isString = Service.IsString(field, thisType);
                                int maxLength = isString ? Service.GetMaxLength(field, thisType) : -1;
                                var textFormat = thisFieldType == RecordFieldType.String ? Service.GetTextFormat(field, thisType).ToString() : null;
                                var includeTime = thisFieldType == RecordFieldType.Date && Service.IsDateIncludeTime(field, thisType);
                                var minValue = "-1";
                                var maxValue = "-1";
                                if (thisFieldType == RecordFieldType.Decimal)
                                {
                                    minValue = Service.GetMinDecimalValue(field, thisType).ToString(CultureInfo.InvariantCulture);
                                    maxValue = Service.GetMaxDecimalValue(field, thisType).ToString(CultureInfo.InvariantCulture);
                                }
                                if (thisFieldType == RecordFieldType.Double)
                                {
                                    minValue = Service.GetMinDoubleValue(field, thisType).ToString(CultureInfo.InvariantCulture);
                                    maxValue = Service.GetMaxDoubleValue(field, thisType).ToString(CultureInfo.InvariantCulture);
                                }
                                if (thisFieldType == RecordFieldType.Integer)
                                {
                                    minValue = Service.GetMinIntValue(field, thisType).ToString(CultureInfo.InvariantCulture);
                                    maxValue = Service.GetMaxIntValue(field, thisType).ToString(CultureInfo.InvariantCulture);
                                }
                                if (thisFieldType == RecordFieldType.Money)
                                {
                                    minValue = Service.GetMinMoneyValue(field, thisType).ToString(CultureInfo.InvariantCulture);
                                    maxValue = Service.GetMaxMoneyValue(field, thisType).ToString(CultureInfo.InvariantCulture);
                                }

                                var fieldExport = new FieldExport(thisTypeLabel, thisType,
                                    fieldLabel, field, Service.GetFieldType(field, thisType),
                                    Service.IsCustomField(field, thisType), Service.IsMandatory(field, thisType), Service.GetFieldDescription(field, thisType), primaryField == field, Service.IsFieldAuditOn(field, thisType), Service.IsFieldSearchable(field, thisType)
                                    , displayRelated, referencedType, maxLength, textFormat, includeTime, minValue, maxValue, picklist);
                                if (Service.IsString(field, thisType))

                                    fieldExport.MaxLength = Service.GetMaxLength(field, thisType);                                {
                                }
                                allFields.Add(fieldExport);
                            }
                            catch (Exception ex)
                            {
                                response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Field",
                                    field, ex));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Record Type Fields",
                            thisType, ex));
                    }
                }
                var fileName = "FieldExport_" + DateTime.Now.ToFileTime() + ".csv";
                CsvUtility.CreateCsv(request.SaveToFolder.FolderPath, fileName, allFields);
                response.FieldsFileName = fileName;
                response.Folder = request.SaveToFolder.FolderPath;
            }
        }

        private string CreatePicklistName(string field, string thisType)
        {
            return Service.IsSharedPicklist(field, thisType)
                ? Service.GetSharedPicklistDisplayName(field, thisType)
                : JoinTypeAndFieldName(field, thisType);
        }

        private static string JoinTypeAndFieldName(string field, string thisType)
        {
            return string.Format("{0}.{1}", thisType, field);
        }

        private void ProcessForEntities(CustomisationExporterRequest request, CustomisationExporterResponse response,
            LogController controller)
        {
            if (request.ExportEntities)
            {
                var allEntities = new List<EntityExport>();
                var types = GetRecordTypesToExport(request).OrderBy(t => Service.GetDisplayName(t)).ToArray();
                var count = types.Count();
                for (var i = 0; i < count; i++)
                {
                    var thisType = types.ElementAt(i);
                    var thisTypeLabel = Service.GetDisplayName(thisType);
                    controller.UpdateProgress(i, count, "Exporting Record Type " + thisTypeLabel);
                    try
                    {
                        allEntities.Add(new EntityExport(thisTypeLabel, thisType, Service.IsCustomType(thisType),
                            Service.GetSqlViewName(thisType), Service.GetRecordTypeCode(thisType), Service.GetCollectionName(thisType), Service.GetDescription(thisType)
                            , Service.IsAuditOn(thisType), Service.IsActivityType(thisType), Service.HasNotes(thisType), Service.HasActivities(thisType)
                            , Service.HasConnections(thisType), Service.HasMailMerge(thisType), Service.HasQueues(thisType)));
                    }
                    catch (Exception ex)
                    {
                        response.AddResponseItem(new CustomisationExporterResponseItem("Error Exporting Record Type",
                            thisType, ex));
                    }
                }
                var fileName = "TypesExport_" + DateTime.Now.ToFileTime() + ".csv";
                CsvUtility.CreateCsv(request.SaveToFolder.FolderPath, fileName, allEntities);
                response.TypesFileName = fileName;
            }
        }

        private IEnumerable<string> GetRecordTypesToExport(CustomisationExporterRequest request)
        {
            var recordTypes = request.AllRecordTypes
                ? Service.GetAllRecordTypes()
                : request.RecordTypes.Select(r => r.RecordType.Key);
            return recordTypes.Where(r => !Service.GetDisplayName(r).IsNullOrWhiteSpace()).ToArray();
        }
    }
}