using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Serialisation;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JosephM.Record.Extentions
{
    /// <summary>
    ///     General Use Utility Methods For IRecords
    /// </summary>
    public static class RecordExtentions
    {
        public static bool FieldsEqual(this IRecord record, string fieldName, object value)
        {
            var field1 = record.GetField(fieldName);
            var field2 = value;
            if (field1 == null && field2 == null)
                return true;
            else if (field1 == null || field2 == null)
            {
                if (field1 is string || field2 is string)
                    return String.IsNullOrEmpty((string)field1) && String.IsNullOrEmpty((string)field2);
                else
                    return false;
            }
            return field1.Equals(field2);
        }

        /// <summary>
        ///     Loads The IRecord Into A Lookup. Does Not Load Name
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static Lookup ToLookup(this IRecord record)
        {
            return ToLookup(null, record);
        }

        public static Lookup ToLookup(this IRecordService service, IRecord record)
        {
            var lookup = new Lookup(record.Type, record.Id, null);
            if (service != null)
            {
                var primaryField = service.GetDisplayNameField(lookup.RecordType);
                if (primaryField != null)
                    lookup.Name = record.GetStringField(primaryField);
            }
            if (lookup.Name.IsNullOrWhiteSpace())
                lookup.Name = "Name Not Loaded";
            return lookup;
        }

        public static Lookup ToLookupWithAltDisplayNameName(this IRecordService service, IRecord record)
        {
            var displayStrings = new List<string>();

            var config = service.GetTypeConfigs().GetFor(record.Type);
            if (config != null)
            {
                if (config.ParentLookupField != null)
                {
                    var thisOne = service.GetFieldAsDisplayString(record, config.ParentLookupField);
                    if (!string.IsNullOrWhiteSpace(thisOne))
                        displayStrings.Add(thisOne);
                }
                if (config.UniqueChildFields != null)
                {
                    foreach (var unique in (config.UniqueChildFields))
                    {
                        var thisOne = service.GetFieldAsDisplayString(record, unique);
                        if (!string.IsNullOrWhiteSpace(thisOne))
                            displayStrings.Add(thisOne);
                    }
                }
            }
            if (!displayStrings.Any())
                return service.ToLookup(record);
            return new Lookup(record.Type, record.Id, string.Join(".", displayStrings));
        }

        /// <summary>
        ///     Loads The IRecord Into A Lookup. Does Not Load Name
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static IDictionary<string, object> GetFields(this IRecord record)
        {
            var fields = record.GetFieldsInEntity();
            return fields.ToDictionary(f => f, record.GetField);
        }

        /// <summary>
        ///     Populates All Lookup Fields In The Set Of IRecords With The Name Of The Referenced Record
        ///     Used If The IRecordService Has Not Loaded The Name Of Referenced Records
        /// </summary>
        /// <param name="records">Records To Load The Names Into Lookup Fields</param>
        /// <param name="service">Service To Retrieve The Record Names</param>
        /// <param name="ignoreType">Types To Not Load The Name</param>
        public static void PopulateEmptyLookups(this IEnumerable<IRecord> records, IRecordService service,
            IEnumerable<string> ignoreType)
        {
            var emptyLookupNames = new Dictionary<string, List<Lookup>>();
            foreach (var record in records)
            {
                foreach (var field in record.GetFieldsInEntity())
                {
                    if (!field.IsNullOrWhiteSpace())
                    {
                        var value = record.GetField(field);
                        if (value is Lookup && ((Lookup)value).Name == null)
                        {
                            if (!emptyLookupNames.ContainsKey(field))
                                emptyLookupNames.Add(field, new List<Lookup>());
                            emptyLookupNames[field].Add((Lookup)value);
                        }
                    }
                }
            }
            PopulateLookups(service, emptyLookupNames, ignoreType);
        }

        public static void PopulateLookups(this IRecordService service, Dictionary<string, List<Lookup>> emptyLookupNames, IEnumerable<string> ignoreType)
        {
            if (ignoreType == null)
                ignoreType = new string[0];
            foreach (var field in emptyLookupNames.Keys)
            {
                var distinctTypes =
                    emptyLookupNames[field].Select(l => l.RecordType).Distinct();
                foreach (var distinctType in distinctTypes.Where(t => !ignoreType.Contains(t)))
                {
                    try
                    {
                        var thisDistinctType = distinctType;
                        if (!thisDistinctType.IsNullOrWhiteSpace())
                        {
                            var typePrimaryKey = service.GetRecordTypeMetadata(distinctType).PrimaryKeyName;
                            var typePrimaryField = service.GetRecordTypeMetadata(distinctType).PrimaryFieldSchemaName;
                            if (!typePrimaryField.IsNullOrWhiteSpace() && !typePrimaryKey.IsNullOrWhiteSpace())
                            {
                                var thisTypeLookups =
                                    emptyLookupNames[field].Where(l => l.RecordType == thisDistinctType).ToArray();
                                var distinctIds =
                                    thisTypeLookups.Select(l => l.Id).Where(s => !s.IsNullOrWhiteSpace()).Distinct();
                                var conditions =
                                    distinctIds.Select(id => new Condition(typePrimaryKey, ConditionType.Equal, id));
                                var theseRecords = service.RetrieveAllOrClauses(thisDistinctType, conditions,
                                    new[] { typePrimaryField }).ToArray();
                                foreach (var lookup in thisTypeLookups)
                                {
                                    if (theseRecords.Any(r => r.Id == lookup.Id))
                                        lookup.Name = theseRecords.First(r => r.Id == lookup.Id)
                                            .GetStringField(typePrimaryField);
                                }
                            }
                        }
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        public static object LoadObject(this IRecordService recordService, Lookup lookup, IStoredObjectFields objectConfig)
        {
            var record = recordService.Get(lookup.RecordType, lookup.Id);
            var assemblyString = record.GetStringField(objectConfig.AssemblyField);
            var typeString = record.GetStringField(objectConfig.TypeQualfiedNameField);
            var jsonString = record.GetStringField(objectConfig.ValueField);
            var assembly = Assembly.Load(assemblyString);
            var type = assembly.GetType(typeString);
            return JsonHelper.JsonStringToObject(jsonString, type);
        }

        public static string Create(this IRecordService recordService, IRecord record)
        {
            return recordService.Create(record, null);
        }

        public static void Update(this IRecordService recordService, IRecord record)
        {
            recordService.Update(record, null);
        }

        public static void Delete(this IRecordService recordService, IRecord record)
        {
            recordService.Delete(record.Type, record.Id);
        }

        public static IEnumerable<IRecord> RetrieveAll(this IRecordService recordService, string recordType, IEnumerable<string> fields)
        {
            return recordService.GetFirstX(recordType, -1, fields, null, null);
        }

        public static IRecord GetFirst(this IRecordService recordService, string recordType)
        {
            var results = recordService.GetFirstX(recordType, 1, null, null, null);
            return results.Any() ? results.First() : null;
        }

        public static IEnumerable<IRecord> RetrieveAllAndClauses(this IRecordService recordService, string recordType, IEnumerable<Condition> conditions, IEnumerable<string> fields)
        {
            return recordService.GetFirstX(recordType, -1, fields, conditions, null);
        }

        public static string GetFieldLabel(this IRecordService recordService, string fieldName, string recordtype)
        {
            return recordService.GetFieldMetadata(fieldName, recordtype).DisplayName ?? fieldName;
        }

        public static int GetMaxLength(this IRecordService recordService, string fieldName, string recordtype)
        {
            return recordService.GetFieldMetadata(fieldName, recordtype).MaxLength;
        }

        public static RecordFieldType GetFieldType(this IRecordService recordService, string fieldName, string recordtype)
        {
            return recordService.GetFieldMetadata(fieldName, recordtype).FieldType;
        }

        public static IRecord GetFirst(this IRecordService recordService, string recordType, string fieldName, object fieldValue)
        {
            var results = recordService.GetFirstX(recordType, 1, null, new[] { new Condition(fieldName, ConditionType.Equal, fieldValue) }, null);
            return results.Any() ? results.First() : null;
        }

        public static IFieldMetadata GetFieldMetadata(this IRecordService recordService, string fieldName, string recordType)
        {
            var fieldMetadata = recordService.GetFieldMetadata(recordType).Where(f => f.SchemaName == fieldName);
            if (!fieldMetadata.Any())
                throw new NullReferenceException(string.Format("No field in the {0} types metadata has name of {1}", recordType,
                    fieldName));
            return fieldMetadata.First();
        }


        public static IEnumerable<string> GetFields(this IRecordService recordService, string recordType)
        {
            return recordService.GetFieldMetadata(recordType).Select(m => m.SchemaName).ToArray();
        }

        public static string GetDisplayName(this IRecordService recordService, string recordType)
        {
            return recordService.GetRecordTypeMetadata(recordType).DisplayName;
        }

        public static string GetCollectionName(this IRecordService recordService, string recordType)
        {
            return recordService.GetRecordTypeMetadata(recordType).CollectionName;
        }

        public static string GetPrimaryField(this IRecordService recordService, string recordType)
        {
            return recordService.GetRecordTypeMetadata(recordType).PrimaryFieldSchemaName;
        }

        public static string GetPrimaryKey(this IRecordService recordService, string recordType)
        {
            return recordService.GetRecordTypeMetadata(recordType).PrimaryKeyName;
        }

        public static bool IsLookup(this IRecordService recordService, string fieldName, string recordType)
        {
            return recordService.GetFieldType(fieldName, recordType) == RecordFieldType.Lookup;
        }

        public static IEnumerable<PicklistOption> GetPicklistKeyValues(this IRecordService recordService,
            string fieldName, string recordType)
        {
            return recordService.GetPicklistKeyValues(fieldName, recordType, null, null);
        }

        public static string GetPicklistLabel(this IRecordService recordService, string fieldName, string recordType, string value)
        {
            var options = recordService.GetPicklistKeyValues(fieldName, recordType, null, null);
            if (options.Any(o => o.Key == value))
                return options.First(o => o.Key == value).Value;
            return value;
        }

        public static ParseFieldResponse ParseFieldRequest(this IRecordService recordService, ParseFieldRequest request)
        {
            try
            {
                var parsed = recordService.ParseField(request.FieldName, request.RecordType, request.Value);
                return new ParseFieldResponse(parsed);
            }
            catch (Exception ex)
            {
                return new ParseFieldResponse(ex.Message);
            }
        }

        public static SortedDictionary<string, IRecord> IndexRecordsByValue(this IRecordService recordService, string recordType, string indexByField, IEnumerable<string> requiredFields)
        {
            var items = recordService.RetrieveAllAndClauses(recordType, null, requiredFields);

            var result = new SortedDictionary<string, IRecord>();
            foreach (var record in items)
            {
                var fieldObject = record.GetField(indexByField);
                var fieldString = fieldObject == null ? null : fieldObject.ToString();
                if (!String.IsNullOrWhiteSpace(fieldString) && !result.ContainsKey(fieldString))
                    result.Add(fieldString, record);
            }
            return result;
        }

        public static bool IsMultiline(this IFieldMetadata fieldMetadata)
        {
            return fieldMetadata.FieldType == RecordFieldType.Memo || fieldMetadata.TextFormat == TextFormat.TextArea;
        }

        public static bool IsActivityParty(this IFieldMetadata fieldMetadata)
        {
            return fieldMetadata.FieldType == RecordFieldType.ActivityParty;
        }

        public static bool IsString(this IRecordService recordService, string fieldName, string recordType)
        {
            return new[] { RecordFieldType.String, RecordFieldType.Memo }.Contains(recordService.GetFieldType(fieldName, recordType));
        }

        public static SortedDictionary<string, string> IndexMatchingGuids(this IRecordService recordService, string entityName, string matchField,
    IEnumerable<string> matchValues)
        {
            var result = new SortedDictionary<string, string>();
            if (matchValues != null && matchValues.Any())
            {
                var filterExpressions = new List<Condition>();
                foreach (var matchValue in matchValues)
                {
                    var parseValue = recordService.ParseField(matchField, entityName, matchValue);
                    var stringValue = recordService.GetFieldAsMatchString(entityName, matchField, parseValue);
                    filterExpressions.Add(new Condition(matchField, ConditionType.Equal,
                        stringValue));
                }
                var records = recordService.RetrieveAllOrClauses(entityName, filterExpressions, new[] { matchField });

                foreach (var entity in records)
                {
                    var matchValue = entity.GetStringField(recordService.GetFieldAsMatchString(entityName, matchField, matchField));
                    if (!result.ContainsKey(matchValue))
                        result.Add(matchValue, entity.Id.ToString());
                }
                foreach (var value in matchValues)
                {
                    if (!result.ContainsKey(value))
                        result.Add(value, null);
                }
            }
            return result;
        }

        /// <summary>
        ///     Warning! Unlimited results
        /// </summary>
        public static SortedDictionary<string, string> IndexGuidsByValue(this IRecordService recordService, string entityType, string indexByField)
        {
            return recordService.IndexIdsByField(indexByField, recordService.RetrieveAll(entityType, new[] { indexByField }));
        }

        public static SortedDictionary<string, string> IndexIdsByField(this IRecordService recordService, string indexByField, IEnumerable<IRecord> records)
        {
            var result = new SortedDictionary<string, string>();
            foreach (var record in records)
            {
                var fieldValue = recordService.GetFieldAsMatchString(record.Type, indexByField,
                    record.GetField(indexByField));
                if (!String.IsNullOrWhiteSpace(fieldValue) && !result.ContainsKey(fieldValue))
                    result.Add(fieldValue, record.Id);
            }
            return result;
        }

        public static bool RecordTypeExists(this IRecordService recordService, string recordType)
        {
            return recordService.GetAllRecordTypes().Contains(recordType);
        }

        public static bool FieldExists(this IRecordService recordService, string fieldName, string recordType)
        {
            return recordService.GetFieldMetadata(recordType).Any(f => f.SchemaName == fieldName);
        }

        public static IMany2ManyRelationshipMetadata GetManyRelationshipMetadata(this IRecordService recordService, string name, string recordType)
        {
            var match = recordService.GetManyToManyRelationships(recordType).Where(r => r.SchemaName == name);
            if (!match.Any())
                throw new NullReferenceException(string.Format("No {0} relationship for type {1} has name {2}", typeof(IMany2ManyRelationshipMetadata).Name, recordType, name));
            return match.First();
        }

        public static IPicklistSet GetSharedPicklist(this IRecordService recordService, string name)
        {
            var match = recordService.GetSharedPicklists()
                .Where(r => r.SchemaName == name)
                .ToArray();
            if (!match.Any())
                throw new NullReferenceException("No picklist found with name " + name);
            return match.First();
        }

        public static IEnumerable<PicklistOption> GetSharedPicklistOptions(this IRecordService recordService, string name)
        {
            return recordService.GetSharedPicklist(name).PicklistOptions;
        }

        public static IntegerType GetIntegerFormat(this IRecordService recordService, string fieldName, string recordType)
        {
            return recordService.GetFieldMetadata(fieldName, recordType).IntegerFormat;
        }
    }
}
