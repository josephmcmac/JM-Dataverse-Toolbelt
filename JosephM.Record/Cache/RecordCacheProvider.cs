using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Record.IService;

namespace JosephM.Record.Cache
{
    /// <summary>
    ///     Provides An Interface To Lookup Records Without Repeated Querying Of The Same Record Or Type
    /// </summary>
    public class RecordCacheProvider
    {
        private readonly
            IDictionary
                <string, IDictionary<string, IDictionary<string, IDictionary<string, IEnumerable<string>>>>>
            _cachedAssociations =
                new SortedDictionary
                    <string,
                        IDictionary<string, IDictionary<string, IDictionary<string, IEnumerable<string>>>>>();

        private readonly IDictionary<string, IDictionary<string, IDictionary<string, IRecord>>>
            _cachedEntities = new SortedDictionary<string, IDictionary<string, IDictionary<string, IRecord>>>();

        private readonly IDictionary<string, IDictionary<string, IDictionary<string, string>>>
            _cachedLookups = new SortedDictionary<string, IDictionary<string, IDictionary<string, string>>>();

        public RecordCacheProvider(IRecordService service, LogController controller, IRecordCacheConfig cacheConfig)
        {
            Service = service;
            Controller = controller;
            RecordCacheConfig = cacheConfig;
        }

        private readonly Object lockObject = new Object();

        private IRecordService Service { get; set; }

        private IRecordCacheConfig RecordCacheConfig { get; set; }

        private LogController Controller { get; set; }

        /// <summary>
        ///     If value is not null returns the id of a record with value in the field. If no existing record is found then
        ///     creates a new crm record
        /// </summary>
        public string GetMatchingGuid(string entityType, string fieldName, object value)
        {
            lock (lockObject)
            {
                string result = null;

                var matchValue = (string) value;
                if (string.IsNullOrWhiteSpace(matchValue))
                    return null;
                var fieldSwitches = GetThisMatchCache(entityType, fieldName);
                //Get the entry for this value
                if (fieldSwitches.ContainsKey(matchValue))
                    result = fieldSwitches[matchValue];
                else
                {
                    //if there is no matching entry then query crm directly
                    var entity = Service.GetFirst(entityType, fieldName, value);
                    if (entity != null)
                    {
                        result = entity.Id;
                        fieldSwitches.Add(matchValue, entity.Id);
                    }
                }
                return result;
            }
        }

        /// <summary>
        ///     If value is not null returns the id of a record with value in the field. If no existing record is found then
        ///     creates a new crm record
        /// </summary>
        public IDictionary<string, string> GetMatchingGuids(string entityType, string fieldName,
            IEnumerable<string> values)
        {
            lock (lockObject)
            {
                var fieldSwitches = GetThisMatchCache(entityType, fieldName);

                var matchedGuids = new SortedDictionary<string, string>();
                var unmatchedStrings = new List<string>();
                if (values != null)
                {
                    foreach (var value in values)
                    {
                        var matchValue = value;
                        if (string.IsNullOrWhiteSpace(matchValue))
                            throw new NullReferenceException("Match value cannot be empty");
                        //Get the entry for this value
                        if (fieldSwitches.ContainsKey(matchValue))
                        {
                            matchedGuids.Add(matchValue, fieldSwitches[matchValue]);
                        }
                        else
                        {
                            unmatchedStrings.Add(matchValue);
                        }
                    }
                    if (unmatchedStrings.Any())
                    {
                        var additionalGuids = Service.IndexMatchingGuids(entityType,
                            fieldName,
                            unmatchedStrings);
                        foreach (var unmatchedString in unmatchedStrings)
                        {
                            if (additionalGuids.ContainsKey(unmatchedString) &&
                                additionalGuids[unmatchedString].IsNullOrWhiteSpace())
                            {
                                matchedGuids.Add(unmatchedString, additionalGuids[unmatchedString]);
                                fieldSwitches.Add(unmatchedString, matchedGuids[unmatchedString]);
                            }
                            else
                            {
                                matchedGuids.Add(unmatchedString, null);
                            }
                        }
                    }
                }
                return matchedGuids;
            }
        }

        internal IDictionary<string, string> GetThisMatchCache(string entityType, string fieldName)
        {
            //Get the cache for this record type
            if (!_cachedLookups.ContainsKey(entityType))
            {
                _cachedLookups.Add(entityType, new SortedDictionary<string, IDictionary<string, string>>());
            }
            var entityFieldSwitches = _cachedLookups[entityType];
            //Get the cache for this field on this record type
            if (!entityFieldSwitches.ContainsKey(fieldName))
            {
                if (IsCacheAll(entityType))
                {
                    //if this context not yet accessed and cache all is true then index every record by this field
                    Controller.LogLiteral("Loading all " + entityType + " lookups to cache");
                    entityFieldSwitches.Add(fieldName, Service.IndexGuidsByValue(entityType, fieldName));
                    Controller.LogLiteral("Loaded all " + entityType + " lookups to cache");
                }
                else
                    entityFieldSwitches.Add(fieldName, new SortedDictionary<string, string>());
            }
            var fieldSwitches = entityFieldSwitches[fieldName];
            return fieldSwitches;
        }

        /// <summary>
        ///     returns the set of ids currently associated to the record
        /// </summary>
        public IEnumerable<string> GetExistingIds(string entityType, string id, string relationshipName,
            string otherSideId)
        {
            lock (lockObject)
            {
                //get the cache for this relationship
                var thisAssociationSwitches = GetThisAssociationCache(entityType,
                    relationshipName,
                    otherSideId);
                if (!(thisAssociationSwitches.ContainsKey(id)))
                {
                    //if not already cached then retrieve from crm
                    thisAssociationSwitches.Add(id,
                        Service.GetAssociatedIds(entityType, id, relationshipName, otherSideId));
                }
                return thisAssociationSwitches[id];
            }
        }

        /// <summary>
        ///     Gets the dictionary containing the cached associations for this relationship
        /// </summary>
        internal IDictionary<string, IEnumerable<string>> GetThisAssociationCache(string entityType,
            string relationshipName,
            string otherSideId)
        {
            //Traverse to the dictionary for this association context creating if it doesn;t already exist
            if (!_cachedAssociations.ContainsKey(entityType))
            {
                _cachedAssociations.Add(entityType,
                    new SortedDictionary
                        <string, IDictionary<string, IDictionary<string, IEnumerable<string>>>
                            >());
            }
            var entitySwitches
                = _cachedAssociations[entityType];
            if (!entitySwitches.ContainsKey(relationshipName))
            {
                entitySwitches.Add(relationshipName,
                    new SortedDictionary<string, IDictionary<string, IEnumerable<string>>>());
            }
            var relationshipSwitches =
                entitySwitches[relationshipName];
            if (!(relationshipSwitches.ContainsKey(otherSideId)))
            {
                if (IsCacheAll(entityType))
                {
                    //if cache all and this is the first time accessing this context then load all associations into the dictionary
                    Controller.LogLiteral("Loading all " + entityType + " " + relationshipName +
                                          " relations to cache");
                    relationshipSwitches.Add(otherSideId,
                        Service.IndexAssociatedIds(entityType, relationshipName, otherSideId));
                    Controller.LogLiteral("Loaded all " + entityType + " " + relationshipName + " relations to cache");
                }
                else
                    relationshipSwitches.Add(otherSideId, new SortedDictionary<string, IEnumerable<string>>());
            }
            var thisAssociationSwitches = relationshipSwitches[otherSideId];
            return thisAssociationSwitches;
        }

        /// <summary>
        ///     Explicitly sets the cache for this records associated ids
        /// </summary>
        public void SetAssociatedIds(string entityType, string id, string relationshipName, string otherSideId,
            IEnumerable<string> items)
        {
            lock (lockObject)
            {
                if (items == null)
                    items = new string[] {};
                var thisAssociationSwitches = GetThisAssociationCache(entityType,
                    relationshipName,
                    otherSideId);
                if (!(thisAssociationSwitches.ContainsKey(id)))
                {
                    thisAssociationSwitches.Add(id, items);
                }
                else
                {
                    thisAssociationSwitches[id] = items;
                }
            }
        }

        /// <summary>
        ///     If one exists returns an entity which contains the value in the field
        /// </summary>
        public IRecord GetMatchingEntity(string entityType, string matchField, object value, string[] requiredFields)
        {
            lock (lockObject)
            {
                var matchValue = Service.GetFieldAsMatchString(entityType, matchField, value);
                if (string.IsNullOrWhiteSpace(matchValue))
                    return null;
                //get the cached entities
                var entitySwitches = GetThisEntityCache(entityType, matchField,
                    requiredFields);
                if (!(entitySwitches.ContainsKey(matchValue)))
                {
                    //if no cached record matches then directly query crm
                    var item = Service.GetFirst(entityType, matchField,
                        Service.ParseField(matchField, entityType, value));
                    if (item == null)
                    {
                        return null;
                    }
                    else
                        entitySwitches.Add(matchValue, item);
                }
                return entitySwitches[matchValue];
            }
        }

        /// <summary>
        ///     Gets the dictionary containing the cache for the entity indexed by the field
        /// </summary>
        internal IDictionary<string, IRecord> GetThisEntityCache(string entityType, string matchField,
            string[] requiredFields)
        {
            if (!_cachedEntities.ContainsKey(entityType))
            {
                _cachedEntities.Add(entityType, new SortedDictionary<string, IDictionary<string, IRecord>>());
            }
            var entityFieldSwitches = _cachedEntities[entityType];
            if (!entityFieldSwitches.ContainsKey(matchField))
            {
                if (IsCacheAll(entityType))
                {
                    ////if cache all and this is the first time accessing this context then load everything
                    Controller.LogLiteral("Loading all " + entityType + " entities to cache");
                    entityFieldSwitches.Add(matchField, Service.IndexRecordsByField(entityType, matchField));
                    Controller.LogLiteral("Loaded all " + entityType + " entities to cache");
                }
                else
                    entityFieldSwitches.Add(matchField, new SortedDictionary<string, IRecord>());
            }
            var entitySwitches = entityFieldSwitches[matchField];
            return entitySwitches;
        }

        /// <summary>
        ///     Explicitly sets/adds this entity in the cache
        /// </summary>
        public void SetCachedEntity(string entityType, string matchField, IRecord newItem, string[] requiredFields)
        {
            lock (lockObject)
            {
                var matchValue = Service.GetFieldAsMatchString(entityType, matchField, newItem.GetField(matchField));
                if (string.IsNullOrWhiteSpace(matchValue))
                    return;
                //get the cache dictionary and add this item
                var entitySwitches = GetThisEntityCache(entityType, matchField,
                    requiredFields);
                if (!(entitySwitches.ContainsKey(matchValue)))
                    entitySwitches.Add(matchValue, newItem);
                else
                    entitySwitches[matchValue] = newItem;
            }
        }

        internal bool IsCacheAll(string entityType)
        {
            return RecordCacheConfig.CacheAll &&
                   (RecordCacheConfig.CacheAllExclude == null || !RecordCacheConfig.CacheAllExclude.Contains(entityType));
        }
    }
}