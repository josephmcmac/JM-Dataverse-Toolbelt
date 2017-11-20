#region

using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

#endregion

namespace JosephM.Deployment.ExportXml
{
    public class ExportXmlService :
        ServiceBase<ExportXmlRequest, ExportXmlResponse, ExportXmlResponseItem>
    {
        public ExportXmlService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        protected XrmRecordService XrmRecordService { get; set; }

        protected XrmService XrmService
        {
            get
            {
                return XrmRecordService.XrmService;
            }
        }

        public override void ExecuteExtention(ExportXmlRequest request, ExportXmlResponse response,
            LogController controller)
        {
            ExportXml(request.RecordTypesToExport, request.Folder, request.IncludeNotes, request.IncludeNNRelationshipsBetweenEntities, controller);
        }



        public void ExportXml(IEnumerable<ExportRecordType> exports, Folder folder, bool includeNotes, bool incoldeNNBetweenEntities, LogController controller)
        {
            if (!Directory.Exists(folder.FolderPath))
                Directory.CreateDirectory(folder.FolderPath);

            ProcessExport(exports, includeNotes, incoldeNNBetweenEntities, controller
                , (entity) => WriteToXml(entity, folder.FolderPath, false)
                , (entity) => WriteToXml(entity, folder.FolderPath, true));
        }

        public void ProcessExport(IEnumerable<ExportRecordType> exports, bool includeNotes, bool incoldeNNBetweenEntities, LogController controller
            , Action<Entity> processEntity, Action<Entity> processAssociation)
        {
            if (exports == null || !exports.Any())
                throw new Exception("Error No Record Types To Export");
            var countToExport = exports.Count();
            var countsExported = 0;
            var exported = new Dictionary<string, List<Entity>>();
            foreach (var exportType in exports)
            {
                var type = exportType.RecordType == null ? null : exportType.RecordType.Key;
                controller.UpdateProgress(countsExported++, countToExport, string.Format("Exporting {0} Records", type));
                var conditions = new List<ConditionExpression>();
                if (type == "list")
                    conditions.Add(new ConditionExpression("type", ConditionOperator.Equal, XrmPicklists.ListType.Dynamic));
                if (type == "knowledgearticle")
                    conditions.Add(new ConditionExpression("islatestversion", ConditionOperator.Equal, true));
                //doesn't work for too many notes
                //should have option on each or all entities for notes maybe
                IEnumerable<Entity> entities;
                switch (exportType.Type)
                {
                    case ExportType.AllRecords:
                        {
                            entities = XrmService.RetrieveAllAndClauses(type, conditions)
                                .Where(e => !CheckIgnoreForExport(exportType, e))
                                .ToArray();
                            break;
                        }
                    case ExportType.FetchXml:
                        {
                            var queryExpression = XrmService.ConvertFetchToQueryExpression(exportType.FetchXml);
                            queryExpression.ColumnSet = new ColumnSet(true);
                            entities = queryExpression.PageInfo != null && queryExpression.PageInfo.Count > 0
                                ? XrmService.RetrieveFirstX(queryExpression, queryExpression.PageInfo.Count)
                                : XrmService.RetrieveAll(queryExpression);
                            entities = entities
                                .Where(e => !CheckIgnoreForExport(exportType, e))
                                .ToArray();
                            break;
                        }
                    case ExportType.SpecificRecords:
                        {
                            var primaryKey = XrmService.GetPrimaryKeyField(type);
                            var ids = exportType.SpecificRecordsToExport == null
                                ? new string[0]
                                : exportType.SpecificRecordsToExport
                                    .Select(r => r.Record == null ? null : r.Record.Id)
                                    .Where(s => !s.IsNullOrWhiteSpace()).Distinct().ToArray();
                            entities = ids.Any()
                                ? XrmService.RetrieveAllOrClauses(type,
                                    ids.Select(
                                        i => new ConditionExpression(primaryKey, ConditionOperator.Equal, new Guid(i))))
                                : new Entity[0];
                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException(string.Format("No Export Implemented For {0} {1} For {2} Records", typeof(ExportType).Name, exportType.Type, type));
                        }
                }

                var excludeFields = exportType.IncludeAllFields
                    ? new string[0]
                    : XrmService.GetFields(exportType.RecordType.Key).Except(exportType.IncludeOnlyTheseFieldsInExportedRecords.Select(f => f.RecordField == null ? null : f.RecordField.Key).Distinct().ToArray());
                var primaryField = XrmService.GetPrimaryNameField(exportType.RecordType.Key);
                if (excludeFields.Contains(primaryField))
                    excludeFields = excludeFields.Except(new[] { primaryField }).ToArray();

                if (exportType.ExplicitValuesToSet != null)
                {
                    foreach (var field in exportType.ExplicitValuesToSet)
                    {
                        foreach (var entity in entities)
                        {
                            entity.SetField(field.FieldToSet.Key, field.ClearValue ? null : field.ValueToSet, XrmService);
                        }
                        if (excludeFields.Contains(field.FieldToSet.Key))
                            excludeFields = excludeFields.Except(new[] { field.FieldToSet.Key }).ToArray();
                    }
                }

                var fieldsAlwaysExclude = new[] { "calendarrules" };
                excludeFields = excludeFields.Union(fieldsAlwaysExclude).ToArray();

                foreach (var entity in entities)
                {
                    entity.RemoveFields(excludeFields);
                    processEntity(entity);
                }
                if (!exported.ContainsKey(type))
                    exported.Add(type, new List<Entity>());
                exported[type].AddRange(entities);
                if (includeNotes)
                {
                    var notes = XrmService
                        .RetrieveAllOrClauses("annotation",
                            new[] { new ConditionExpression("objecttypecode", ConditionOperator.Equal, type) });
                    foreach (var note in notes)
                    {
                        var objectId = note.GetLookupGuid("objectid");
                        if (objectId.HasValue && entities.Select(e => e.Id).Contains(objectId.Value))
                        {
                            processEntity(note);
                        }
                    }
                }
            }
            var relationshipsDone = new List<string>();
            if (incoldeNNBetweenEntities)
            {
                foreach (var type in exported.Keys)
                {
                    var nnRelationships = XrmService.GetEntityManyToManyRelationships(type)
                        .Where(
                            r =>
                                exported.Keys.Contains(r.Entity1LogicalName) && exported.Keys.Contains(r.Entity2LogicalName));
                    foreach (var item in nnRelationships)
                    {
                        var type1 = item.Entity1LogicalName;
                        var type2 = item.Entity2LogicalName;
                        if (!relationshipsDone.Contains(item.SchemaName))
                        {
                            var associations = XrmService.RetrieveAllEntityType(item.IntersectEntityName, null);
                            foreach (var association in associations)
                            {
                                if (exported[type1].Any(e => e.Id == association.GetGuidField(item.Entity1IntersectAttribute))
                                    && exported[type2].Any(e => e.Id == association.GetGuidField(item.Entity2IntersectAttribute)))
                                    processAssociation(association);
                            }
                            relationshipsDone.Add(item.SchemaName);
                        }
                    }
                }
            }
        }

        private void WriteToXml(Entity entity, string folder, bool association)
        {
            var lateBoundSerializer = new DataContractSerializer(typeof(Entity));
            var namesToUse = association ? "association" : entity.GetStringField(XrmRecordService.GetPrimaryField(entity.LogicalName).Left(15));
            if (!namesToUse.IsNullOrWhiteSpace())
            {
                var invalidChars = Path.GetInvalidFileNameChars();
                foreach (var character in invalidChars)
                    namesToUse = namesToUse.Replace(character, '_');
            }
            var fileName = string.Format(@"{0}_{1}_{2}", entity.LogicalName, entity.Id, namesToUse);
            fileName = fileName.Replace('-', '_');
            //ensure dont exceed max filename length
            fileName = string.Format(@"{0}\{1}", folder, fileName).Left(240);
            fileName = fileName + ".xml";
            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                lateBoundSerializer.WriteObject(fileStream, entity);
            }
        }

        private bool CheckIgnoreForExport(ExportRecordType exportType, Entity entity)
        {
            if (XrmService.FieldExists("statecode", entity.LogicalName))
            {
                if (!exportType.IncludeInactiveRecords)
                {
                    var activeStates = new List<int>(new[] { XrmPicklists.State.Active });
                    if (entity.LogicalName == "product")
                        activeStates.AddRange(new[] { 2, 3 });//draft and under revision for latest crm releases
                    if (entity.LogicalName == "knowledgearticle")
                    {
                        activeStates.Clear();
                        activeStates.AddRange(new[] { 1, 2, 3 });//draft and under revision for latest crm releases
                    }
                    if (!activeStates.Contains(entity.GetOptionSetValue("statecode")))
                        return true;
                }
            }
            //include 1 = public
            if (entity.LogicalName == "queue" && entity.GetOptionSetValue("queueviewtype") == 1)
                return true;
            //exclude 1 = customer service 2 = holiday schedule
            if (entity.LogicalName == "calendar" && !new[] { 1, 2, }.Contains(entity.GetOptionSetValue("type")))
                return true;
            return false;
        }
    }
}