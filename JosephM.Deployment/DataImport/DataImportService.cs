using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;


namespace JosephM.Deployment.DataImport
{
    public class DataImportService
    {
        public DataImportService(XrmRecordService xrmRecordService)
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

        private Dictionary<string, Dictionary<string, Dictionary<string, List<Entity>>>> _cachedRecords = new Dictionary<string, Dictionary<string, Dictionary<string, List<Entity>>>>();

        public IEnumerable<Entity> GetMatchingEntities(string type, IDictionary<string, object> fieldValues, string ignoreCacheFor = null)
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

            if(type != ignoreCacheFor
                && conditions.Count == 1
                && fieldValues.Values.First() != null)
            {
                var maxCacheCount = 5000;
                var fieldName = fieldValues.Keys.First();
                var matchString = XrmService.GetFieldAsMatchString(type, fieldName, fieldValues.Values.First());
                if (!_cachedRecords.ContainsKey(type))
                    _cachedRecords.Add(type, new Dictionary<string, Dictionary<string, List<Entity>>>());
                if (!_cachedRecords[type].ContainsKey(fieldName))
                {
                    var query = XrmService.BuildQuery(type, null, new[] { new ConditionExpression(fieldName, ConditionOperator.NotNull) }, null);
                    var recordsToCache = XrmService.RetrieveFirstX(query, maxCacheCount);
                    _cachedRecords[type].Add(fieldName, new Dictionary<string, List<Entity>>());
                    foreach(var item in recordsToCache)
                    {
                        var cacheMatchString = XrmService.GetFieldAsMatchString(type, fieldName, item.GetFieldValue(fieldName));
                        if (!_cachedRecords[type][fieldName].ContainsKey(cacheMatchString))
                            _cachedRecords[type][fieldName].Add(cacheMatchString, new List<Entity>());
                        _cachedRecords[type][fieldName][cacheMatchString].Add(item);
                    }
                }
                //only use the cache if there were less than maxRecords
                //otherwise there may be dupicates not included
                if (_cachedRecords[type][fieldName].SelectMany(kv => kv.Value).Count() < maxCacheCount
                    && _cachedRecords[type][fieldName].ContainsKey(matchString))
                     return _cachedRecords[type][fieldName][matchString];
            }

            return XrmService.RetrieveAllAndClauses(type, conditions, null);
        }

        public IEnumerable<Entity> GetMatchingEntities(string type, string field, string value, string ignoreCacheFor = null)
        {
            var typeConfig = XrmRecordService.GetTypeConfigs().GetFor(type);
            if (typeConfig == null || typeConfig.ParentLookupType != type || field != XrmService.GetPrimaryNameField(type))
            {
                return GetMatchingEntities(type, new Dictionary<string, object>()
                {
                    { field, value }
                }, ignoreCacheFor: ignoreCacheFor);
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

        public enum MatchOption
        {
            PrimaryKeyOnly,
            PrimaryKeyThenName
        }

        public DataImportResponse DoImport(IEnumerable<Entity> entities, ServiceRequestController controller, bool maskEmails, MatchOption matchOption = MatchOption.PrimaryKeyThenName, IEnumerable<DataImportResponseItem> loadExistingErrorsIntoSummary = null, Dictionary<string, IEnumerable<string>> altMatchKeyDictionary = null, bool updateOnly = false, bool includeOwner = false, bool containsExportedConfigFields = true) 
        {
            var response = new DataImportResponse(entities, loadExistingErrorsIntoSummary);
            controller.AddObjectToUi(response);
            try
            {
                controller.LogLiteral("Preparing Import");

                var ignoreFields = GetIgnoreFields(includeOwner);

                altMatchKeyDictionary = altMatchKeyDictionary ?? new Dictionary<string, IEnumerable<string>>();

                var fieldsToRetry = new Dictionary<Entity, List<string>>();
                var typesToImport = entities.Select(e => e.LogicalName).Distinct();

                var allNNRelationships = XrmService.GetAllNnRelationshipEntityNames();
                var associationTypes = typesToImport.Where(allNNRelationships.Contains).ToArray();

                typesToImport = typesToImport.Where(t => !associationTypes.Contains(t)).ToArray();

                var idSwitches = new Dictionary<string, Dictionary<Guid, Guid>>();
                foreach (var item in typesToImport)
                    idSwitches.Add(item, new Dictionary<Guid, Guid>());

                #region tryordertypes

                var dependencyDictionary = typesToImport
                    .ToDictionary(s => s, s => new List<string>());
                var dependentTo = typesToImport
                    .ToDictionary(s => s, s => new List<string>());

                var toDo = typesToImport.Count();
                var done = 0;
                var fieldsToImport = new Dictionary<string, IEnumerable<string>>();
                foreach (var type in typesToImport)
                {
                    controller.LogLiteral($"Loading Fields For Import {done++}/{toDo}");
                    var thatTypeEntities = entities.Where(e => e.LogicalName == type).ToList();
                    var fields = GetFieldsToImport(thatTypeEntities, type, includeOwner)
                        .Where(f => XrmService.FieldExists(f, type) &&
                            (XrmService.IsLookup(f, type) || XrmService.IsActivityParty(f, type)));
                    fieldsToImport.Add(type, fields.ToArray());
                }

                toDo = typesToImport.Count();
                done = 0;
                foreach (var type in typesToImport)
                {
                    controller.LogLiteral($"Ordering Types For Import {done++}/{toDo}");
                    //iterate through the types and if any of them have a lookup which references this type
                    //then insert this one before it for import first
                    //otherwise just append to the end
                    foreach (var otherType in typesToImport.Where(s => s != type))
                    {
                        var fields = fieldsToImport[otherType];
                        var thatTypeEntities = entities.Where(e => e.LogicalName == otherType).ToList();
                        foreach (var field in fields)
                        {
                            if (thatTypeEntities.Any(e =>
                                (XrmService.IsLookup(field, otherType) && e.GetLookupType(field).Split(',').Contains(type))
                                || (XrmService.IsActivityParty(field, otherType) && e.GetActivityParties(field).Any(p => p.GetLookupType(Fields.activityparty_.partyid) == type))))
                            {
                                dependencyDictionary[type].Add(otherType);
                                dependentTo[otherType].Add(type);
                                break;
                            }
                        }
                    }
                }
                var orderedTypes = new List<string>();
                foreach (var dependency in dependencyDictionary)
                {
                    if (!dependentTo[dependency.Key].Any())
                        orderedTypes.Insert(0, dependency.Key);
                    if (orderedTypes.Contains(dependency.Key))
                        continue;
                    foreach (var otherType in orderedTypes.ToArray())
                    {
                        if(dependency.Value.Contains(otherType))
                        {
                            orderedTypes.Insert(orderedTypes.IndexOf(otherType), dependency.Key);
                            break;
                        }
                    }
                    if (!orderedTypes.Contains(dependency.Key))
                        orderedTypes.Add(dependency.Key);
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
                        if (indexOfFirst > indexOfSecond)
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
                    if (_cachedRecords.ContainsKey(recordType))
                        _cachedRecords.Remove(recordType);
                    try
                    {
                        var thisRecordType = recordType;
                        var thisTypesConfig = XrmRecordService.GetTypeConfigs().GetFor(recordType);
                        var displayPrefix = $"Importing {recordType} Records ({countImported + 1}/{countToImport})";
                        controller.UpdateProgress(countImported++, countToImport, string.Format("Importing {0} Records", recordType));
                        controller.UpdateLevel2Progress(0, 1, "Loading");
                        var primaryField = XrmService.GetPrimaryNameField(recordType);
                        var thisTypeEntities = entities.Where(e => e.LogicalName == recordType).ToList();

                        var orderedEntities = new List<Entity>();

                        #region tryorderentities

                        var importFieldsForEntity = GetFieldsToImport(thisTypeEntities, recordType, includeOwner).ToArray();
                        var fieldsDontExist = GetFieldsInEntities(thisTypeEntities)
                            .Where(f => !f.Contains("."))
                            .Where(f => !XrmService.FieldExists(f, thisRecordType))
                            .Where(f => !ignoreFields.Contains(f))
                            .Distinct()
                            .ToArray();
                        foreach (var field in fieldsDontExist)
                        {
                            response.AddImportError(
                                    new DataImportResponseItem(recordType, field, null, null,
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

                        var thisTypeCreatedDictionary = response.GetImportForType(recordType).GetCreatedEntities();

                        foreach (var entity in orderedEntities)
                        {
                            var thisEntity = entity;
                            try
                            {
                                IEnumerable<Entity> existingMatchingIds = new Entity[0];
                                if (altMatchKeyDictionary.ContainsKey(thisEntity.LogicalName))
                                {
                                    var matchKetFieldDictionary = altMatchKeyDictionary[thisEntity.LogicalName]
                                        .Distinct().ToDictionary(f => f, f => thisEntity.GetField(f));
                                    if(matchKetFieldDictionary.Any(kv => XrmEntity.FieldsEqual(null, kv.Value)))
                                    {
                                        throw new Exception("Match Key Field Is Empty");
                                    }
                                    existingMatchingIds = GetMatchingEntities(thisEntity.LogicalName, matchKetFieldDictionary);

                                }
                                else if (matchOption == MatchOption.PrimaryKeyThenName || thisTypesConfig != null)
                                {
                                    existingMatchingIds = GetMatchForExistingRecord(thisEntity, containsExportedConfigFields, thisTypeCreatedDictionary);
                                }
                                else if (matchOption == MatchOption.PrimaryKeyOnly && thisEntity.Id != Guid.Empty)
                                {
                                    existingMatchingIds = XrmService.RetrieveAllAndClauses(thisEntity.LogicalName, new[]
                                    {
                                        new ConditionExpression(XrmService.GetPrimaryKeyField(thisEntity.LogicalName), ConditionOperator.Equal, thisEntity.Id)
                                    });
                                }
                                if (!existingMatchingIds.Any() && updateOnly)
                                {
                                    throw new Exception("Updates Only And No Matching Record Found");
                                }
                                if (existingMatchingIds.Any())
                                {
                                    var matchRecord = existingMatchingIds.First();
                                    if (thisEntity.Id != Guid.Empty)
                                        idSwitches[recordType].Add(thisEntity.Id, matchRecord.Id);
                                    thisEntity.Id = matchRecord.Id;
                                    thisEntity.SetField(XrmService.GetPrimaryKeyField(thisEntity.LogicalName), thisEntity.Id);
                                    if (thisTypesConfig != null)
                                    {
                                        if (thisTypesConfig.ParentLookupField != null)
                                            thisEntity.SetField(thisTypesConfig.ParentLookupField, matchRecord.GetField(thisTypesConfig.ParentLookupField));
                                        if (thisTypesConfig.UniqueChildFields != null)
                                        {
                                            foreach (var childField in thisTypesConfig.UniqueChildFields)
                                                thisEntity.SetField(childField, matchRecord.GetField(childField));
                                        }
                                    }
                                }
                                var isUpdate = existingMatchingIds.Any();
                                //parse all but aliased fields
                                foreach (var field in thisEntity.GetFieldsInEntity().Where(f => !f.Contains(".")).ToArray())
                                {
                                    if (importFieldsForEntity.Contains(field)
                                        && XrmService.IsLookup(field, thisEntity.LogicalName)
                                        && thisEntity.GetField(field) != null)
                                    {
                                        ParseLookup(response, fieldsToRetry, thisEntity, field, true, containsExportedConfigFields);
                                    }
                                    else if (importFieldsForEntity.Contains(field)
                                        && XrmService.IsActivityParty(field, thisEntity.LogicalName)
                                        && thisEntity.GetField(field) != null)
                                    {
                                        var parties = thisEntity.GetActivityParties(field);
                                        foreach(var party in parties)
                                        {
                                            if (party.GetField(Fields.activityparty_.partyid) != null)
                                            {
                                                ParseLookup(response, fieldsToRetry, party, Fields.activityparty_.partyid, false, containsExportedConfigFields);
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
                                    var fieldsToSetWhichAreChanged = fieldsToSet.Where(f => !XrmEntity.FieldsEqual(existingRecord.GetField(f), thisEntity.GetField(f))).ToArray();
                                    if (fieldsToSetWhichAreChanged.Any())
                                    {
                                        XrmService.Update(thisEntity, fieldsToSetWhichAreChanged);
                                        response.AddUpdated(thisEntity);
                                    }
                                    else
                                    {
                                        response.AddSkippedNoChange(thisEntity);
                                    }
                                }
                                else
                                {
                                    PopulateRequiredCreateFields(fieldsToRetry, thisEntity, fieldsToSet, containsExportedConfigFields);
                                    CheckThrowValidForCreate(thisEntity, fieldsToSet);
                                    thisEntity.Id = XrmService.Create(thisEntity, fieldsToSet);
                                    response.AddCreated(thisEntity);
                                }
                                if (!isUpdate)
                                {
                                    if (thisEntity.LogicalName == Entities.product)
                                    {
                                        var createdEntity = XrmService.Retrieve(thisEntity.LogicalName, thisEntity.Id);
                                        var thisState = thisEntity.GetOptionSetValue("statecode");
                                        var thisStatus = thisEntity.GetOptionSetValue("statuscode");
                                        var matchState = createdEntity.GetOptionSetValue("statecode");
                                        var matchStatus = createdEntity.GetOptionSetValue("statuscode");
                                        if ((thisState != -1 && thisState != matchState)
                                            || (thisStatus != -1 && thisState != matchStatus))
                                        {
                                            SetState(thisEntity);
                                        }
                                    }
                                    else if (thisEntity.GetOptionSetValue("statecode") > 0)
                                    {
                                        SetState(thisEntity);
                                    }
                                }
                                else if (isUpdate && existingMatchingIds.Any())
                                {
                                    var matchRecord = existingMatchingIds.First();
                                    var thisState = thisEntity.GetOptionSetValue("statecode");
                                    var thisStatus = thisEntity.GetOptionSetValue("statuscode");
                                    var matchState = matchRecord.GetOptionSetValue("statecode");
                                    var matchStatus = matchRecord.GetOptionSetValue("statuscode");
                                    if ((thisState != -1 && thisState != matchState)
                                        || (thisStatus != -1 && thisState != matchStatus))
                                    {
                                        SetState(thisEntity);
                                        response.AddUpdated(thisEntity);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                if (fieldsToRetry.ContainsKey(thisEntity))
                                {
                                    fieldsToRetry.Remove(thisEntity);
                                    response.RemoveFieldForRetry(thisEntity);
                                }
                                var field = altMatchKeyDictionary.ContainsKey(thisEntity.LogicalName)
                                    ? string.Join("|", altMatchKeyDictionary[thisEntity.LogicalName])
                                    : null;
                                var value = altMatchKeyDictionary.ContainsKey(thisEntity.LogicalName)
                                    ? string.Join("|", altMatchKeyDictionary[thisEntity.LogicalName].Select(k => XrmService.GetFieldAsDisplayString(entity.LogicalName, k, entity.GetField(k))))
                                    : null;
                                var rowNumber = entity.Contains("Sheet.RowNumber")
                                    ? entity.GetInt("Sheet.RowNumber")
                                    : (int?)null;
                                response.AddImportError(entity, 
                                    new DataImportResponseItem(recordType, field, entity.GetStringField(primaryField), value,
                                        ex.Message + (entity.Id != Guid.Empty ? " Id=" + entity.Id : ""),
                                        ex, rowNumber: rowNumber));
                            }
                            countRecordsImported++;
                            controller.UpdateLevel2Progress(countRecordsImported, countRecordsToImport, estimator.GetProgressString(countRecordsImported));
                        }
                    }
                    catch (Exception ex)
                    {
                        response.AddImportError(
                            new DataImportResponseItem(recordType, null, null, null, string.Format("Error Importing Type {0}", recordType), ex));
                    }
                    if (_cachedRecords.ContainsKey(recordType))
                        _cachedRecords.Remove(recordType);
                }

                controller.TurnOffLevel2();
                countToImport = fieldsToRetry.Count;
                countImported = 0;
                controller.UpdateProgress(countImported, countToImport, "Retrying Unresolved Fields");
                estimator = new TaskEstimator(countToImport);
                foreach (var item in fieldsToRetry)
                {
                    var thisEntity = item.Key;
                    var thisPrimaryField = XrmService.GetPrimaryNameField(thisEntity.LogicalName);
                    try
                    {
                        foreach (var field in item.Value)
                        {
                            if (XrmService.IsLookup(field, thisEntity.LogicalName) && thisEntity.GetField(field) != null)
                            {
                                response.RemoveFieldForRetry(thisEntity, field);
                                var fieldResolved = false;
                                var thisLookupName = thisEntity.GetLookupName(field);
                                try
                                {
                                    var targetTypesToTry = GetTargetTypesToTry(thisEntity, field);
                                    var idNullable = thisEntity.GetLookupGuid(field);
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
                                            var matchRecords = thisLookupName.IsNullOrWhiteSpace() ?
                                                new Entity[0] :
                                                GetMatchingEntities(lookupEntity,
                                                targetPrimaryField,
                                                thisLookupName);
                                            if (matchRecords.Count() == 1)
                                            {
                                                thisEntity.SetLookupField(field, matchRecords.First());
                                                ((EntityReference)(thisEntity.GetField(field))).Name = thisLookupName;
                                                fieldResolved = true;
                                            }
                                            else if (matchRecords.Count() > 0)
                                            {
                                                throw new Exception($"Multiple {lookupEntity} Records Matched The Name");
                                            }
                                        }
                                        if (fieldResolved)
                                            break;
                                    }
                                    if (!fieldResolved)
                                    {
                                        throw new Exception($"Could Not Find Record With Name");
                                    }
                                    else
                                    {
                                        XrmService.Update(thisEntity, new[] { field });
                                        response.AddUpdated(thisEntity);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    var keyValue = altMatchKeyDictionary.ContainsKey(thisEntity.LogicalName)
                                        ? string.Join("|", altMatchKeyDictionary[thisEntity.LogicalName].Select(k => XrmService.GetFieldAsDisplayString(thisEntity.LogicalName, k, thisEntity.GetField(k))))
                                        : null;
                                    var rowNumber = thisEntity.Contains("Sheet.RowNumber")
                                        ? thisEntity.GetInt("Sheet.RowNumber")
                                        : (int?)null;
                                    if (thisEntity.Contains(field))
                                        thisEntity.Attributes.Remove(field);
                                    response.AddImportError(thisEntity, 
                                         new DataImportResponseItem(thisEntity.LogicalName,
                                         field,
                                         thisEntity.GetStringField(thisPrimaryField) ?? keyValue, thisLookupName,
                                            "Error Setting Lookup Field - " + ex.Message, ex, rowNumber: rowNumber));
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        response.AddImportError(thisEntity, 
                            new DataImportResponseItem(thisEntity.LogicalName, null, thisEntity.GetStringField(thisPrimaryField), null,
                                string.Format("Error Importing Record Id={0}", thisEntity.Id),
                                ex));
                    }
                    countImported++;
                    controller.UpdateProgress(countImported, countToImport, estimator.GetProgressString(countImported, taskName: $"Retrying Unresolved Fields"));
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
                            var newAssociation = XrmService.AssociateSafe(relationship.SchemaName, type1, field1, id1, type2, field2, new[] { id2 });
                            if (newAssociation)
                                response.AddCreated(thisEntity);
                            else
                                response.AddSkippedNoChange(thisEntity);
                        }
                        catch (Exception ex)
                        {
                            var rowNumber = thisEntity.Contains("Sheet.RowNumber")
                                ? thisEntity.GetInt("Sheet.RowNumber")
                                : (int?)null;
                            response.AddImportError(thisEntity, 
                            new DataImportResponseItem(
                                    string.Format("Error Associating Record Of Type {0} Id {1}", thisEntity.LogicalName,
                                        thisEntity.Id),
                                    ex, rowNumber: rowNumber));
                        }
                        countRecordsImported++;
                        controller.UpdateLevel2Progress(countRecordsImported, countRecordsToImport, estimator.GetProgressString(countRecordsImported));
                    }
                }
            }
            catch(Exception ex)
            {
                throw;
            }
            finally
            {
                controller.RemoveObjectFromUi(response);
            }
            return response;
        }

        private void ParseLookup(DataImportResponse response, Dictionary<Entity, List<string>> fieldsToRetry, Entity thisEntity, string field, bool allowAddForRetry, bool containsExportedConfigFields)
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
                    var idMatches = GetMatchingEntities(lookupEntity,
                            new Dictionary<string, object>
                            {
                                { targetPrimaryKey, idNullable.Value }
                            }, ignoreCacheFor: thisEntity.LogicalName);

                    if (idMatches.Any())
                    {
                        ((EntityReference)(thisEntity.GetField(field))).Name = idMatches.First().GetStringField(targetPrimaryField);
                        fieldResolved = true;
                    }
                    else
                    {
                        var typeConfigParentOrUniqueFields = new List<string>();
                        var typeConfig = XrmRecordService.GetTypeConfigs().GetFor(thisEntity.LogicalName);
                        if (typeConfig != null)
                        {
                            if (typeConfig.ParentLookupField != null)
                                typeConfigParentOrUniqueFields.Add(typeConfig.ParentLookupField);
                            if(typeConfig.UniqueChildFields != null)
                                typeConfigParentOrUniqueFields.AddRange(typeConfig.UniqueChildFields);
                        }
                        if (containsExportedConfigFields && typeConfigParentOrUniqueFields.Contains(field))
                        {
                            //if the field is part of type config unique fields
                            //then we need to match the target based on the type config rather than just the name
                            //additionally if a lookup field in the config doesnt resolve then we should throw an error
                            var targetType = thisEntity.GetLookupType(field);
                            var targetName = thisEntity.GetLookupName(field);
                            var targetTypeConfig = XrmRecordService.GetTypeConfigs().GetFor(targetType);
                            var primaryField = XrmService.GetPrimaryNameField(targetType);
                            var matchQuery = XrmService.BuildQuery(targetType, null, new[]
                            {
                                new ConditionExpression(primaryField, ConditionOperator.Equal, targetName)
                            }, null);
                            var targetTypeParentOrUniqueFields = new List<string>();
                            if (targetTypeConfig != null)
                            {
                                if (targetTypeConfig.ParentLookupField != null)
                                    targetTypeParentOrUniqueFields.Add(targetTypeConfig.ParentLookupField);
                                if (targetTypeConfig.UniqueChildFields != null)
                                    targetTypeParentOrUniqueFields.AddRange(targetTypeConfig.UniqueChildFields);
                            }
                            if (targetTypeParentOrUniqueFields.Any())
                            {
                                AddUniqueFieldConfigJoins(thisEntity, matchQuery, targetTypeParentOrUniqueFields, containsExportedConfigFields, prefixFieldInEntity: field + ".");
                            }
                            var matches = XrmService.RetrieveAll(matchQuery);
                            if (matches.Count() != 1)
                            {
                                throw new Exception($"Could Not Find Matching Target Record For The Field {field} Named '{targetName}'. This Field Is Configured As Required To Match In The Target Instance When Populated");
                            }
                            thisEntity.SetLookupField(field, matches.First());
                            fieldResolved = true;
                        }
                        else
                        {
                            var matchRecords = name.IsNullOrWhiteSpace() ?
                                new Entity[0] :
                                GetMatchingEntities(lookupEntity,
                                targetPrimaryField,
                                name, ignoreCacheFor: thisEntity.LogicalName);
                            if (matchRecords.Count() == 1)
                            {
                                thisEntity.SetLookupField(field, matchRecords.First());
                                ((EntityReference)(thisEntity.GetField(field))).Name = name;
                                fieldResolved = true;
                            }
                        }
                    }
                }
                if (!fieldResolved)
                {
                    if (!allowAddForRetry)
                        throw new Exception($"Could Not Resolve {field} {name}");
                    if (!fieldsToRetry.ContainsKey(thisEntity))
                        fieldsToRetry.Add(thisEntity, new List<string>());
                    fieldsToRetry[thisEntity].Add(field);
                    response.AddFieldForRetry(thisEntity, field);
                }
            }
        }

        private void SetState(Entity thisEntity)
        {
            var theState = thisEntity.GetOptionSetValue("statecode");
            var theStatus = thisEntity.GetOptionSetValue("statuscode");
            if (thisEntity.LogicalName == Entities.incident && theState == OptionSets.Case.Status.Resolved)
            {
                var closeIt = new Entity(Entities.incidentresolution);
                closeIt.SetLookupField(Fields.incidentresolution_.incidentid, thisEntity);
                closeIt.SetField(Fields.incidentresolution_.subject, "Close By Data Import");
                var req = new CloseIncidentRequest
                {
                    IncidentResolution = closeIt,
                    Status = new OptionSetValue(theStatus)
                };
                XrmService.Execute(req);
            }
            else
                XrmService.SetState(thisEntity, thisEntity.GetOptionSetValue("statecode"), thisEntity.GetOptionSetValue("statuscode"));
        }

        private void PopulateRequiredCreateFields(Dictionary<Entity, List<string>> fieldsToRetry, Entity thisEntity, List<string> fieldsToSet, bool containsExportedConfigFields)
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
                var unitGroupName = thisEntity.GetLookupName(Fields.uom_.uomscheduleid);
                if (string.IsNullOrWhiteSpace(unitGroupName))
                    throw new NullReferenceException($"Error The {XrmService.GetFieldLabel(Fields.uom_.uomscheduleid, Entities.uom)} Name Is Not Populated");
                fieldsToSet.Add(Fields.uom_.uomscheduleid);

                var baseUnitName = thisEntity.GetLookupName(Fields.uom_.baseuom);
                var baseUnitMatchQuery = XrmService.BuildQuery(Entities.uom, null, null, null);
                if(containsExportedConfigFields)
                {
                    var configUniqueFields = XrmRecordService.GetTypeConfigs().GetFor(Entities.uom).UniqueChildFields;
                    AddUniqueFieldConfigJoins(thisEntity, baseUnitMatchQuery, configUniqueFields, true, prefixFieldInEntity: $"{Fields.uom_.baseuom}.");
                }
                else
                {
                    if (baseUnitName == null)
                        throw new NullReferenceException("{Fields.uom_.baseuom} name is required");
                    baseUnitMatchQuery.Criteria.AddCondition(new ConditionExpression(Fields.uom_.name, ConditionOperator.Equal, baseUnitName));
                    var unitGroupLink = baseUnitMatchQuery.AddLink(Entities.uomschedule, Fields.uom_.uomscheduleid, Fields.uomschedule_.uomscheduleid);
                    unitGroupLink.LinkCriteria.AddCondition(new ConditionExpression(Fields.uomschedule_.name, ConditionOperator.Equal, unitGroupName));
                }
                var baseUnitMatches = XrmService.RetrieveAll(baseUnitMatchQuery);
                if (baseUnitMatches.Count() == 0)
                    throw new Exception($"Could Not Identify The {XrmService.GetFieldLabel(Fields.uom_.baseuom, Entities.uom)} {baseUnitName}. No Match Found For The {XrmService.GetFieldLabel(Fields.uom_.uomscheduleid, Entities.uom)}");
                if (baseUnitMatches.Count() > 1)
                    throw new Exception($"Could Not Identify The {XrmService.GetFieldLabel(Fields.uom_.baseuom, Entities.uom)} {baseUnitName}. Multiple Matches Found For The {XrmService.GetFieldLabel(Fields.uom_.uomscheduleid, Entities.uom)}");
                thisEntity.SetLookupField(Fields.uom_.baseuom, baseUnitMatches.First());
                thisEntity.SetField(Fields.uom_.uomscheduleid, baseUnitMatches.First().GetField(Fields.uom_.uomscheduleid));
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

        private IEnumerable<Entity> GetMatchForExistingRecord(Entity thisEntity, bool containsExportedConfigFields, IDictionary<Guid, Entity> thisTypeCreatedDictionary)
        {
            //okay this matching is somewhat complicated due to implementing
            //type configs
            //this is where for exmaple portal code cannot only match by name, but has to mtach based on
            //a combination of fields including those in linked records
            //for example entity form metadata must match based on field name, metadatatype, form and website
            //and the website field i part of the parent record
            //this is where type configs come in
            //if there are type config all the necessary fields for itself and its linked records are included in
            //exports and used to match records in the target system
            var thisTypesConfig = XrmRecordService.GetTypeConfigs().GetFor(thisEntity.LogicalName);
            if (thisTypesConfig == null)
            {
                //if there is no config this is simple we just match by id or name

                //first check for id match
                var existingMatches = thisEntity.Id == Guid.Empty
                                ? new Entity[0]
                                : GetMatchingEntities(thisEntity.LogicalName, new Dictionary<string, object>
                                {
                                    { XrmService.GetPrimaryKeyField(thisEntity.LogicalName), thisEntity.Id }
                                });
                if (!existingMatches.Any())
                {
                    //if no id match then check by the name (unless override the name match field)
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
                        foreach (var item in existingMatches.ToArray())
                        {
                            //if creating multiple items with the same name during an import
                            //then items should not match to those created during this import
                            if (thisTypeCreatedDictionary.ContainsKey(item.Id))
                            {
                                existingMatches = existingMatches.Except(new[] { item }).ToArray();
                            }
                        }
                        if (existingMatches.Count() > 1)
                            throw new Exception(string.Format("More Than One Record Match To The {0} Of {1} When Matching The Name",
                                "Name", name));
                    }
                }
                return existingMatches;
            }
            else
            {
                //okay so this type has a type config so we need to use that to match in the target instance
                //todo think these 3 branches are virtually the same
                if (thisTypesConfig.ParentLookupField == null)
                {
                    //just unique fields (no parent defined)
                    if (thisTypesConfig.UniqueChildFields == null || !thisTypesConfig.UniqueChildFields.Any())
                    {
                        throw new NullReferenceException($"There is a type config for type {thisTypesConfig.Type} but it does not have {nameof(TypeConfigs.Config.ParentLookupField)} or {nameof(TypeConfigs.Config.UniqueChildFields)} configured");
                    }
                    //okay so this queries for matches on all the unique fields in the target
                    //not if a referenced record in the unique fields has a type config
                    //thiose are also joined in the AddUniqueFieldConfigJoins method
                    var matchQuery = XrmService.BuildQuery(thisTypesConfig.Type, null, null, null);
                    var uniqueFields = thisTypesConfig.UniqueChildFields;
                    AddUniqueFieldConfigJoins(thisEntity, matchQuery, uniqueFields, containsExportedConfigFields);
                    var matches = XrmService.RetrieveAll(matchQuery);
                    if (matches.Count() > 1)
                        throw new Exception(string.Format("More Than One Match For the {0} Record Of {1}",
                            thisTypesConfig.Type, thisEntity.GetField(XrmService.GetPrimaryNameField(thisTypesConfig.Type))));
                    return matches;
                }
                else
                {
                    //okay so this queries for matches on all the unique fields in the target
                    //not if a referenced record in the unique fields has a type config
                    //thiose are also joined in the AddUniqueFieldConfigJoins method
                    var primaryField = XrmService.GetPrimaryNameField(thisTypesConfig.Type);
                    var isParent = thisEntity.GetLookupGuid(thisTypesConfig.ParentLookupField) == null;
                    if (isParent)
                    {
                        var matchQuery = XrmService.BuildQuery(thisEntity.LogicalName, null, null, null);
                        matchQuery.Criteria.AddCondition(new ConditionExpression(thisTypesConfig.ParentLookupField, ConditionOperator.Null));
                        var matchFields = thisTypesConfig.UniqueChildFields ?? new[] { XrmService.GetPrimaryNameField(thisEntity.LogicalName) };
                        foreach (var field in matchFields)
                        {
                            var theValue = thisEntity.GetFieldValue(field);
                            if (theValue == null)
                                matchQuery.Criteria.AddCondition(new ConditionExpression(field, ConditionOperator.Null));
                            else if (theValue is EntityReference)
                            {
                                var refName = XrmEntity.GetLookupName(theValue);
                                var type = XrmEntity.GetLookupType(theValue);
                                var linkToReferenced = matchQuery.AddLink(type, field, XrmService.GetPrimaryKeyField(type));
                                if (refName == null)
                                    linkToReferenced.LinkCriteria.AddCondition(XrmService.GetPrimaryNameField(type), ConditionOperator.Null);
                                else
                                    linkToReferenced.LinkCriteria.AddCondition(XrmService.GetPrimaryNameField(type), ConditionOperator.Equal, refName);
                            }
                            else
                                matchQuery.Criteria.AddCondition(new ConditionExpression(field, ConditionOperator.Equal, XrmService.ConvertToQueryValue(field, thisEntity.LogicalName, theValue)));
                        }
                        var matchesForThisAsRoot = XrmService.RetrieveAll(matchQuery);
                        var name = thisEntity.GetStringField(primaryField);
                        if (matchesForThisAsRoot.Count() > 1)
                        {
                            throw new Exception($"Multiple Matches Were Found For The Parent Record With Name '{name}'. {matchFields.JoinGrammarAnd()} Must Be Unique On These Records To Identify The Matching Record");
                        }
                        return matchesForThisAsRoot;
                    }
                    else
                    {
                        //okay so this queries for matches on the parent unique fields in the target
                        //not if a referenced record in the unique fields has a type config
                        //thiose are also joined in the AddUniqueFieldConfigJoins method
                        var parentName = thisEntity.GetLookupName(thisTypesConfig.ParentLookupField);
                        if (parentName == null)
                            throw new Exception(string.Format("Cannot identify parent record for {0} Of {1} because the parent reference name is empty",
                                 XrmService.GetPrimaryNameField(thisTypesConfig.Type), thisEntity.GetStringField(primaryField)));
                        var parentPrimaryNameField = XrmService.GetPrimaryNameField(thisTypesConfig.ParentLookupType);

                        var matchingChildQuery = XrmService.BuildQuery(thisTypesConfig.Type, null, null, null);
                        var parentAndUniqueFieldsToMatch = new List<string>();
                        parentAndUniqueFieldsToMatch.Add(thisTypesConfig.ParentLookupField);
                        if (thisTypesConfig.UniqueChildFields != null)
                            parentAndUniqueFieldsToMatch.AddRange(thisTypesConfig.UniqueChildFields);

                        foreach (var field in parentAndUniqueFieldsToMatch)
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
                        var matchingChildren = XrmService.RetrieveAll(matchingChildQuery);
                        if (matchingChildren.Count() > 1)
                            throw new Exception(string.Format("More Than One Match Found For the Child {0} Record Of {1} {2}",
                                thisTypesConfig.Type, parentName, parentPrimaryNameField));
                        if (matchingChildren.Count() == 0 && thisTypesConfig.BlockCreateChild)
                            throw new Exception(string.Format("Creation Prevented For Child {0} Record Of {1} {2}. These Are Expected To Be Created By The System",
                                thisTypesConfig.Type, parentName, parentPrimaryNameField));

                        return matchingChildren;
                    }
                }
            }
        }

        private void AddUniqueFieldConfigJoins(Entity thisEntity, QueryExpression matchQuery, IEnumerable<string> uniqueFields, bool containsExportedConfigFields, string prefixFieldInEntity = null)
        {
            foreach (var field in uniqueFields)
            {
                var theValue = thisEntity.GetFieldValue(prefixFieldInEntity + field);
                if (theValue == null)
                    matchQuery.Criteria.AddCondition(new ConditionExpression(field, ConditionOperator.Null));
                else if (theValue is EntityReference)
                {
                    var name = XrmEntity.GetLookupName(theValue);
                    var type = XrmEntity.GetLookupType(theValue);
                    var linkToReferenced = matchQuery.AddLink(type, field, XrmService.GetPrimaryKeyField(type));
                    if (name == null)
                        linkToReferenced.LinkCriteria.AddCondition(XrmService.GetPrimaryNameField(type), ConditionOperator.Null);
                    else
                    {
                        linkToReferenced.LinkCriteria.AddCondition(XrmService.GetPrimaryNameField(type), ConditionOperator.Equal, name);
                        if (containsExportedConfigFields)
                            AddReferenceConfigJoins(linkToReferenced, thisEntity, field);
                    }
                }
                else
                    matchQuery.Criteria.AddCondition(new ConditionExpression(field, ConditionOperator.Equal, XrmService.ConvertToQueryValue(field, matchQuery.EntityName, theValue)));
            }
        }

        private void AddReferenceConfigJoins(LinkEntity linkToReferenced, Entity thisEntity, string field)
        {
            var referencedType = XrmEntity.GetLookupType(thisEntity.GetFieldValue(field));
            var referencedTypeConfig = XrmRecordService.GetTypeConfigs().GetFor(referencedType);
            if (referencedTypeConfig != null && referencedTypeConfig.UniqueChildFields != null)
            {
                foreach(var uniqueField in referencedTypeConfig.UniqueChildFields)
                {
                    var theValue = thisEntity.GetFieldValue($"{field}.{uniqueField}");
                    if (theValue == null)
                        linkToReferenced.LinkCriteria.AddCondition(new ConditionExpression(uniqueField, ConditionOperator.Null));
                    else if (theValue is EntityReference)
                    {
                        var name = XrmEntity.GetLookupName(theValue);
                        var type = XrmEntity.GetLookupType(theValue);
                        var nextLinkToReferenced = linkToReferenced.AddLink(type, uniqueField, XrmService.GetPrimaryKeyField(type));
                        if (name == null)
                            nextLinkToReferenced.LinkCriteria.AddCondition(XrmService.GetPrimaryNameField(type), ConditionOperator.Null);
                        else
                        {
                            nextLinkToReferenced.LinkCriteria.AddCondition(XrmService.GetPrimaryNameField(type), ConditionOperator.Equal, name);
                            AddReferenceConfigJoins(nextLinkToReferenced, thisEntity, $"{field}.{uniqueField}" );
                        }
                    }
                    else
                        linkToReferenced.LinkCriteria.AddCondition(new ConditionExpression(uniqueField, ConditionOperator.Equal, XrmService.ConvertToQueryValue(uniqueField, referencedType, theValue)));
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

        private IEnumerable<string> GetFieldsToImport(IEnumerable<Entity> thisTypeEntities, string type, bool includeOwner)
        {
            var fields = GetFieldsInEntities(thisTypeEntities)
                .Where(f => IsIncludeField(f, type, includeOwner: includeOwner))
                .Distinct()
                .ToList();
            return fields;
        }

        public bool IsIncludeField(string fieldName, string entityType, bool includeOwner = false)
        {
            var hardcodeInvalidFields = GetIgnoreFields(includeOwner);
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
            if (fieldName == "transactioncurrencyid")
                return true;
            if (fieldName == Fields.product_.productstructure)
                return true;
            return
                XrmRecordService.FieldExists(fieldName, entityType) && XrmRecordService.GetFieldMetadata(fieldName, entityType).Writeable;

        }

        private static IEnumerable<string> GetIgnoreFields(bool includeOwner)
        {
            var fields = new[]
            {
                    "yomifullname", "administratorid", "owneridtype", "timezoneruleversionnumber", "utcconversiontimezonecode", "organizationid", "owninguser", "owningbusinessunit","owningteam",
                    "overriddencreatedon", "statuscode", "statecode", "createdby", "createdon", "modifiedby", "modifiedon", "modifiedon", "jmcg_currentnumberposition", "calendarrules", "parentarticlecontentid", "rootarticleid", "previousarticlecontentid"
                    , "address1_addressid", "address2_addressid", "processid", Fields.incident_.slaid, Fields.incident_.firstresponsebykpiid, Fields.incident_.resolvebykpiid, "entityimage_url", "entityimage_timestamp", "safedescription"
            };
            if (!includeOwner)
                fields = fields.Union(new[] { "ownerid" }).ToArray();
            return fields;
        }
    }
}
