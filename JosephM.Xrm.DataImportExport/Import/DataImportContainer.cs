using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.DataImportExport.Import
{
    public class DataImportContainer
    {
        private readonly Dictionary<IRecord, List<string>> _fieldsToRetry = new Dictionary<IRecord, List<string>>();
        public DataImportContainer(DataImportResponse response, XrmRecordService xrmRecordService, Dictionary<string, IEnumerable<KeyValuePair<string, bool>>> altMatchKeyDictionary, Dictionary<string, Dictionary<string, KeyValuePair<string, string>>> altLookupMatchKeyDictionary, IEnumerable<IRecord> entities, ServiceRequestController controller, bool includeOwner, bool includeOverrideCreatedOn,bool maskEmails, MatchOption matchOption, bool updateOnly, bool containsExportedConfigFields, int executeMultipleSetSize, int targetCacheLimit, bool onlyFieldMatchActive, bool forceSubmitAllFields, bool displayTimeEstimations, int parallelImportProcessCount, bool bypassWorkflowsAndPlugins = false, bool trustSourceLookupGuids = false)
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
            TrustSourceLookupGuids = trustSourceLookupGuids;
            _maxCacheCount = targetCacheLimit;
            EntitiesToImport = entities;
            var typesToImport = entities.Select(e => e.Type).Distinct();

            var allNNRelationships = XrmRecordService.GetAllNnRelationshipEntityNames();

            AssociationTypesToImport = typesToImport.Where(allNNRelationships.Contains).ToArray();
            EntityTypesToImport = typesToImport.Where(t => !AssociationTypesToImport.Contains(t)).ToArray();

            IdSwitches = new Dictionary<string, Dictionary<string, string>>();
            foreach (var item in typesToImport)
            {
                IdSwitches.Add(item, new Dictionary<string, string>());
            }
        }

        public DataImportResponse Response { get; }
        public XrmRecordService XrmRecordService { get; }
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
        public bool TrustSourceLookupGuids { get; private set; }

        public IDictionary<IRecord, List<string>> FieldsToRetry {  get { return _fieldsToRetry; } }
        public IEnumerable<string> AssociationTypesToImport { get; }

        public IEnumerable<IRecord> EntitiesToImport { get; }
        public IEnumerable<string> EntityTypesToImport { get; }
        public Dictionary<string, Dictionary<string, string>> IdSwitches { get; }

        public void LogEntityError(IRecord entity, Exception ex)
        {
            if (_fieldsToRetry.ContainsKey(entity))
            {
                _fieldsToRetry.Remove(entity);
                Response.RemoveFieldForRetry(entity);
            }
            var field = AltMatchKeyDictionary.ContainsKey(entity.Type)
                ? string.Join("|", AltMatchKeyDictionary[entity.Type])
                : null;
            var value = AltMatchKeyDictionary.ContainsKey(entity.Type)
                ? string.Join("|", AltMatchKeyDictionary[entity.Type].Select(k => XrmRecordService.GetFieldAsDisplayString(entity.Type, k.Key, entity.GetField(k.Key))))
                : null;
            var rowNumber = entity.ContainsField("Sheet.RowNumber")
                ? entity.GetIntegerField("Sheet.RowNumber")
                : (int?)null;
            var primaryField = XrmRecordService.GetPrimaryField(entity.Type);
            Response.AddImportError(entity,
                new DataImportResponseItem(entity.Type, field, entity.GetStringField(primaryField), value,
                    ex.Message + (entity.Id != null ? " Id=" + entity.Id : ""),
                    ex, rowNumber: rowNumber));
        }

        public void LogAssociationError(IRecord associationEntity, Exception ex)
        {
            var rowNumber = associationEntity.ContainsField("Sheet.RowNumber")
                ? associationEntity.GetIntegerField("Sheet.RowNumber")
                : (int?)null;
            Response.AddImportError(associationEntity,
            new DataImportResponseItem(
                    $"Error Associating Record Of Type {associationEntity.Type} Id {associationEntity.Id}",
                    ex, rowNumber: rowNumber));
        }

        public IEnumerable<string> GetFieldsInEntities(IEnumerable<IRecord> thisTypeEntities)
        {
            return thisTypeEntities.SelectMany(e => e.GetFieldsInEntity()).Distinct().ToArray();
        }

        public IEnumerable<string> GetFieldsToImport(IEnumerable<IRecord> thisTypeEntities, string type)
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
            if (fieldName == Fields.savedqueryvisualization_.primaryentitytypecode)
                return true;
            if (fieldName == "overriddencreatedon")
                return true;
            if (fieldName == Fields.userquery_.querytype)
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

        private Dictionary<string, Dictionary<string, Dictionary<string, List<IRecord>>>> _cachedRecords = new Dictionary<string, Dictionary<string, Dictionary<string, List<IRecord>>>>();

        public void LoadTargetsToCache(string recordType)
        {
            var recordsReferenced = new HashSet<string>();
            recordsReferenced.Add(recordType);
            var thisTypeEntities = EntitiesToImport.Where(e => e.Type == recordType);
            var fieldsForImport = GetFieldsToImport(thisTypeEntities, recordType);
            foreach (var entity in thisTypeEntities)
            {
                foreach (var field in fieldsForImport)
                {
                    var value = entity.GetField(field);
                    if (value is Core.FieldType.Lookup entityReference)
                    {
                        var type = entityReference.RecordType;
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

            var executeQueryResponses = XrmRecordService.ExecuteMultipleQueries(loadTheseOnes.Select(rt =>
            {
                var matchFilters = _matchFilters.ContainsKey(rt)
                    ? _matchFilters[rt]
                    : new Condition[0];
                var query = new QueryDefinition(rt)
                {
                    Top = _maxCacheCount
                };
                query.RootFilter.Conditions.AddRange(matchFilters);
                return query;
            }).ToArray());


            if (executeQueryResponses.Any())
            {
                try
                {
                    var i = 0;
                    foreach (var response in executeQueryResponses)
                    {
                        var type = loadTheseOnes.ElementAt(i);
                        if (response.Exception != null)
                        {
                            Response.AddImportError(new DataImportResponseItem(
                                 $"Error Loading Target Records Cache for {type})", response.Exception));
                        }
                        else
                        {
                            var records = response.Records;
                            if (!_cachedRecords.ContainsKey(type))
                                _cachedRecords.Add(type, new Dictionary<string, Dictionary<string, List<IRecord>>>());
                            var primaryKey = XrmRecordService.GetPrimaryField(type);
                            if (!_cachedRecords[type].ContainsKey(primaryKey))
                                _cachedRecords[type].Add(primaryKey, new Dictionary<string, List<IRecord>>());

                            foreach (var record in records)
                            {
                                var cacheMatchString = record.Id.ToString();
                                if (!_cachedRecords[type][primaryKey].ContainsKey(cacheMatchString))
                                    _cachedRecords[type][primaryKey].Add(cacheMatchString, new List<IRecord>());
                                _cachedRecords[type][primaryKey][cacheMatchString].Add(record);
                            }
                            var primaryNameField = XrmRecordService.GetPrimaryField(type);
                            if (primaryNameField != null)
                            {
                                if (!_cachedRecords[type].ContainsKey(primaryNameField))
                                    _cachedRecords[type].Add(primaryNameField, new Dictionary<string, List<IRecord>>());
                                foreach (var record in records)
                                {
                                    var cacheMatchString = XrmRecordService.GetFieldAsMatchString(type, primaryNameField, record.GetStringField(primaryNameField));
                                    if (!_cachedRecords[type][primaryNameField].ContainsKey(cacheMatchString))
                                        _cachedRecords[type][primaryNameField].Add(cacheMatchString, new List<IRecord>());
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

        private object _lockObject = new object();
        public IEnumerable<IRecord> GetMatchingEntities(string type, IDictionary<string, object> fieldValues, string ignoreCacheFor = null)
        {
            var conditions = fieldValues.Select(fv =>
                fv.Value == null
                ? new Condition(fv.Key, ConditionType.Null)
                : new Condition(fv.Key, ConditionType.Equal, XrmRecordService.ParseField(fv.Key, type, fv.Value))
            ).ToList();

            var matchFilters = _matchFilters.ContainsKey(type)
                ? _matchFilters[type]
                : new Condition[0];

            if (type != ignoreCacheFor
                && conditions.Count == 1
                && fieldValues.Values.First() != null)
            {
                CheckLoadCache(new[] { type });
                var fieldName = fieldValues.Keys.First();

                var matchString = XrmRecordService.GetFieldAsMatchString(type, fieldName, fieldValues.Values.First());
                lock (_lockObject)
                {
                    if (!_cachedRecords.ContainsKey(type))
                        _cachedRecords.Add(type, new Dictionary<string, Dictionary<string, List<IRecord>>>());
                }
                lock (_lockObject)
                {
                    if (!_cachedRecords[type].ContainsKey(fieldName))
                    {
                        var query = new QueryDefinition(type);
                        query.RootFilter.Conditions.AddRange(matchFilters);
                        query.Top = _maxCacheCount;
                        var recordsToCache = XrmRecordService.RetreiveAll(query);
                        _cachedRecords[type].Add(fieldName, new Dictionary<string, List<IRecord>>());
                        foreach (var item in recordsToCache)
                        {
                            var cacheMatchString = XrmRecordService.GetFieldAsMatchString(type, fieldName, item.GetField(fieldName));
                            if (!_cachedRecords[type][fieldName].ContainsKey(cacheMatchString))
                            {
                                _cachedRecords[type][fieldName].Add(cacheMatchString, new List<IRecord>());
                            }
                            _cachedRecords[type][fieldName][cacheMatchString].Add(item);
                        }
                    }
                }
                //only use the cache if there were less than maxRecords
                //otherwise there may be duplicates not included
                lock (_lockObject)
                {
                    if (_cachedRecords[type][fieldName].SelectMany(kv => kv.Value).Count() < _maxCacheCount
                        && _cachedRecords[type][fieldName].ContainsKey(matchString))
                    {
                        return _cachedRecords[type][fieldName][matchString];
                    }
                }
            }
            return XrmRecordService.RetrieveAllAndClauses(type, conditions.Union(matchFilters).ToArray());
        }

        private IEnumerable<IRecord> GetMatchesByNameForRootRecord(TypeConfigs.Config parentChildConfig, string name)
        {
            //okay if this is a parent record (e.g a root web page)
            //then match by name and where the parent reference is empty
            if (name == null)
            {
                throw new NullReferenceException("Name Is Null For Parent Record");
            }
            var matches = XrmRecordService.RetrieveAllAndClauses(parentChildConfig.Type,
                        new[] {
                                new Condition(parentChildConfig.ParentLookupField, ConditionType.Null),
                                new Condition(XrmRecordService.GetPrimaryField(parentChildConfig.Type), ConditionType.Equal, name) });
            if (matches.Count() > 1)
            {
                throw new Exception($"More Than One Record Match To The {XrmRecordService.GetPrimaryField(parentChildConfig.Type)} Of {name}");
            }
            return matches;
        }

        public IEnumerable<IRecord> GetMatchingEntities(string type, string field, string value, string ignoreCacheFor = null)
        {
            var typeConfig = XrmRecordService.GetTypeConfigs().GetFor(type);
            if (typeConfig == null || typeConfig.ParentLookupType != type || field != XrmRecordService.GetPrimaryField(type))
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

        public IRecord GetUniqueMatchingEntity(string type, string field, string value)
        {
            var matchingRecords = GetMatchingEntities(type, field, value);
            if (!matchingRecords.Any())
            {
                throw new NullReferenceException($"No Record Matched To The {"Name"} Of {value} When Matching The Name");
            }
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

        private IRecord _rootBusinessUnit;
        public IRecord GetRootBusinessUnit()
        {
            if (_rootBusinessUnit == null)
            {
                _rootBusinessUnit = XrmRecordService.GetFirst(Entities.businessunit, Fields.businessunit_.parentbusinessunitid, null, new string[0]);
            }
            return _rootBusinessUnit;
        }

        public QueryDefinition GetMatchQueryExpression(IRecord thisEntity, DataImportContainer dataImportContainer)
        {
            var thisTypesConfig = XrmRecordService.GetTypeConfigs().GetFor(thisEntity.Type);
            if (thisTypesConfig != null)
            {
                var matchQuery = new QueryDefinition(thisTypesConfig.Type);
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
                var primaryKey = XrmRecordService.GetPrimaryKey(thisEntity.Type);
                var primaryName = XrmRecordService.GetPrimaryField(thisEntity.Type);
                var matchQuery = new QueryDefinition(thisEntity.Type);
                if (AltMatchKeyDictionary.ContainsKey(thisEntity.Type))
                {
                    var matchKeyFieldDictionary = AltMatchKeyDictionary[thisEntity.Type]
                        .Distinct().ToDictionary(f => f.Key, f => thisEntity.GetField(f.Key));

                    foreach(var matchKeyField in matchKeyFieldDictionary)
                    {
                        if (matchKeyField.Value is Lookup er
                            && er.Id == null
                            && !string.IsNullOrWhiteSpace(er.Name)
                            && !string.IsNullOrWhiteSpace(er.RecordType)
                            && XrmRecordService.RecordTypeExists(er.RecordType))
                        {
                            var linkTo = new Join(matchKeyField.Key, er.RecordType, XrmRecordService.GetPrimaryKey(er.RecordType));
                            matchQuery.Joins.Add(linkTo);
                            if (dataImportContainer.AltLookupMatchKeyDictionary != null
                                && dataImportContainer.AltLookupMatchKeyDictionary.ContainsKey(thisEntity.Type)
                                && dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.Type].ContainsKey(matchKeyField.Key))
                            {
                               var altMatchType = dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.Type][matchKeyField.Key].Key;
                                var altMatchField = dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.Type][matchKeyField.Key].Value;
                                linkTo.RootFilter.AddCondition(altMatchField, ConditionType.Equal, er.Name);
                            }
                            else
                            {
                                linkTo.RootFilter.AddCondition(XrmRecordService.GetPrimaryField(er.RecordType), ConditionType.Equal, er.Name);
                            }
                        }
                        else
                        {
                            matchQuery.RootFilter.AddCondition(matchKeyField.Key, ConditionType.Equal, matchKeyField.Value);
                        }
                    }
                    if(OnlyFieldMatchActive)
                    {
                        matchQuery.RootFilter.AddCondition("statecode", ConditionType.Equal, 0);
                    }
                }
                else if (MatchOption == MatchOption.PrimaryKeyThenName || thisTypesConfig != null)
                {
                    matchQuery.RootFilter.ConditionOperator = FilterOperator.Or;
                    var orFilter = new Filter() {  ConditionOperator = FilterOperator.Or };
                    matchQuery.RootFilter.SubFilters.Add(orFilter);
                    if (string.IsNullOrWhiteSpace(thisEntity.Id))
                    {
                        orFilter.AddCondition(primaryKey, ConditionType.Null);
                    }
                    else
                    {
                        orFilter.AddCondition(primaryKey, ConditionType.Equal, thisEntity.Id);
                    }
                    if (primaryName != null && thisEntity.GetStringField(primaryName) != null)
                    {
                        orFilter.AddCondition(primaryName, ConditionType.Equal, thisEntity.GetStringField(primaryName));
                    }
                    if (OnlyFieldMatchActive)
                    {
                        matchQuery.RootFilter.AddCondition("statecode", ConditionType.Equal, 0);
                    }
                }
                else if (MatchOption == MatchOption.PrimaryKeyOnly)
                {
                    if (string.IsNullOrWhiteSpace(thisEntity.Id))
                    {
                        matchQuery.RootFilter.AddCondition(primaryKey, ConditionType.Null);
                    }
                    else
                    {
                        matchQuery.RootFilter.AddCondition(primaryKey, ConditionType.Equal, thisEntity.Id);
                    }
                }
                return matchQuery;
            }
        }

        public QueryDefinition GetParseLookupQuery(IRecord thisEntity, string field, string targetType, string matchField)
        {
            object referencedValue = thisEntity.GetLookupName(field) ?? "";
            var referencedId = thisEntity.GetLookupId(field) ?? Guid.Empty.ToString();
            var primaryKey = XrmRecordService.GetPrimaryKey(targetType);
            var configs = XrmRecordService.GetTypeConfigs();
            var thisTypeConfig = configs.GetFor(thisEntity.Type);
            var targetTypeConfig = configs.GetFor(targetType);
            if (thisTypeConfig != null && targetTypeConfig != null && ContainsExportedConfigFields)
            {
                var matchQuery = new QueryDefinition(targetType);
                matchQuery.RootFilter.AddCondition(matchField, ConditionType.Equal, referencedValue);
                
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
                var switchRootFilterToOrId = new Filter();
                switchRootFilterToOrId.ConditionOperator = FilterOperator.Or;
                switchRootFilterToOrId.AddCondition(primaryKey, ConditionType.Equal, referencedId);
                switchRootFilterToOrId.SubFilters.Add(matchQuery.RootFilter);
                matchQuery.RootFilter = switchRootFilterToOrId;
                return matchQuery;
            }
            else
            {
                var matchQuery = new QueryDefinition(targetType);
                matchQuery.RootFilter.ConditionOperator = FilterOperator.Or;
                matchQuery.RootFilter.AddCondition(primaryKey, ConditionType.Equal, referencedId);
                if (matchField != null && matchField != primaryKey && referencedValue != null)
                {
                    if(XrmRecordService.IsLookup(matchField, targetType))
                    {
                        referencedValue = thisEntity.GetLookupId(field) ?? Guid.Empty.ToString();
                    }
                    matchQuery.RootFilter.AddCondition(matchField, ConditionType.Equal, referencedValue);
                }
                return matchQuery;
            }
        }

        public void AddUniqueFieldConfigJoins(IRecord thisEntity, QueryDefinition matchQuery, IEnumerable<string> uniqueFields, string prefixFieldInEntity = null)
        {
            foreach (var field in uniqueFields)
            {
                var theValue = thisEntity.GetField(prefixFieldInEntity + field);
                if (theValue == null)
                {
                    matchQuery.RootFilter.Conditions.Add(new Condition(field, ConditionType.Null));
                }
                else if (theValue is Lookup lookup)
                {
                    var name = lookup.Name;
                    var type = lookup.RecordType;
                    var linkToReferenced = new Join(field, type, XrmRecordService.GetPrimaryKey(type));
                    matchQuery.Joins.Add(linkToReferenced);
                    if (name == null)
                    {
                        linkToReferenced.RootFilter.Conditions.Add(new Condition(XrmRecordService.GetPrimaryField(type), ConditionType.Null));
                    }
                    else
                    {
                        linkToReferenced.RootFilter.AddCondition(XrmRecordService.GetPrimaryField(type), ConditionType.Equal, name);
                        if (ContainsExportedConfigFields)
                        {
                            AddReferenceConfigJoins(linkToReferenced, thisEntity, field);
                        }
                    }
                }
                else
                {
                    matchQuery.RootFilter.AddCondition(field, ConditionType.Equal, theValue);
                }
            }
        }

        public void AddCreated(IRecord originalEntity)
        {
            Response.AddCreated(originalEntity);
            var thisRecordType = originalEntity.Type;
            if (_cachedRecords.ContainsKey(thisRecordType))
            {
                foreach(var fieldDictionary in _cachedRecords[thisRecordType])
                {
                    var indexedField = fieldDictionary.Key;
                    var matchString = XrmRecordService.GetFieldAsMatchString(thisRecordType, indexedField, originalEntity.GetField(indexedField));
                    if (!_cachedRecords[thisRecordType][indexedField].ContainsKey(matchString))
                    {
                        _cachedRecords[thisRecordType][indexedField].Add(matchString, new List<IRecord>());
                    }
                    _cachedRecords[thisRecordType][indexedField][matchString].Add(originalEntity);
                }
            }
        }

        private void AddReferenceConfigJoins(Join linkToReferenced, IRecord thisEntity, string field)
        {
            var referencedType = thisEntity.GetLookupType(field);
            var referencedTypeConfig = XrmRecordService.GetTypeConfigs().GetFor(referencedType);
            if (referencedTypeConfig != null && referencedTypeConfig.UniqueChildFields != null)
            {
                foreach (var uniqueField in referencedTypeConfig.UniqueChildFields)
                {
                    var theValue = thisEntity.GetField($"{field}.{uniqueField}");
                    if (theValue == null)
                    {
                        linkToReferenced.RootFilter.Conditions.Add(new Condition(uniqueField, ConditionType.Null));
                    }
                    else if (theValue is Lookup lookup)
                    {
                        var name = lookup.Name;
                        var type = lookup.RecordType;
                        var nextLinkToReferenced = new Join(uniqueField, type, XrmRecordService.GetPrimaryKey(type));
                        linkToReferenced.Joins.Add(nextLinkToReferenced);
                        if (name == null)
                        {
                            nextLinkToReferenced.RootFilter.Conditions.Add(new Condition(XrmRecordService.GetPrimaryField(type), ConditionType.Null));
                        }
                        else
                        {
                            nextLinkToReferenced.RootFilter.AddCondition(XrmRecordService.GetPrimaryField(type), ConditionType.Equal, name);
                            AddReferenceConfigJoins(nextLinkToReferenced, thisEntity, $"{field}.{uniqueField}");
                        }
                    }
                    else
                    {
                        linkToReferenced.RootFilter.AddCondition(uniqueField, ConditionType.Equal, theValue);
                    }
                }
            }
        }

        private Dictionary<string, IEnumerable<Condition>> _matchFilters = new Dictionary<string, IEnumerable<Condition>>
        {
            { Entities.workflow, new [] { new Condition(Fields.workflow_.type, ConditionType.Equal, XrmPicklists.WorkflowType.Definition)}}
        };

        private Dictionary<string, IEnumerable<Condition>> _matchNameFilters = new Dictionary<string, IEnumerable<Condition>>
        {
            { Entities.contact, new [] { new Condition(Fields.contact_.merged, ConditionType.NotEqual, true)}},
            { Entities.account, new [] { new Condition(Fields.account_.merged, ConditionType.NotEqual, true)}},
            { Entities.knowledgearticle, new [] { new Condition(Fields.knowledgearticle_.isrootarticle, ConditionType.NotEqual, true) }}
        };

        public bool IsValidForCache(string recordType)
        {
            var primaryKey = XrmRecordService.GetPrimaryField(recordType);
            return
                _cachedRecords.ContainsKey(recordType)
                && _cachedRecords[recordType].ContainsKey(primaryKey)
                && _cachedRecords[recordType][primaryKey].SelectMany(kv => kv.Value).Count() < _maxCacheCount;
        }

        public IEnumerable<IRecord> FilterForNameMatch(IEnumerable<IRecord> matchRecords)
        {
            var results = new List<IRecord>();
            foreach(var match in matchRecords)
            {
                if(_matchNameFilters.ContainsKey(match.Type))
                {
                    if(_matchNameFilters[match.Type].Any(c => !c.MeetsCondition(match)))
                    {
                        continue;
                    }
                }
                results.Add(match);
            }
            return results;
        }

        public void RemoveFromCache(string recordType)
        {
            lock (_lockObject)
            {
                if (_cachedRecords.ContainsKey(recordType))
                {
                    _cachedRecords.Remove(recordType);
                }
            }
        }
    }
}