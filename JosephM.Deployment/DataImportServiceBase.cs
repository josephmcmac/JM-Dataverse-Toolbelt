#region

using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Xrm;
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

namespace JosephM.Deployment
{
    public abstract class DataImportServiceBase<TReq, Tres, TResItem>
        : ServiceBase<TReq, Tres, TResItem>
        where TReq : ServiceRequestBase
        where Tres : ServiceResponseBase<TResItem>, new()
        where TResItem : ServiceResponseItem
    {
        public DataImportServiceBase(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public XrmRecordService XrmRecordService { get; set; }

        protected XrmService XrmService
        {
            get
            {
                return XrmRecordService.XrmService;
            }
        }

        public string GetBaseTransactionId(IRecordService service)
        {
            var organisation = service.GetFirst("organization");
            return organisation.GetLookupId("basecurrencyid");
        }

        protected IEnumerable<Entity> GetMatchingEntities(string type, IDictionary<string,object> fieldValues)
        {
            var conditions = fieldValues.Select(fv =>
            fv.Value == null
            ? new ConditionExpression(fv.Key, ConditionOperator.Null)
            : new ConditionExpression(fv.Key, ConditionOperator.Equal, XrmService.ConvertToQueryValue(fv.Key, type, XrmService.ParseField(fv.Key, type, fv.Value)))
            ).ToList();
            if (type == "workflow")
                conditions.Add(new ConditionExpression("type", ConditionOperator.Equal, XrmPicklists.WorkflowType.Definition));
            if (type == "account" || type == "contact")
                conditions.Add(new ConditionExpression("merged", ConditionOperator.NotEqual, true));
            if (type == "knowledgearticle")
                conditions.Add(new ConditionExpression("islatestversion", ConditionOperator.Equal, true));
            return XrmService.RetrieveAllAndClauses(type, conditions, new String[0]);
        }

        protected IEnumerable<Entity> GetMatchingEntities(string type, string field, string value)
        {
            var typeConfig = XrmRecordService.GetTypeConfigs().GetFor(type);
            if (typeConfig == null || typeConfig.ParentLookupType != type || field != XrmService.GetPrimaryNameField(type))
            {
                return GetMatchingEntities(type, new Dictionary<string, object>()
                {
                    { field, value }
                });
            }
            else
            {
                //okay so this is basically where we are resolving a lookup field by name
                //and the referenced type has a parent/child config
                //e.g. web pages
                //so the lookup should reference the root one
                return GetMatchesByNameForRootRecord(typeConfig, value);
            }
        }

        private Entity GetUniqueMatchingEntity(string type, string field, string value)
        {
            var matchingRecords = GetMatchingEntities(type, field, value);
            if (!matchingRecords.Any())
                throw new NullReferenceException(string.Format("No Record Matched To The {0} Of {1} When Matching The Name",
                        "Name", value));
            if (matchingRecords.Count() > 1)
                throw new Exception(string.Format("More Than One Record Match To The {0} Of {1} When Matching The Name",
                    "Name", value));
            return matchingRecords.First();
        }

        private string MapColumnToFieldSchemaName(XrmService service, string type, string column)
        {
            if (column.StartsWith("key|"))
                column = column.Substring(4);
            var fields = service.GetFields(type);
            var fieldsForLabel = fields.Where(f => service.GetFieldLabel(f, type) == column);
            if (fieldsForLabel.Count() == 1)
                return fieldsForLabel.First();
            var fieldsForName = fields.Where(t => t.ToLower() == column.ToLower());
            if (fieldsForName.Any())
                return fieldsForName.First();
            throw new NullReferenceException(string.Format("No Unique Field Found On Record Type {0} Matched (Label Or Name) For Column Of {1}", type, column));
        }

        private class GetTargetTypeResponse
        {
            public GetTargetTypeResponse(string logicalName, bool isRelationship)
            {
                LogicalName = logicalName;
                IsRelationship = isRelationship;
            }

            public bool IsRelationship { get; set; }
            public string LogicalName { get; set; }
        }

        private GetTargetTypeResponse GetTargetType(XrmService service, string csvName)
        {
            var name = csvName;
            if (name.EndsWith(".csv"))
                name = name.Substring(0, name.IndexOf(".csv", StringComparison.Ordinal));
            name = Path.GetFileName(name);
            var recordTypes = service.GetAllEntityTypes();
            var typesForLabel = recordTypes.Where(t => service.GetEntityDisplayName(t) == name || service.GetEntityCollectionName(t) == name);
            if (typesForLabel.Count() == 1)
                return new GetTargetTypeResponse(typesForLabel.First(), false);
            var typesForName = recordTypes.Where(t => t == name);
            if (typesForName.Any())
                return new GetTargetTypeResponse(typesForName.First(), false);

            var relationshipEntities = service.GetAllNnRelationshipEntityNames();
            var matchingRelationships = relationshipEntities.Where(r => r == name);
            if (matchingRelationships.Count() == 1)
                return new GetTargetTypeResponse(matchingRelationships.First(), true);

            throw new NullReferenceException(string.Format("No Unique Record Type Or Relationship Matched (Label Or Name) For CSV Name Of {0}", name));
        }

        public IEnumerable<DataImportResponseItem> DoImport(IEnumerable<Entity> entities, LogController controller, bool maskEmails, bool matchExistingRecords = true)
        {
            controller.LogLiteral("Preparing Import");
            var response = new List<DataImportResponseItem>();

            var fieldsToRetry = new Dictionary<Entity, List<string>>();
            var typesToImport = entities.Select(e => e.LogicalName).Distinct();

            var allNNRelationships = XrmService.GetAllNnRelationshipEntityNames();
            var associationTypes = typesToImport.Where(allNNRelationships.Contains).ToArray();

            typesToImport = typesToImport.Where(t => !associationTypes.Contains(t)).ToArray();

            var orderedTypes = new List<string>();

            var idSwitches = new Dictionary<string, Dictionary<Guid, Guid>>();
            foreach (var item in typesToImport)
                idSwitches.Add(item, new Dictionary<Guid, Guid>());

            #region tryordertypes


            foreach (var type in typesToImport)
            {
                //iterate through the types and if any of them have a lookup which references this type
                //then insert this one before it for import first
                //otherwise just append to the end
                foreach (var type2 in orderedTypes)
                {
                    var thatType = type2;
                    var thatTypeEntities = entities.Where(e => e.LogicalName == thatType).ToList();
                    var fields = GetFieldsToImport(thatTypeEntities, thatType)
                        .Where(f => XrmService.FieldExists(f, thatType) && XrmService.IsLookup(f, thatType));

                    foreach (var field in fields)
                    {
                        if (thatTypeEntities.Any(e => e.GetLookupType(field) == type))
                        {
                            orderedTypes.Insert(orderedTypes.IndexOf(type2), type);
                            break;
                        }
                    }
                    if (orderedTypes.Contains(type))
                        break;
                }
                if (!orderedTypes.Contains(type))
                    orderedTypes.Add(type);
            }

            //these priorities are because when the first type gets create it creates a 'child' of the second type
            //so we need to ensure the parent created first
            var prioritiseOver = new List<KeyValuePair<string, string>>();
            prioritiseOver.Add(new KeyValuePair<string, string>(Entities.team, Entities.queue));
            prioritiseOver.Add(new KeyValuePair<string, string>(Entities.uomschedule, Entities.uom));
            foreach (var item in prioritiseOver)
            {
                //if the first item is after the second item in the list
                //then remove and insert it before the second item
                if (orderedTypes.Contains(item.Key) && orderedTypes.Contains(item.Value))
                {
                    var indexOfFirst = orderedTypes.IndexOf(item.Key);
                    var indexOfSecond = orderedTypes.IndexOf(item.Value);
                    if(indexOfFirst > indexOfSecond)
                    {
                        orderedTypes.RemoveAt(indexOfFirst);
                        orderedTypes.Insert(indexOfSecond, item.Key);
                    }
                }
            }

            #endregion tryordertypes
            var estimator = new TaskEstimator(1);

            var countToImport = orderedTypes.Count;
            var countImported = 0;
            foreach (var recordType in orderedTypes)
            {
                try
                {
                    var thisRecordType = recordType;
                    var thisTypesConfig = XrmRecordService.GetTypeConfigs().GetFor(recordType);
                    var displayPrefix = $"Importing {recordType} Records ({countImported + 1}/{countToImport})";
                    controller.UpdateProgress(countImported++, countToImport, string.Format("Importing {0} Records", recordType));
                    controller.UpdateLevel2Progress(0, 1, "Loading");
                    var primaryField = XrmService.GetPrimaryNameField(recordType);
                    var thisTypeEntities = entities.Where(e => e.LogicalName == recordType).ToList();

                    var orConditions = thisTypeEntities
                        .Select(
                            e =>
                                new ConditionExpression(XrmService.GetPrimaryKeyField(e.LogicalName),
                                    ConditionOperator.Equal, e.Id))
                        .ToArray();
                    var existingEntities = XrmService.RetrieveAllOrClauses(recordType, orConditions);

                    var orderedEntities = new List<Entity>();

                    #region tryorderentities

                    var importFieldsForEntity = GetFieldsToImport(thisTypeEntities, recordType).ToArray();
                    var fieldsDontExist = GetFieldsInEntities(thisTypeEntities)
                        .Where(f => !f.Contains("."))
                        .Where(f => !XrmService.FieldExists(f, thisRecordType))
                        .Where(f => !HardcodedIgnoreFields.Contains(f))
                        .Distinct()
                        .ToArray();
                    foreach (var field in fieldsDontExist)
                    {
                        response.Add(
                                new DataImportResponseItem(recordType, field, null,
                                string.Format("Field {0} On Entity {1} Doesn't Exist In Target Instance And Will Be Ignored", field, recordType),
                                new NullReferenceException(string.Format("Field {0} On Entity {1} Doesn't Exist In Target Instance And Will Be Ignored", field, recordType))));
                    }

                    var selfReferenceFields = importFieldsForEntity.Where(
                                f =>
                                    XrmService.IsLookup(f, recordType) &&
                                    XrmService.GetLookupTargetEntity(f, recordType) == recordType).ToArray();

                    foreach (var entity in thisTypeEntities)
                    {
                        foreach (var entity2 in orderedEntities)
                        {
                            if (selfReferenceFields.Any(f => entity2.GetLookupGuid(f) == entity.Id || (entity2.GetLookupGuid(f) == Guid.Empty && entity2.GetLookupName(f) == entity.GetStringField(primaryField))))
                            {
                                orderedEntities.Insert(orderedEntities.IndexOf(entity2), entity);
                                break;
                            }
                        }
                        if (!orderedEntities.Contains(entity))
                            orderedEntities.Add(entity);
                    }

                    #endregion tryorderentities

                    var countRecordsToImport = orderedEntities.Count;
                    var countRecordsImported = 0;
                    estimator = new TaskEstimator(countRecordsToImport);

                    foreach (var entity in orderedEntities)
                    {
                        var thisEntity = entity;
                        try
                        {
                            var existingMatchingIds = matchExistingRecords
                                ? GetMatchForExistingRecord(existingEntities, thisEntity)
                                : new Entity[0];
                            if (existingMatchingIds.Any())
                            {
                                var matchRecord = existingMatchingIds.First();
                                if (thisEntity.Id != Guid.Empty)
                                    idSwitches[recordType].Add(thisEntity.Id, matchRecord.Id);
                                thisEntity.Id = matchRecord.Id;
                                thisEntity.SetField(XrmService.GetPrimaryKeyField(thisEntity.LogicalName), thisEntity.Id);
                                if(thisTypesConfig != null)
                                {
                                    thisEntity.SetField(thisTypesConfig.ParentLookupField, matchRecord.GetField(thisTypesConfig.ParentLookupField));
                                    if(thisTypesConfig.UniqueChildFields != null)
                                    {
                                        foreach(var childField in thisTypesConfig.UniqueChildFields)
                                            thisEntity.SetField(thisTypesConfig.ParentLookupField, matchRecord.GetField(thisTypesConfig.ParentLookupField));
                                    }
                                }
                            }
                            var isUpdate = existingMatchingIds.Any();
                            //parse all but aliased fields
                            foreach (var field in thisEntity.GetFieldsInEntity().Where(f => !f.Contains(".")).ToArray())
                            {
                                if (importFieldsForEntity.Contains(field) &&
                                    XrmService.IsLookup(field, thisEntity.LogicalName) &&
                                    thisEntity.GetField(field) != null)
                                {
                                    var idNullable = thisEntity.GetLookupGuid(field);
                                    if (idNullable.HasValue)
                                    {
                                        var targetTypesToTry = GetTargetTypesToTry(thisEntity, field);
                                        var name = thisEntity.GetLookupName(field);
                                        var fieldResolved = false;
                                        foreach (var lookupEntity in targetTypesToTry)
                                        {
                                            var targetPrimaryKey = XrmRecordService.GetPrimaryKey(lookupEntity);
                                            var targetPrimaryField = XrmRecordService.GetPrimaryField(lookupEntity);
                                            var matchRecord = XrmService.GetFirst(lookupEntity, targetPrimaryKey,
                                                idNullable.Value);
                                            if (matchRecord != null)
                                            {
                                                ((EntityReference)(thisEntity.GetField(field))).Name = matchRecord.GetStringField(targetPrimaryField);
                                                fieldResolved = true;
                                            }
                                            else
                                            {
                                                var matchRecords = name.IsNullOrWhiteSpace() ?
                                                    new Entity[0] :
                                                    GetMatchingEntities(lookupEntity,
                                                    targetPrimaryField,
                                                    name);
                                                if (matchRecords.Count() == 1)
                                                {
                                                    thisEntity.SetLookupField(field, matchRecords.First());
                                                    ((EntityReference)(thisEntity.GetField(field))).Name = name;
                                                    fieldResolved = true;
                                                }
                                            }
                                            if (!fieldResolved)
                                            {
                                                if (!fieldsToRetry.ContainsKey(thisEntity))
                                                    fieldsToRetry.Add(thisEntity, new List<string>());
                                                fieldsToRetry[thisEntity].Add(field);
                                            }
                                        }
                                    }
                                }
                            }
                            var fieldsToSet = new List<string>();
                            fieldsToSet.AddRange(thisEntity.GetFieldsInEntity()
                                .Where(importFieldsForEntity.Contains));
                            if (fieldsToRetry.ContainsKey(thisEntity))
                                fieldsToSet.RemoveAll(f => fieldsToRetry[thisEntity].Contains(f));

                            if (maskEmails)
                            {
                                var emailFields = new[] { "emailaddress1", "emailaddress2", "emailaddress3" };
                                foreach (var field in emailFields)
                                {
                                    var theEmail = thisEntity.GetStringField(field);
                                    if (!string.IsNullOrWhiteSpace(theEmail))
                                    {
                                        thisEntity.SetField(field, theEmail.Replace("@", "_AT_") + "_@fakemaskedemail.com");
                                    }
                                }
                            }

                            if (isUpdate)
                            {
                                var existingRecord = existingMatchingIds.First();
                                XrmService.Update(thisEntity, fieldsToSet.Where(f => !XrmEntity.FieldsEqual(existingRecord.GetField(f), thisEntity.GetField(f))));
                            }
                            else
                            {
                                PopulateRequiredCreateFields(fieldsToRetry, thisEntity, fieldsToSet);
                                CheckThrowValidForCreate(thisEntity, fieldsToSet);
                                thisEntity.Id = XrmService.Create(thisEntity, fieldsToSet);
                            }
                            if (!isUpdate && thisEntity.GetOptionSetValue("statecode") > 0)
                                XrmService.SetState(thisEntity, thisEntity.GetOptionSetValue("statecode"), thisEntity.GetOptionSetValue("statuscode"));
                            else if (isUpdate && existingMatchingIds.Any())
                            {
                                var matchRecord = existingMatchingIds.First();
                                var thisState = thisEntity.GetOptionSetValue("statecode");
                                var thisStatus = thisEntity.GetOptionSetValue("statuscode");
                                var matchState = matchRecord.GetOptionSetValue("statecode");
                                var matchStatus = matchRecord.GetOptionSetValue("statuscode");
                                if ((thisState != -1 && thisState != matchState)
                                    ||  (thisStatus != -1 && thisState != matchStatus))
                                {
                                    XrmService.SetState(thisEntity, thisEntity.GetOptionSetValue("statecode"), thisEntity.GetOptionSetValue("statuscode"));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (fieldsToRetry.ContainsKey(thisEntity))
                                fieldsToRetry.Remove(thisEntity);
                            response.Add(
                                new DataImportResponseItem(recordType, null, entity.GetStringField(primaryField),
                                    string.Format("Error Importing Record Id={0}", entity.Id),
                                    ex));
                        }
                        countRecordsImported++;
                        controller.UpdateLevel2Progress(countRecordsImported, countRecordsToImport, estimator.GetProgressString(countRecordsImported));
                    }
                }
                catch (Exception ex)
                {
                    response.Add(
                        new DataImportResponseItem(recordType, null, null, string.Format("Error Importing Type {0}", recordType), ex));
                }
            }
            controller.TurnOffLevel2();
            countToImport = fieldsToRetry.Count;
            countImported = 0;
            controller.UpdateProgress(countImported, countToImport, "Retrying Unresolved Fields");
            estimator = new TaskEstimator(countToImport);
            foreach (var item in fieldsToRetry)
            {
                var thisEntity = item.Key;
                controller.UpdateProgress(countImported++, countToImport, string.Format("Retrying Unresolved Fields {0}", thisEntity.LogicalName));
                var thisPrimaryField = XrmService.GetPrimaryNameField(thisEntity.LogicalName);
                try
                {
                    foreach (var field in item.Value)
                    {
                        if (XrmService.IsLookup(field, thisEntity.LogicalName) && thisEntity.GetField(field) != null)
                        {
                            try
                            {
                                var targetTypesToTry = GetTargetTypesToTry(thisEntity, field);
                                var name = thisEntity.GetLookupName(field);
                                var idNullable = thisEntity.GetLookupGuid(field);
                                var fieldResolved = false;
                                foreach (var lookupEntity in targetTypesToTry)
                                {
                                    var targetPrimaryKey = XrmRecordService.GetPrimaryKey(lookupEntity);
                                    var targetPrimaryField = XrmRecordService.GetPrimaryField(lookupEntity);
                                    var matchRecord = idNullable.HasValue ? XrmService.GetFirst(lookupEntity, targetPrimaryKey,
                                        idNullable.Value) : null;
                                    if (matchRecord != null)
                                    {
                                        thisEntity.SetLookupField(field, matchRecord);
                                        ((EntityReference)(thisEntity.GetField(field))).Name = matchRecord.GetStringField(targetPrimaryField);
                                        fieldResolved = true;
                                    }
                                    else
                                    {
                                        var matchRecords = name.IsNullOrWhiteSpace() ?
                                            new Entity[0] :
                                            GetMatchingEntities(lookupEntity,
                                            targetPrimaryField,
                                            name);
                                        if (matchRecords.Count() == 1)
                                        {
                                            thisEntity.SetLookupField(field, matchRecords.First());
                                            ((EntityReference)(thisEntity.GetField(field))).Name = name;
                                            fieldResolved = true;
                                        }
                                    }
                                    if (!fieldResolved)
                                    {
                                        throw new Exception(string.Format("Could not find matching record for field {0}.{1} {2}", thisEntity.LogicalName, field, name));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                if (thisEntity.Contains(field))
                                    thisEntity.Attributes.Remove(field);
                                response.Add(
                                     new DataImportResponseItem(thisEntity.LogicalName, field, thisEntity.GetStringField(thisPrimaryField),
                                        string.Format("Error Setting Lookup Field Id={0}", thisEntity.Id), ex));
                            }
                        }
                    }
                    XrmService.Update(thisEntity, item.Value);
                }
                catch (Exception ex)
                {
                    response.Add(
                        new DataImportResponseItem(thisEntity.LogicalName, null, thisEntity.GetStringField(thisPrimaryField),
                            string.Format("Error Importing Record Id={0}", thisEntity.Id),
                            ex));
                }
                countImported++;
                controller.UpdateProgress(countImported, countToImport, estimator.GetProgressString(countImported, taskName: $"Retrying Unresolved Fields {thisEntity.LogicalName}"));
            }
            countToImport = associationTypes.Count();
            countImported = 0;
            foreach (var relationshipEntityName in associationTypes)
            {
                var thisEntityName = relationshipEntityName;
                controller.UpdateProgress(countImported++, countToImport, $"Associating {thisEntityName} Records");
                controller.UpdateLevel2Progress(0, 1, "Loading");
                var thisTypeEntities = entities.Where(e => e.LogicalName == thisEntityName).ToList();
                var countRecordsToImport = thisTypeEntities.Count;
                var countRecordsImported = 0;
                estimator = new TaskEstimator(countRecordsToImport);
                foreach (var thisEntity in thisTypeEntities)
                {
                    try
                    {
                        var relationship = XrmService.GetRelationshipMetadataForEntityName(thisEntityName);
                        var type1 = relationship.Entity1LogicalName;
                        var field1 = relationship.Entity1IntersectAttribute;
                        var type2 = relationship.Entity2LogicalName;
                        var field2 = relationship.Entity2IntersectAttribute;

                        //bit of hack
                        //when importing from csv just set the fields to the string name of the referenced record
                        //so either string when csv or guid when xml import/export
                        var value1 = thisEntity.GetField(relationship.Entity1IntersectAttribute);
                        var id1 = value1 is string
                            ? GetUniqueMatchingEntity(type1, XrmRecordService.GetPrimaryField(type1), (string)value1).Id
                            : thisEntity.GetGuidField(relationship.Entity1IntersectAttribute);

                        var value2 = thisEntity.GetField(relationship.Entity2IntersectAttribute);
                        var id2 = value2 is string
                            ? GetUniqueMatchingEntity(type2, XrmRecordService.GetPrimaryField(type2), (string)value2).Id
                            : thisEntity.GetGuidField(relationship.Entity2IntersectAttribute);

                        //add a where field lookup reference then look it up
                        if (idSwitches.ContainsKey(type1) && idSwitches[type1].ContainsKey(id1))
                            id1 = idSwitches[type1][id1];
                        if (idSwitches.ContainsKey(type2) && idSwitches[type2].ContainsKey(id2))
                            id2 = idSwitches[type2][id2];
                        XrmService.AssociateSafe(relationship.SchemaName, type1, field1, id1, type2, field2, new[] { id2 });
                    }
                    catch (Exception ex)
                    {
                        response.Add(
                        new DataImportResponseItem(
                                string.Format("Error Associating Record Of Type {0} Id {1}", thisEntity.LogicalName,
                                    thisEntity.Id),
                                ex));
                    }
                    countRecordsImported++;
                    controller.UpdateLevel2Progress(countRecordsImported, countRecordsToImport, estimator.GetProgressString(countRecordsImported));
                }
            }
            return response;
        }

        private void PopulateRequiredCreateFields(Dictionary<Entity, List<string>> fieldsToRetry, Entity thisEntity, List<string> fieldsToSet)
        {
            if (thisEntity.LogicalName == Entities.team
                && !fieldsToSet.Contains(Fields.team_.businessunitid)
                && XrmService.FieldExists(Fields.team_.businessunitid, Entities.team))
            {
                thisEntity.SetLookupField(Fields.team_.businessunitid, GetRootBusinessUnitId(), Entities.businessunit);
                fieldsToSet.Add(Fields.team_.businessunitid);
                if (fieldsToRetry.ContainsKey(thisEntity)
                    && fieldsToRetry[thisEntity].Contains(Fields.team_.businessunitid))
                    fieldsToRetry[thisEntity].Remove(Fields.team_.businessunitid);
            }
            if (thisEntity.LogicalName == Entities.subject
                    && !fieldsToSet.Contains(Fields.subject_.featuremask)
                    && XrmService.FieldExists(Fields.subject_.featuremask, Entities.subject))
            {
                thisEntity.SetField(Fields.subject_.featuremask, 1);
                fieldsToSet.Add(Fields.subject_.featuremask);
                if (fieldsToRetry.ContainsKey(thisEntity)
                    && fieldsToRetry[thisEntity].Contains(Fields.subject_.featuremask))
                    fieldsToRetry[thisEntity].Remove(Fields.subject_.featuremask);
            }
            if (thisEntity.LogicalName == Entities.uomschedule)
                
            {
                fieldsToSet.Add(Fields.uomschedule_.baseuomname);
            }
            if (thisEntity.LogicalName == Entities.uom)
            {
                //var uomGroupName = thisEntity.GetLookupName(Fields.uom_.uomscheduleid);
                //var uomGroup = GetUniqueMatchingEntity(Entities.uomschedule, Fields.uomschedule_.name, uomGroupName);
                //thisEntity.SetLookupField(Fields.uom_.uomscheduleid, uomGroup);
                var unitGroupId = thisEntity.GetLookupGuid(Fields.uom_.uomscheduleid);
                if (!unitGroupId.HasValue)
                    throw new NullReferenceException($"Error The {XrmService.GetFieldLabel(Fields.uom_.uomscheduleid, Entities.uom)} Is Not Populated");
                fieldsToSet.Add(Fields.uom_.uomscheduleid);

                var baseUnitName = thisEntity.GetLookupName(Fields.uom_.baseuom);
                var baseUnitMatches = GetMatchingEntities(Entities.uom, new Dictionary<string, object>
                {
                    { Fields.uom_.name, baseUnitName },
                    { Fields.uom_.uomscheduleid, unitGroupId.Value }
                });
                if (baseUnitMatches.Count() == 0)
                    throw new Exception($"Could Not Identify The {XrmService.GetFieldLabel(Fields.uom_.baseuom, Entities.uom)} {baseUnitName}. No Match Found For The {XrmService.GetFieldLabel(Fields.uom_.uomscheduleid, Entities.uom)}");
                if (baseUnitMatches.Count() > 1)
                    throw new Exception($"Could Not Identify The {XrmService.GetFieldLabel(Fields.uom_.baseuom, Entities.uom)} {baseUnitName}. Multiple Matches Found For The {XrmService.GetFieldLabel(Fields.uom_.uomscheduleid, Entities.uom)}");
                thisEntity.SetLookupField(Fields.uom_.baseuom, baseUnitMatches.First());
                fieldsToSet.Add(Fields.uom_.baseuom);
            }
            if (thisEntity.LogicalName == Entities.product)
            {
                var unitGroupId = thisEntity.GetLookupGuid(Fields.product_.defaultuomscheduleid);
                if(unitGroupId.HasValue)
                    fieldsToSet.Add(Fields.product_.defaultuomscheduleid);
                var unitId = thisEntity.GetLookupGuid(Fields.product_.defaultuomid);
                if (unitId.HasValue)
                    fieldsToSet.Add(Fields.product_.defaultuomid);
            }
        }

        private Guid GetRootBusinessUnitId()
        {
            return XrmService.GetFirst(Entities.businessunit, Fields.businessunit_.parentbusinessunitid, null, new string[0]).Id;
        }

        private List<string> GetTargetTypesToTry(Entity thisEntity, string field)
        {
            var targetTypesToTry = new List<string>();

            if (!string.IsNullOrWhiteSpace(thisEntity.GetLookupType(field)))
            {
                targetTypesToTry.AddRange(thisEntity.GetLookupType(field).Split(','));
            }
            else
            {
                switch (XrmRecordService.GetFieldType(field, thisEntity.LogicalName))
                {
                    case Record.Metadata.RecordFieldType.Owner:
                        targetTypesToTry.Add("systemuser");
                        targetTypesToTry.Add("team");
                        break;
                    case Record.Metadata.RecordFieldType.Customer:
                        targetTypesToTry.Add("account");
                        targetTypesToTry.Add("contact");
                        break;
                    case Record.Metadata.RecordFieldType.Lookup:
                        targetTypesToTry.Add(thisEntity.GetLookupType(field));
                        break;
                    default:
                        throw new NotImplementedException(string.Format("Could not determine target type for field {0}.{1} of type {2}", thisEntity.LogicalName, field, XrmService.GetFieldType(field, thisEntity.LogicalName)));
                }
            }

            return targetTypesToTry;
        }

        private IEnumerable<Entity> GetMatchForExistingRecord(IEnumerable<Entity> existingEntitiesWithIdMatches, Entity thisEntity)
        {
            var thisTypesConfig = XrmRecordService.GetTypeConfigs().GetFor(thisEntity.LogicalName);
            if (thisTypesConfig == null)
            {
                //okay this is where we just need to find the matching record by name
                var existingMatches = thisEntity.Id == Guid.Empty
                                ? new Entity[0]
                                : existingEntitiesWithIdMatches.Where(e => e.Id == thisEntity.Id);
                if (!existingMatches.Any())
                {
                    var matchBySpecificFieldEntities = new Dictionary<string, string>()
                    {
                        {  "knowledgearticle", "articlepublicnumber" }
                    };
                    if (thisEntity.LogicalName == "businessunit" && thisEntity.GetField("parentbusinessunitid") == null)
                    {
                        existingMatches = XrmService.RetrieveAllAndClauses("businessunit",
                            new[] { new ConditionExpression("parentbusinessunitid", ConditionOperator.Null) });
                    }
                    else if (matchBySpecificFieldEntities.ContainsKey(thisEntity.LogicalName))
                    {
                        var matchField = matchBySpecificFieldEntities[thisEntity.LogicalName];
                        var valueToMatch = thisEntity.GetStringField(matchField);
                        if (matchField.IsNullOrWhiteSpace())
                            throw new NullReferenceException(string.Format("{0} Is Required On The {1}", XrmService.GetFieldLabel(matchField, thisEntity.LogicalName), XrmService.GetEntityLabel(thisEntity.LogicalName)));
                        existingMatches = GetMatchingEntities(thisEntity.LogicalName, matchField, valueToMatch);
                        if (existingMatches.Count() > 1)
                            throw new Exception(string.Format("More Than One Record Match To The {0} Of {1}",
                                matchField, valueToMatch));
                    }
                    else
                    {
                        var primaryField = XrmService.GetPrimaryNameField(thisEntity.LogicalName);
                        var name = thisEntity.GetStringField(primaryField);
                        if (name.IsNullOrWhiteSpace())
                            throw new NullReferenceException(string.Format("{0} Is Required On The {1}", XrmService.GetFieldLabel(primaryField, thisEntity.LogicalName), XrmService.GetEntityLabel(thisEntity.LogicalName)));
                        existingMatches = GetMatchingEntities(thisEntity.LogicalName, primaryField, name);
                        if (existingMatches.Count() > 1)
                            throw new Exception(string.Format("More Than One Record Match To The {0} Of {1} When Matching The Name",
                                "Name", name));
                    }
                }
                return existingMatches;
            }
            else
            {
                //okay these are where we have a special type which I cannot just match the record name
                //initially implemented for adx where web page have multiple for each language
                //but also for entity form metadata which doesn't have a name
                //and web page access control rules which may not have unique name
                var primaryField = XrmService.GetPrimaryNameField(thisTypesConfig.Type);
                var isParent = thisEntity.GetLookupGuid(thisTypesConfig.ParentLookupField) == null;
                if (isParent)
                {
                    var name = thisEntity.GetStringField(primaryField);
                    return GetMatchesByNameForRootRecord(thisTypesConfig, name);
                }
                else
                {
                    //okay for a child
                    //we need to get the matching parent in the target
                    //then query if one exists for that parent with the unique fields matching
                    var parentName = thisEntity.GetLookupName(thisTypesConfig.ParentLookupField);
                    if(parentName == null)
                        throw new Exception(string.Format("Cannot identify parent record for {0} Of {1} because the parent reference name is empty",
                             XrmService.GetPrimaryNameField(thisTypesConfig.Type), thisEntity.GetStringField(primaryField)));
                    var parentPrimaryNameField = XrmService.GetPrimaryNameField(thisTypesConfig.ParentLookupType);

                    var matchingParentQuery = XrmService.BuildQuery(thisTypesConfig.ParentLookupType, null, new[]
                    {
                        new ConditionExpression(parentPrimaryNameField, ConditionOperator.Equal, parentName)
                    }, null);

                    if (thisTypesConfig.Type == thisTypesConfig.ParentLookupType)
                        matchingParentQuery.Criteria.AddCondition(new ConditionExpression(thisTypesConfig.ParentLookupField, ConditionOperator.Null));
                    else
                    {
                        var thisTypesParentsConfig = XrmRecordService.GetTypeConfigs().GetFor(thisTypesConfig.ParentLookupType);
                        if (thisTypesParentsConfig != null)
                        {
                            //okay so this record should have captured fields in the parent
                            //which are required to match the target parent in aliased value
                            //add the parents parent condition to the query
                            //note we have to use names for the parent condition as ids may not be consistent
                            var parentParentId = XrmEntity.GetLookupGuid(thisEntity.GetFieldValue($"{thisTypesConfig.ParentLookupField}.{thisTypesParentsConfig.ParentLookupField}"));
                            if (!parentParentId.HasValue)
                            {
                                matchingParentQuery.Criteria.AddCondition(new ConditionExpression(thisTypesParentsConfig.ParentLookupField, ConditionOperator.Null));
                            }
                            else
                            {
                                var name = XrmEntity.GetLookupName(thisEntity.GetFieldValue($"{thisTypesConfig.ParentLookupField}.{thisTypesParentsConfig.ParentLookupField}"));
                                var linkToParent = matchingParentQuery.AddLink(thisTypesParentsConfig.ParentLookupType, thisTypesParentsConfig.ParentLookupField, XrmService.GetPrimaryKeyField(thisTypesParentsConfig.ParentLookupType));
                                if (name == null)
                                    linkToParent.LinkCriteria.AddCondition(XrmService.GetPrimaryNameField(thisTypesParentsConfig.ParentLookupType), ConditionOperator.Null);
                                else
                                    linkToParent.LinkCriteria.AddCondition(XrmService.GetPrimaryNameField(thisTypesParentsConfig.ParentLookupType), ConditionOperator.Equal, name);
                            }


                            if (thisTypesParentsConfig.UniqueChildFields != null)
                            {
                                //add the parents unique fields to the query
                                //note we have to use name conditions for lookup fields as may not be consistent
                                foreach (var field in thisTypesParentsConfig.UniqueChildFields)
                                {
                                    var theValue = thisEntity.GetFieldValue($"{thisTypesConfig.ParentLookupField}.{field}");
                                    if (theValue == null)
                                        matchingParentQuery.Criteria.AddCondition(new ConditionExpression(field, ConditionOperator.Null));
                                    else if(theValue is EntityReference)
                                    {
                                        var name = XrmEntity.GetLookupName(theValue);
                                        var type = XrmEntity.GetLookupType(theValue);
                                        var linkToReferenced = matchingParentQuery.AddLink(type, field, XrmService.GetPrimaryKeyField(type));
                                        if (name == null)
                                            linkToReferenced.LinkCriteria.AddCondition(XrmService.GetPrimaryNameField(type), ConditionOperator.Null);
                                        else
                                            linkToReferenced.LinkCriteria.AddCondition(XrmService.GetPrimaryNameField(type), ConditionOperator.Equal, name);
                                    }
                                    else
                                        matchingParentQuery.Criteria.AddCondition(new ConditionExpression(field, ConditionOperator.Equal, XrmService.ConvertToQueryValue(field, thisTypesParentsConfig.ParentLookupType, theValue)));
                                }
                            }
                        }
                    }

                    var matchingParents = XrmService.RetrieveAll(matchingParentQuery);
                    if (matchingParents.Count() != 1)
                        throw new Exception(string.Format("Could Not Find Unique Match For the Parent Record {0} Of {1}",
                            parentPrimaryNameField, parentName));
                    var parent = matchingParents.First();
                    thisEntity.SetLookupField(thisTypesConfig.ParentLookupField, parent);
                    var matchingChildQuery = XrmService.BuildQuery(thisTypesConfig.Type, null, new []
                    {
                        new ConditionExpression(thisTypesConfig.ParentLookupField, ConditionOperator.Equal, parent.Id)
                    }, null);
                    if(thisTypesConfig.UniqueChildFields != null)
                    {
                        foreach (var field in thisTypesConfig.UniqueChildFields)
                        {
                            var theValue = thisEntity.GetFieldValue(field);
                            if (theValue == null)
                                matchingChildQuery.Criteria.AddCondition(new ConditionExpression(field, ConditionOperator.Null));
                            else if (theValue is EntityReference)
                            {
                                var name = XrmEntity.GetLookupName(theValue);
                                var type = XrmEntity.GetLookupType(theValue);
                                var linkToReferenced = matchingChildQuery.AddLink(type, field, XrmService.GetPrimaryKeyField(type));
                                if (name == null)
                                    linkToReferenced.LinkCriteria.AddCondition(XrmService.GetPrimaryNameField(type), ConditionOperator.Null);
                                else
                                    linkToReferenced.LinkCriteria.AddCondition(XrmService.GetPrimaryNameField(type), ConditionOperator.Equal, name);
                            }
                            else
                                matchingChildQuery.Criteria.AddCondition(new ConditionExpression(field, ConditionOperator.Equal, XrmService.ConvertToQueryValue(field, thisEntity.LogicalName, theValue)));
                        }
                    }
                    var matchingChildren = XrmService.RetrieveAll(matchingChildQuery);
                    if (matchingChildren.Count() > 1)
                        throw new Exception(string.Format("More Than One Match For the Child {0} Record Of {1} {2}",
                            thisTypesConfig.Type, parentName, parentPrimaryNameField));
                    if (matchingChildren.Count() == 0 && thisTypesConfig.BlockCreateChild)
                        throw new Exception(string.Format("Creation Prevented For Child {0} Record Of {1} {2}. These Are Expected To Be Created By The System",
                            thisTypesConfig.Type, parentName, parentPrimaryNameField));

                    return matchingChildren;
                }
            }
        }

        private IEnumerable<Entity> GetMatchesByNameForRootRecord(TypeConfigs.Config parentChildConfig, string name)
        {
            //okay if this is a parent record (e.g a root web page)
            //then match by name and where the parent reference is empty
            if (name == null)
                throw new NullReferenceException("Name Is Null For Parent Record");
            var matches = XrmService.RetrieveAllAndClauses(parentChildConfig.Type,
                        new[] {
                                new ConditionExpression(parentChildConfig.ParentLookupField, ConditionOperator.Null),
                                new ConditionExpression(XrmService.GetPrimaryNameField(parentChildConfig.Type), ConditionOperator.Equal, name) });
            if (matches.Count() > 1)
                throw new Exception(string.Format("More Than One Record Match To The {0} Of {1}",
                    XrmService.GetPrimaryNameField(parentChildConfig.Type), name));
            return matches;
        }

        private void CheckThrowValidForCreate(Entity thisEntity, List<string> fieldsToSet)
        {
            if (thisEntity != null)
            {
                switch (thisEntity.LogicalName)
                {
                    case "annotation":
                        if (!fieldsToSet.Contains("objectid"))
                            throw new NullReferenceException(string.Format("Cannot create {0} {1} as its parent {2} does not exist"
                                , XrmService.GetEntityLabel(thisEntity.LogicalName), thisEntity.GetStringField(XrmService.GetPrimaryNameField(thisEntity.LogicalName))
                                , thisEntity.GetStringField("objecttypecode") != null ? XrmService.GetEntityLabel(thisEntity.GetStringField("objecttypecode")) : "Unknown Type"));
                        break;
                    case "productpricelevel":
                        if (!fieldsToSet.Contains("pricelevelid"))
                            throw new NullReferenceException(string.Format("Cannot create {0} {1} as its parent {2} is empty"
                                , XrmService.GetEntityLabel(thisEntity.LogicalName), thisEntity.GetStringField(XrmService.GetPrimaryNameField(thisEntity.LogicalName))
                                , XrmService.GetEntityLabel("pricelevel")));
                        break;
                }
            }
            return;
        }

        private IEnumerable<string> GetFieldsInEntities(IEnumerable<Entity> thisTypeEntities)
        {
            return thisTypeEntities.SelectMany(e => e.GetFieldsInEntity());
        }

        private IEnumerable<string> GetFieldsToImport(IEnumerable<Entity> thisTypeEntities, string type)
        {
            var fields = GetFieldsInEntities(thisTypeEntities)
                .Where(f => IsIncludeField(f, type))
                .Distinct()
                .ToList();
            return fields;
        }

        public bool IsIncludeField(string fieldName, string entityType)
        {
            var hardcodeInvalidFields = HardcodedIgnoreFields;
            if (hardcodeInvalidFields.Contains(fieldName))
                return false;
            //these are just hack since they are not updateable fields (IsWriteable)
            if (fieldName == "parentbusinessunitid")
                return true;
            if (fieldName == "businessunitid")
                return true;
            if (fieldName == "pricelevelid")
                return true;
            if (fieldName == "salesliteratureid")
                return true;
            return
                XrmRecordService.FieldExists(fieldName, entityType) && XrmRecordService.GetFieldMetadata(fieldName, entityType).Writeable;

        }

        private static IEnumerable<string> HardcodedIgnoreFields
        {
            get
            {
                return new[]
                {
                    "yomifullname", "administratorid", "owneridtype", "ownerid", "timezoneruleversionnumber", "utcconversiontimezonecode", "organizationid", "owninguser", "owningbusinessunit","owningteam",
                    "overriddencreatedon", "statuscode", "statecode", "createdby", "createdon", "modifiedby", "modifiedon", "modifiedon", "jmcg_currentnumberposition", "calendarrules", "parentarticlecontentid", "rootarticleid", "previousarticlecontentid"
                };
            }
        }
    }
}
