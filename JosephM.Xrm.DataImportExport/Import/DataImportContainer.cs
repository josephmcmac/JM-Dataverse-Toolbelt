using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace JosephM.Xrm.DataImportExport.Import
{
    public class DataImportContainer
    {
        private readonly Dictionary<Entity, List<string>> _fieldsToRetry = new Dictionary<Entity, List<string>>();
        public DataImportContainer(DataImportResponse response, XrmRecordService xrmRecordService, Dictionary<string, IEnumerable<KeyValuePair<string, bool>>> altMatchKeyDictionary, Dictionary<string, Dictionary<string, KeyValuePair<string, string>>> altLookupMatchKeyDictionary, IEnumerable<Entity> entities, ServiceRequestController controller, bool includeOwner, bool includeOverrideCreatedOn,bool maskEmails, MatchOption matchOption, bool updateOnly, bool containsExportedConfigFields, int executeMultipleSetSize, int targetCacheLimit, bool onlyFieldMatchActive, bool forceSubmitAllFields, bool displayTimeEstimations, int parallelImportProcessCount, bool bypassWorkflowsAndPlugins = false)
        {
            Response = response;
            XrmRecordService = xrmRecordService;
            AltMatchKeyDictionary = altMatchKeyDictionary;
            AltLookupMatchKeyDictionary = altLookupMatchKeyDictionary;
            Controller = controller;
            IncludeOwner = includeOwner;
            IncludeOverrideCreatedOn = includeOverrideCreatedOn;
            MaskEmails = maskEmails;
            OnlyFieldMatchActive = onlyFieldMatchActive;
            ForceSubmitAllFields = forceSubmitAllFields;
            DisplayTimeEstimations = displayTimeEstimations;
            MatchOption = matchOption;
            UpdateOnly = updateOnly;
            ContainsExportedConfigFields = containsExportedConfigFields;
            ExecuteMultipleSetSize = executeMultipleSetSize;
            ParallelImportProcessCount = parallelImportProcessCount;
            BypassFlowsPluginsAndWorkflows = bypassWorkflowsAndPlugins;
            _maxCacheCount = targetCacheLimit;
            EntitiesToImport = entities;
            var typesToImport = entities.Select(e => e.LogicalName).Distinct();

            var allNNRelationships = XrmService.GetAllNnRelationshipEntityNames();

            AssociationTypesToImport = typesToImport.Where(allNNRelationships.Contains).ToArray();
            EntityTypesToImport = typesToImport.Where(t => !AssociationTypesToImport.Contains(t)).ToArray();

            IdSwitches = new Dictionary<string, Dictionary<Guid, Guid>>();
            foreach (var item in typesToImport)
                IdSwitches.Add(item, new Dictionary<Guid, Guid>());
        }

        public DataImportResponse Response { get; }
        public XrmRecordService XrmRecordService { get; }
        public XrmService XrmService { get { return XrmRecordService.XrmService; } }
        public Dictionary<string, IEnumerable<KeyValuePair<string, bool>>> AltMatchKeyDictionary { get; }
        public Dictionary<string, Dictionary<string, KeyValuePair<string, string>>> AltLookupMatchKeyDictionary { get; }
        public ServiceRequestController Controller { get; }
        public bool IncludeOwner { get; }
        public bool IncludeOverrideCreatedOn { get; }
        public bool MaskEmails { get; }
        public bool OnlyFieldMatchActive { get; }
        public bool ForceSubmitAllFields { get; }
        public bool DisplayTimeEstimations { get; }
        public MatchOption MatchOption { get; }
        public bool UpdateOnly { get; }
        public bool ContainsExportedConfigFields { get; }
        public int ExecuteMultipleSetSize { get; }
        public int ParallelImportProcessCount { get; private set; }
        public bool BypassFlowsPluginsAndWorkflows { get; private set; }

        public IDictionary<Entity, List<string>> FieldsToRetry {  get { return _fieldsToRetry; } }
        public IEnumerable<string> AssociationTypesToImport { get; }

        public IEnumerable<Entity> EntitiesToImport { get; }
        public IEnumerable<string> EntityTypesToImport { get; }
        public Dictionary<string, Dictionary<Guid, Guid>> IdSwitches { get; }

        public void LogEntityError(Entity entity, Exception ex)
        {
            if (_fieldsToRetry.ContainsKey(entity))
            {
                _fieldsToRetry.Remove(entity);
                Response.RemoveFieldForRetry(entity);
            }
            var field = AltMatchKeyDictionary.ContainsKey(entity.LogicalName)
                ? string.Join("|", AltMatchKeyDictionary[entity.LogicalName])
                : null;
            var value = AltMatchKeyDictionary.ContainsKey(entity.LogicalName)
                ? string.Join("|", AltMatchKeyDictionary[entity.LogicalName].Select(k => XrmService.GetFieldAsDisplayString(entity.LogicalName, k.Key, entity.GetField(k.Key), XrmRecordService.LocalisationService.XrmLocalisationService)))
                : null;
            var rowNumber = entity.Contains("Sheet.RowNumber")
                ? entity.GetInt("Sheet.RowNumber")
                : (int?)null;
            var primaryField = XrmService.GetPrimaryNameField(entity.LogicalName);
            Response.AddImportError(entity,
                new DataImportResponseItem(entity.LogicalName, field, entity.GetStringField(primaryField), value,
                    ex.Message + (entity.Id != Guid.Empty ? " Id=" + entity.Id : ""),
                    ex, rowNumber: rowNumber));
        }

        public void LogAssociationError(Entity associationEntity, Exception ex)
        {
            var rowNumber = associationEntity.Contains("Sheet.RowNumber")
                ? associationEntity.GetInt("Sheet.RowNumber")
                : (int?)null;
            Response.AddImportError(associationEntity,
            new DataImportResponseItem(
                    string.Format("Error Associating Record Of Type {0} Id {1}", associationEntity.LogicalName,
                        associationEntity.Id),
                    ex, rowNumber: rowNumber));
        }

        public IEnumerable<string> GetFieldsInEntities(IEnumerable<Entity> thisTypeEntities)
        {
            return thisTypeEntities.SelectMany(e => e.GetFieldsInEntity()).Distinct().ToArray();
        }

        public IEnumerable<string> GetFieldsToImport(IEnumerable<Entity> thisTypeEntities, string type)
        {
            var fields = GetFieldsInEntities(thisTypeEntities)
                .Where(f => ForceSubmitAllFields || IsIncludeField(f, type, XrmRecordService, IncludeOwner, IncludeOverrideCreatedOn))
                .Distinct()
                .ToList();
            return fields;
        }

        public static bool IsIncludeField(string fieldName, string entityType, XrmRecordService xrmRecordService, bool includeOwner, bool includeOverrideCreatedOn)
        {
            var hardcodeInvalidFields = GetIgnoreFields(entityType, includeOwner, includeOverrideCreatedOn);
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
            if (fieldName == "salesliteratureid")
                return true;
            if (entityType == Entities.productsubstitute &&
                new [] { Fields.productsubstitute_.productid, Fields.productsubstitute_.substitutedproductid }.Contains(fieldName))
                return true;
            if (fieldName == Fields.product_.productstructure)
                return true;
            if (fieldName == "overriddencreatedon")
                return true;
            return
                xrmRecordService.FieldExists(fieldName, entityType) && xrmRecordService.GetFieldMetadata(fieldName, entityType).Writeable;
        }

        public IEnumerable<string> GetIgnoreFields(string recordType)
        {
            return GetIgnoreFields(recordType, IncludeOwner, IncludeOverrideCreatedOn);
        }

        public static IEnumerable<string> GetIgnoreFields(string recordType, bool includeOwner, bool includeOverrideCreatedOn)
        {
            var fields = new[]
            {
                "yomifullname", "administratorid", "owneridtype", "timezoneruleversionnumber", "utcconversiontimezonecode", "organizationid", "owninguser", "owningbusinessunit","owningteam",
                "createdby", "createdon", "modifiedby", "modifiedon", "modifiedon", "jmcg_currentnumberposition", "calendarrules", "parentarticlecontentid", "rootarticleid", "previousarticlecontentid"
                , "address1_addressid", "address2_addressid", "address3_addressid", "processid", Fields.incident_.slaid, Fields.incident_.firstresponsebykpiid, Fields.incident_.resolvebykpiid, "entityimage_url", "entityimage_timestamp", "safedescription", "attachmentid", "jmcg_clonedfrom"
            };
            if (!includeOverrideCreatedOn)
            {
                fields = fields.Union(new[] { "overriddencreatedon" }).ToArray();
            }
            if (!includeOwner)
            {
                fields = fields.Union(new[] { "ownerid" }).ToArray();
            }
            if (recordType == Entities.activitymimeattachment)
            {
                fields = fields.Union(new[] { "objectid" }).ToArray();
            }
            return fields;
        }

        private Dictionary<string, Dictionary<string, Dictionary<string, List<Entity>>>> _cachedRecords = new Dictionary<string, Dictionary<string, Dictionary<string, List<Entity>>>>();

        public void LoadTargetsToCache(string recordType)
        {
            var recordsReferenced = new HashSet<string>();
            recordsReferenced.Add(recordType);
            var thisTypeEntities = EntitiesToImport.Where(e => e.LogicalName == recordType);
            var fieldsForImport = GetFieldsToImport(thisTypeEntities, recordType);
            foreach (var entity in thisTypeEntities)
            {
                foreach (var field in fieldsForImport)
                {
                    var value = entity.GetField(field);
                    if (value is EntityReference entityReference)
                    {
                        var type = entityReference.LogicalName;
                        if (!string.IsNullOrWhiteSpace(type) && !type.Contains(",") && !recordsReferenced.Contains(type))
                        {
                            recordsReferenced.Add(type);
                        }
                    }
                }
            }

            CheckLoadCache(recordsReferenced);
        }

        private string[] _dontCacheTheseTypes = new[] { Entities.activitymimeattachment, Entities.attachment };
        private int _maxCacheCount = 1000;
        private void CheckLoadCache(IEnumerable<string> recordsReferenced)
        {
            var loadTheseOnes = recordsReferenced.ToList();
            var typeConfigs = XrmRecordService.GetTypeConfigs();
            foreach (var one in loadTheseOnes.ToArray())
            {
                if(_dontCacheTheseTypes.Contains(one))
                {
                    loadTheseOnes.Remove(one);
                }
                if(_cachedRecords.ContainsKey(one))
                {
                    loadTheseOnes.Remove(one);
                }
                else
                {
                    var typeConfig = typeConfigs.GetFor(one);
                    if(typeConfig != null)
                    {
                        loadTheseOnes.Remove(one);
                    }
                }
            }

            var getMultipleResponses = XrmService.ExecuteMultiple(loadTheseOnes.Select(rt =>
            {
                var matchFilters = _matchFilters.ContainsKey(rt)
                    ? _matchFilters[rt]
                    : new ConditionExpression[0];
                var query = new QueryExpression(rt)
                {
                    ColumnSet = new ColumnSet(true),
                    TopCount = _maxCacheCount
                };
                query.Criteria.Conditions.AddRange(matchFilters);
                return new RetrieveMultipleRequest
                {
                    Query = query
                };
            }).ToArray());


            if (getMultipleResponses.Any())
            {
                try
                {
                    var i = 0;
                    foreach (var response in getMultipleResponses)
                    {
                        var type = loadTheseOnes.ElementAt(i);
                        if (response.Fault != null)
                        {
                            Response.AddImportError(new DataImportResponseItem(
                                 $"Error Loading Target Records Cache for {type})", new FaultException<OrganizationServiceFault>(response.Fault, response.Fault.Message)));
                        }
                        else
                        {
                            var records = ((RetrieveMultipleResponse)response.Response).EntityCollection.Entities;
                            if (!_cachedRecords.ContainsKey(type))
                                _cachedRecords.Add(type, new Dictionary<string, Dictionary<string, List<Entity>>>());
                            var primaryKey = XrmService.GetPrimaryKeyField(type);
                            if (!_cachedRecords[type].ContainsKey(primaryKey))
                                _cachedRecords[type].Add(primaryKey, new Dictionary<string, List<Entity>>());

                            foreach (var record in records)
                            {
                                var cacheMatchString = record.Id.ToString();
                                if (!_cachedRecords[type][primaryKey].ContainsKey(cacheMatchString))
                                    _cachedRecords[type][primaryKey].Add(cacheMatchString, new List<Entity>());
                                _cachedRecords[type][primaryKey][cacheMatchString].Add(record);
                            }
                            var primaryNameField = XrmService.GetPrimaryNameField(type);
                            if (primaryNameField != null)
                            {
                                if (!_cachedRecords[type].ContainsKey(primaryNameField))
                                    _cachedRecords[type].Add(primaryNameField, new Dictionary<string, List<Entity>>());
                                foreach (var record in records)
                                {
                                    var cacheMatchString = XrmService.GetFieldAsMatchString(type, primaryNameField, record.GetStringField(primaryNameField));
                                    if (!_cachedRecords[type][primaryNameField].ContainsKey(cacheMatchString))
                                        _cachedRecords[type][primaryNameField].Add(cacheMatchString, new List<Entity>());
                                    _cachedRecords[type][primaryNameField][cacheMatchString].Add(record);
                                }
                            }
                        }
                        i++;
                    }
                }
                catch(Exception ex)
                {
                    Response.AddImportError(new DataImportResponseItem(
                                $"Error Loading Target Records Cache ({string.Join(",", loadTheseOnes)})", ex));
                }
            }
        }

        public IEnumerable<Entity> GetMatchingEntities(string type, IDictionary<string, object> fieldValues, string ignoreCacheFor = null)
        {
            var conditions = fieldValues.Select(fv =>
                fv.Value == null
                ? new ConditionExpression(fv.Key, ConditionOperator.Null)
                : new ConditionExpression(fv.Key, ConditionOperator.Equal, XrmService.ConvertToQueryValue(fv.Key, type, XrmService.ParseField(fv.Key, type, fv.Value)))
            ).ToList();

            var matchFilters = _matchFilters.ContainsKey(type)
                ? _matchFilters[type]
                : new ConditionExpression[0];

            if (type != ignoreCacheFor
                && conditions.Count == 1
                && fieldValues.Values.First() != null)
            {
                CheckLoadCache(new[] { type });
                var fieldName = fieldValues.Keys.First();

                var matchString = XrmService.GetFieldAsMatchString(type, fieldName, fieldValues.Values.First());
                if (!_cachedRecords.ContainsKey(type))
                    _cachedRecords.Add(type, new Dictionary<string, Dictionary<string, List<Entity>>>());
                if (!_cachedRecords[type].ContainsKey(fieldName))
                {
                    var query = XrmService.BuildQuery(type, null, matchFilters, null);
                    var recordsToCache = XrmService.RetrieveFirstX(query, _maxCacheCount);
                    _cachedRecords[type].Add(fieldName, new Dictionary<string, List<Entity>>());
                    foreach (var item in recordsToCache)
                    {
                        var cacheMatchString = XrmService.GetFieldAsMatchString(type, fieldName, item.GetFieldValue(fieldName));
                        if (!_cachedRecords[type][fieldName].ContainsKey(cacheMatchString))
                            _cachedRecords[type][fieldName].Add(cacheMatchString, new List<Entity>());
                        _cachedRecords[type][fieldName][cacheMatchString].Add(item);
                    }
                }
                //only use the cache if there were less than maxRecords
                //otherwise there may be dupicates not included
                if (_cachedRecords[type][fieldName].SelectMany(kv => kv.Value).Count() < _maxCacheCount
                    && _cachedRecords[type][fieldName].ContainsKey(matchString))
                    return _cachedRecords[type][fieldName][matchString];
            }
            return XrmService.RetrieveAllAndConditions(type, conditions.Union(matchFilters).ToArray(), null);
        }

        private IEnumerable<Entity> GetMatchesByNameForRootRecord(TypeConfigs.Config parentChildConfig, string name)
        {
            //okay if this is a parent record (e.g a root web page)
            //then match by name and where the parent reference is empty
            if (name == null)
                throw new NullReferenceException("Name Is Null For Parent Record");
            var matches = XrmService.RetrieveAllAndConditions(parentChildConfig.Type,
                        new[] {
                                new ConditionExpression(parentChildConfig.ParentLookupField, ConditionOperator.Null),
                                new ConditionExpression(XrmService.GetPrimaryNameField(parentChildConfig.Type), ConditionOperator.Equal, name) });
            if (matches.Count() > 1)
                throw new Exception(string.Format("More Than One Record Match To The {0} Of {1}",
                    XrmService.GetPrimaryNameField(parentChildConfig.Type), name));
            return matches;
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

        public Entity GetUniqueMatchingEntity(string type, string field, string value)
        {
            var matchingRecords = GetMatchingEntities(type, field, value);
            if (!matchingRecords.Any())
                throw new NullReferenceException(string.Format("No Record Matched To The {0} Of {1} When Matching The Name",
                        "Name", value));
            if (matchingRecords.Count() > 1)
            {
                var caseMatch = matchingRecords.Where(m => string.CompareOrdinal(value, m.GetStringField(field)) == 0);
                var notCaseMatch = matchingRecords.Where(m => string.CompareOrdinal(value, m.GetStringField(field)) != 0);
                if (caseMatch.Count() == 1 && notCaseMatch.Count() > 0)
                {
                    matchingRecords = caseMatch.ToArray();
                }
                else
                {
                    throw new Exception(string.Format("More Than One Record Match To The {0} Of {1} When Matching The Name",
                        "Name", value));
                }
            }
            return matchingRecords.First();
        }

        private Entity _rootBusinessUnit;
        public Entity GetRootBusinessUnit()
        {
            if (_rootBusinessUnit == null)
            {
                _rootBusinessUnit = XrmService.GetFirst(Entities.businessunit, Fields.businessunit_.parentbusinessunitid, null, new string[0]);
            }
            return _rootBusinessUnit;
        }

        public QueryExpression GetMatchQueryExpression(Entity thisEntity, DataImportContainer dataImportContainer)
        {
            var thisTypesConfig = XrmRecordService.GetTypeConfigs().GetFor(thisEntity.LogicalName);
            if (thisTypesConfig != null)
            {
                var matchQuery = XrmService.BuildQuery(thisTypesConfig.Type, null, null, null);
                var parentAndUniqueFieldsToMatch = new List<string>();
                if (thisTypesConfig.ParentLookupField != null)
                    parentAndUniqueFieldsToMatch.Add(thisTypesConfig.ParentLookupField);
                if (thisTypesConfig.UniqueChildFields != null)
                    parentAndUniqueFieldsToMatch.AddRange(thisTypesConfig.UniqueChildFields);
                if (!parentAndUniqueFieldsToMatch.Any())
                    throw new Exception($"Type {thisTypesConfig.Type} has a type config but neither of {nameof(TypeConfigs.Config.ParentLookupField)} or {nameof(TypeConfigs.Config.UniqueChildFields)} has fields configured for matching");

                AddUniqueFieldConfigJoins(thisEntity, matchQuery, parentAndUniqueFieldsToMatch);
                return matchQuery;
            }
            else
            {
                var primaryKey = XrmService.GetPrimaryKeyField(thisEntity.LogicalName);
                var primaryName = XrmService.GetPrimaryNameField(thisEntity.LogicalName);
                var matchQuery = XrmService.BuildQuery(thisEntity.LogicalName, null, null, null);
                if (AltMatchKeyDictionary.ContainsKey(thisEntity.LogicalName))
                {
                    var matchKeyFieldDictionary = AltMatchKeyDictionary[thisEntity.LogicalName]
                        .Distinct().ToDictionary(f => f.Key, f => thisEntity.GetField(f.Key));

                    foreach(var matchKeyField in matchKeyFieldDictionary)
                    {
                        if (matchKeyField.Value is EntityReference er
                            && er.Id == Guid.Empty
                            && !string.IsNullOrWhiteSpace(er.Name)
                            && !string.IsNullOrWhiteSpace(er.LogicalName)
                            && XrmService.EntityExists(er.LogicalName))
                        {
                            var linkTo = matchQuery.AddLink(er.LogicalName, matchKeyField.Key, XrmService.GetPrimaryKeyField(er.LogicalName));
                            if (dataImportContainer.AltLookupMatchKeyDictionary != null
                                && dataImportContainer.AltLookupMatchKeyDictionary.ContainsKey(thisEntity.LogicalName)
                                && dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.LogicalName].ContainsKey(matchKeyField.Key))
                            {
                                var altMatchType = dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.LogicalName][matchKeyField.Key].Key;
                                var altMatchField = dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.LogicalName][matchKeyField.Key].Value;
                                linkTo.LinkCriteria.AddCondition(new ConditionExpression(altMatchField, ConditionOperator.Equal, XrmService.ConvertToQueryValue(altMatchField, altMatchType,  er.Name)));
                            }
                            else
                            {
                                linkTo.LinkCriteria.AddCondition(new ConditionExpression(XrmService.GetPrimaryNameField(er.LogicalName), ConditionOperator.Equal, er.Name));
                            }
                        }
                        else
                        {
                            matchQuery.Criteria.Conditions.Add(new ConditionExpression(matchKeyField.Key, ConditionOperator.Equal, XrmService.ConvertToQueryValue(matchKeyField.Key, thisEntity.LogicalName, matchKeyField.Value)));
                        }
                    }
                    if(OnlyFieldMatchActive)
                    {
                        matchQuery.Criteria.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
                    }
                }
                else if (MatchOption == MatchOption.PrimaryKeyThenName || thisTypesConfig != null)
                {
                    matchQuery.Criteria.FilterOperator = LogicalOperator.Or;
                    var orFilter = matchQuery.Criteria.AddFilter(LogicalOperator.Or);
                    orFilter.Conditions.Add(
                        new ConditionExpression(primaryKey, ConditionOperator.Equal, thisEntity.Id));
                    if (primaryName != null && thisEntity.GetStringField(primaryName) != null)
                    {
                        orFilter.Conditions.Add(
                            new ConditionExpression(primaryName, ConditionOperator.Equal, thisEntity.GetStringField(primaryName)));
                    }
                    if (OnlyFieldMatchActive)
                    {
                        matchQuery.Criteria.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
                    }
                }
                else if (MatchOption == MatchOption.PrimaryKeyOnly)
                {
                    matchQuery.Criteria.Conditions.Add(
                        new ConditionExpression(primaryKey, ConditionOperator.Equal, thisEntity.Id));
                }
                return matchQuery;
            }
        }

        public QueryExpression GetParseLookupQuery(Entity thisEntity, string field, string targetType, string matchField)
        {
            var referencedValue = thisEntity.GetLookupName(field) ?? "";
            var referencedId = thisEntity.GetLookupGuid(field) ?? Guid.Empty;
            var primaryKey = XrmService.GetPrimaryKeyField(targetType);
            var configs = XrmRecordService.GetTypeConfigs();
            var thisTypeConfig = configs.GetFor(thisEntity.LogicalName);
            var targetTypeConfig = configs.GetFor(targetType);
            if (thisTypeConfig != null && targetTypeConfig != null && ContainsExportedConfigFields)
            {
                var matchQuery = XrmService.BuildQuery(targetType, null, new[]
                {
                    new ConditionExpression(matchField, ConditionOperator.Equal, referencedValue)
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
                    AddUniqueFieldConfigJoins(thisEntity, matchQuery, targetTypeParentOrUniqueFields, prefixFieldInEntity: field + ".");
                }
                var switchRootFilterToOrId = new FilterExpression(LogicalOperator.Or);
                switchRootFilterToOrId.AddCondition(new ConditionExpression(primaryKey, ConditionOperator.Equal, referencedId));
                switchRootFilterToOrId.Filters.Add(matchQuery.Criteria);
                matchQuery.Criteria = switchRootFilterToOrId;
                return matchQuery;
            }
            else
            {
                var matchQuery = XrmService.BuildQuery(targetType, null, null, null);
                matchQuery.Criteria.FilterOperator = LogicalOperator.Or;
                matchQuery.Criteria.Conditions.Add(
                    new ConditionExpression(primaryKey, ConditionOperator.Equal, referencedId));
                if (matchField != null && matchField != primaryKey && referencedValue != null)
                {
                    matchQuery.Criteria.Conditions.Add(
                        new ConditionExpression(matchField, ConditionOperator.Equal, referencedValue));
                }
                return matchQuery;
            }
        }

        public void AddUniqueFieldConfigJoins(Entity thisEntity, QueryExpression matchQuery, IEnumerable<string> uniqueFields, string prefixFieldInEntity = null)
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
                        if (ContainsExportedConfigFields)
                            AddReferenceConfigJoins(linkToReferenced, thisEntity, field);
                    }
                }
                else
                    matchQuery.Criteria.AddCondition(new ConditionExpression(field, ConditionOperator.Equal, XrmService.ConvertToQueryValue(field, matchQuery.EntityName, theValue)));
            }
        }

        public void AddCreated(Entity originalEntity)
        {
            Response.AddCreated(originalEntity);
            var thisRecordType = originalEntity.LogicalName;
            if (_cachedRecords.ContainsKey(thisRecordType))
            {
                foreach(var fieldDictionary in _cachedRecords[thisRecordType])
                {
                    var indexedField = fieldDictionary.Key;
                    var matchString = XrmService.GetFieldAsMatchString(thisRecordType, indexedField, originalEntity.GetFieldValue(indexedField));
                    if (!_cachedRecords[thisRecordType][indexedField].ContainsKey(matchString))
                        _cachedRecords[thisRecordType][indexedField].Add(matchString, new List<Entity>());
                    _cachedRecords[thisRecordType][indexedField][matchString].Add(originalEntity);
                }
            }
        }

        private void AddReferenceConfigJoins(LinkEntity linkToReferenced, Entity thisEntity, string field)
        {
            var referencedType = XrmEntity.GetLookupType(thisEntity.GetFieldValue(field));
            var referencedTypeConfig = XrmRecordService.GetTypeConfigs().GetFor(referencedType);
            if (referencedTypeConfig != null && referencedTypeConfig.UniqueChildFields != null)
            {
                foreach (var uniqueField in referencedTypeConfig.UniqueChildFields)
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
                            AddReferenceConfigJoins(nextLinkToReferenced, thisEntity, $"{field}.{uniqueField}");
                        }
                    }
                    else
                        linkToReferenced.LinkCriteria.AddCondition(new ConditionExpression(uniqueField, ConditionOperator.Equal, XrmService.ConvertToQueryValue(uniqueField, referencedType, theValue)));
                }
            }
        }

        private Dictionary<string, IEnumerable<ConditionExpression>> _matchFilters = new Dictionary<string, IEnumerable<ConditionExpression>>
        {
            { Entities.workflow, new [] { new ConditionExpression(Fields.workflow_.type, ConditionOperator.Equal, XrmPicklists.WorkflowType.Definition)}}
        };

        private Dictionary<string, IEnumerable<ConditionExpression>> _matchNameFilters = new Dictionary<string, IEnumerable<ConditionExpression>>
        {
            { Entities.contact, new [] { new ConditionExpression(Fields.contact_.merged, ConditionOperator.NotEqual, true)}},
            { Entities.account, new [] { new ConditionExpression(Fields.account_.merged, ConditionOperator.NotEqual, true)}},
            { Entities.knowledgearticle, new [] { new ConditionExpression(Fields.knowledgearticle_.isrootarticle, ConditionOperator.NotEqual, true) }}
        };

        public bool IsValidForCache(string recordType)
        {
            var primaryKey = XrmService.GetPrimaryKeyField(recordType);
            return
                _cachedRecords.ContainsKey(recordType)
                && _cachedRecords[recordType].ContainsKey(primaryKey)
                && _cachedRecords[recordType][primaryKey].SelectMany(kv => kv.Value).Count() < _maxCacheCount;
        }

        public IEnumerable<Entity> FilterForNameMatch(IEnumerable<Entity> matchRecords)
        {
            var results = new List<Entity>();
            foreach(var match in matchRecords)
            {
                if(_matchNameFilters.ContainsKey(match.LogicalName))
                {
                    if(!XrmEntity.MeetsConditions(match.GetField, _matchNameFilters[match.LogicalName]))
                    {
                        continue;
                    }
                }
                results.Add(match);
            }
            return results;
        }
    }
}