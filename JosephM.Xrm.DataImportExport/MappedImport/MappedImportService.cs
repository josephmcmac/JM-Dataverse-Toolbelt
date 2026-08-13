using JosephM.Application.Application;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.DataImportExport.Import;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JosephM.Xrm.DataImportExport.MappedImport
{
    public class MappedImportService
    {
        public MappedImportService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public XrmRecordService XrmRecordService { get; }
        public IApplicationController ApplicationController { get; }

        public MappedImportResponse DoImport(Dictionary<IMapSourceImport, IEnumerable<IRecord>> mappingsWithTargetData, MappedImportValidationResponse mappedImportValidationResponse, bool maskEmails, bool matchByName, bool updateOnly, ServiceRequestController controller, int? executeMultipleSetSize = null, bool useAmericanDates = false, int? targetCacheLimit = null, bool ignoreNullValues = false, bool onlyFieldMatchActive = false, bool forceSubmitAllFields = false, int parallelImportProcessCount = 1, bool bypassWorkflowsAndPlugins = false, bool trustSourceLookupGuids = false)
        {
            var response = new MappedImportResponse();
            if (mappedImportValidationResponse != null)
            {
                response.LoadParseResponse(mappedImportValidationResponse);
            }
            var dataImportService = new DataImportService(XrmRecordService);
            var matchKeyDictionary = new Dictionary<string, IEnumerable<KeyValuePair<string, bool>>>();
            foreach(var map in mappingsWithTargetData.Keys)
            {
                if(map.AltMatchKeys != null && map.AltMatchKeys.Any())
                {
                    if (matchKeyDictionary.ContainsKey(map.TargetType))
                    {
                        throw new NotSupportedException($"Error Type {map.TargetType} Is Defined With Multiple Match Keys");
                    }
                    matchKeyDictionary.Add(map.TargetType, map.AltMatchKeys.Select(mk => new  KeyValuePair<string, bool>(mk.TargetField, mk.CaseSensitive)).ToArray());
                }
            }
            var lookupKeyDictionary = new Dictionary<string, Dictionary<string, KeyValuePair<string, string>>>();
            foreach (var map in mappingsWithTargetData.Keys)
            {
                if (map.FieldMappings != null)
                {
                    foreach (var fieldMapping in map.FieldMappings)
                    {
                        if (fieldMapping.UseAltMatchField)
                        {
                            if(!lookupKeyDictionary.ContainsKey(map.TargetType))
                            {
                                lookupKeyDictionary.Add(map.TargetType, new Dictionary<string, KeyValuePair<string, string>>());
                            }
                            if(lookupKeyDictionary[map.TargetType].ContainsKey(fieldMapping.TargetField))
                            {
                                throw new NotSupportedException($"Error Type {map.TargetType} Field {fieldMapping.TargetField} Cannot Use Have {nameof(IMapSourceField.UseAltMatchField)} True When The Field Has Multiple Maps in The Import");
                            }
                            lookupKeyDictionary[map.TargetType].Add(fieldMapping.TargetField, new KeyValuePair<string, string>(fieldMapping.AltMatchFieldType, fieldMapping.AltMatchField));
                        }
                    }
                }
            }
            response.LoadDataImport(dataImportService.DoImport(mappingsWithTargetData.SelectMany(m => m.Value).ToArray(), controller, maskEmails, matchOption: matchByName ? MatchOption.PrimaryKeyThenName : MatchOption.PrimaryKeyOnly, loadExistingErrorsIntoSummary: response.ResponseItems, altMatchKeyDictionary: matchKeyDictionary, altLookupMatchKeyDictionary: lookupKeyDictionary, updateOnly: updateOnly, includeOwner: true, includeOverrideCreatedOn: true, containsExportedConfigFields: false, executeMultipleSetSize: executeMultipleSetSize, targetCacheLimit: targetCacheLimit, onlyFieldMatchActive: onlyFieldMatchActive, forceSubmitAllFields: forceSubmitAllFields, displayTimeEstimations: true, parallelImportProcessCount: parallelImportProcessCount, bypassWorkflowsAndPlugins: bypassWorkflowsAndPlugins, trustSourceLookupGuids: trustSourceLookupGuids));
            return response;
        }

        public MappedImportValidationResponse TransformMappingDictionaryDataForTarget(Dictionary<IMapSourceImport, IEnumerable<IRecord>> mappings, LogController logController, bool useAmericanDates = false, bool ignoreNullValues = false)
        {
            var response = TransformMappingIntoTarget(mappings, logController, ignoreNullValues: ignoreNullValues);
            foreach (var mapping in mappings)
            {
                if(mapping.Key.ExplicitValuesToSet != null)
                {
                    foreach (var explicitValueToSet in mapping.Key.ExplicitValuesToSet)
                    {
                        var parseFieldValue = explicitValueToSet.ClearValue
                            ? null
                            : XrmRecordService.ParseField(explicitValueToSet.FieldToSet.Key, mapping.Key.TargetType, explicitValueToSet.ValueToSet);
                        foreach (var record in mapping.Value)
                        {
                            record[explicitValueToSet.FieldToSet.Key] = parseFieldValue;
                        }
                    }
                }
            }
            PopulateContactNameFields(mappings);
            PopulateIds(mappings);
            return response;
        }

        private void PopulateIds(Dictionary<IMapSourceImport, IEnumerable<IRecord>> mappings)
        {
            foreach (var mapping in mappings)
            {
                var primaryKeyField = XrmRecordService.GetPrimaryKey(mapping.Key.TargetType);
                if (primaryKeyField != null)
                {
                    foreach (var record in mapping.Value)
                    {
                        var primarykey = record.GetIdField(primaryKeyField);
                        record.Id = record.GetIdField(primaryKeyField);
                    }
                }
            }
        }

        private MappedImportValidationResponse TransformMappingIntoTarget(Dictionary<IMapSourceImport, IEnumerable<IRecord>> mappings, LogController logController, bool ignoreNullValues = false)
        {
            var response = new MappedImportValidationResponse();
            foreach (var mapping in mappings.Keys.ToArray())
            {
                var sourceRecords = new List<IRecord>(mappings[mapping]);
                var targetRecords = new List<IRecord>();
                mappings[mapping] = targetRecords;

                var nNRelationshipEntityNames = XrmRecordService
                    .GetManyToManyRelationships()
                    .Select(m => m.IntersectEntityName)
                    .ToArray();
                var targetType = mapping.TargetType;
                var isNnRelation = nNRelationshipEntityNames.Contains(targetType);

                var areMappingErrors = false;
                if (!isNnRelation && !XrmRecordService.RecordTypeExists(targetType))
                {
                    response.AddResponseItem(new MappedImportValidationResponse.MappedImportValidationResponseError(null, targetType, null, null, null, "Record Type Does Not Exist", null));
                    areMappingErrors = true;
                }
                if (!areMappingErrors && !isNnRelation)
                {
                    foreach (var fieldMapping in mapping.FieldMappings)
                    {
                        var targetField = fieldMapping.TargetField;
                        if (!XrmRecordService.FieldExists(targetField, targetType))
                        {
                            response.AddResponseItem(new MappedImportValidationResponse.MappedImportValidationResponseError(null, targetType, targetField, null, null, "Field Does Not Exist", null));
                            areMappingErrors = true;
                        }
                    }
                }
                if (areMappingErrors)
                {
                    return response;
                }
                var rowCount = sourceRecords.Count();

                // Precompute mapping info once per mapping to avoid repeated service calls inside the row loop
                var mappingInfos = (mapping.FieldMappings ?? Enumerable.Empty<IMapSourceField>())
                    .Select(fm => new
                    {
                        TargetField = fm.TargetField,
                        SourceField = fm.SourceField,
                        UseAltMatch = fm.UseAltMatchField,
                        AltMatchFieldType = fm.AltMatchFieldType,
                        IsLookup = fm.TargetField != null && XrmRecordService.IsLookup(fm.TargetField, targetType),
                        LookupTargetType = fm.TargetField == null ? null : (fm.UseAltMatchField ? fm.AltMatchFieldType : XrmRecordService.GetLookupTargetType(fm.TargetField, targetType)),
                        ParseFunc = (Func<string, object>)(s => XrmRecordService.ParseField(fm.TargetField, targetType, s))
                    })
                    .ToArray();

                // Snapshot rows and process them in parallel. Results kept in array to preserve order.
                var rows = sourceRecords.ToList();
                var results = new IRecord[rows.Count];
                var parseErrors = new ConcurrentBag<MappedImportValidationResponse.MappedImportValidationResponseError>();
                var processed = 0;

                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1) };

                Parallel.ForEach(rows.Select((r, i) => new { Row = r, Index = i }), parallelOptions, item =>
                {
                    var row = item.Row;
                    var idx = item.Index;
                    var newRecord = XrmRecordService.NewRecord(targetType);
                    newRecord.Id = row.Id;
                    try
                    {
                        var hasFieldValue = false;
                        var fieldValues = new Dictionary<string, object>();

                        foreach (var fm in mappingInfos)
                        {
                            var targetField = fm.TargetField;
                            if (targetField == null)
                                continue;

                            var objectValue = row.GetField(fm.SourceField);
                            var stringValue = row.GetStringField(fm.SourceField)?.Trim();

                            if (!stringValue.IsNullOrWhiteSpace())
                                hasFieldValue = true;

                            if (isNnRelation)
                            {
                                fieldValues[targetField] = stringValue;
                                continue;
                            }

                            if (fm.IsLookup)
                            {
                                if (objectValue is Lookup lk)
                                {
                                    var lktype = string.IsNullOrWhiteSpace(lk.RecordType) ? fm.LookupTargetType : lk.RecordType;
                                    fieldValues[targetField] = new Lookup(lktype, lk.Id, stringValue);
                                }
                                else if (!string.IsNullOrWhiteSpace(stringValue))
                                {
                                    var lookupTargetType = fm.UseAltMatch ? fm.AltMatchFieldType : fm.LookupTargetType;
                                    if (Guid.TryParse(stringValue, out Guid isGuid))
                                    {
                                        fieldValues[targetField] = new Lookup(lookupTargetType, isGuid.ToString(), stringValue);
                                    }
                                    else
                                    {
                                        fieldValues[targetField] = new Lookup(lookupTargetType, null, stringValue);
                                    }
                                }
                                else
                                {
                                    fieldValues[targetField] = null;
                                }
                            }
                            else
                            {
                                try
                                {
                                    fieldValues[targetField] = fm.ParseFunc(stringValue);
                                }
                                catch (Exception ex)
                                {
                                    parseErrors.Add(new MappedImportValidationResponse.MappedImportValidationResponseError(idx + 1, targetType, targetField, null, stringValue, "Error Parsing Field - " + ex.Message, ex));
                                }
                            }
                        }

                        foreach (var fieldValue in fieldValues)
                        {
                            if (!ignoreNullValues || fieldValue.Value != null)
                            {
                                newRecord[fieldValue.Key] = fieldValue.Value;
                            }
                        }
                        if (!hasFieldValue)
                        {
                            // no useful data
                            results[idx] = null;
                        }
                        else
                        {
                            results[idx] = newRecord;
                        }
                    }
                    catch (Exception ex)
                    {
                        parseErrors.Add(new MappedImportValidationResponse.MappedImportValidationResponseError("Unknown Mapping Error", ex));
                    }

                    var proc = System.Threading.Interlocked.Increment(ref processed);
                    if (proc % 1000 == 0 || proc == rows.Count)
                    {
                        logController.LogLiteral($"Mapping {targetType} Data Into Records {proc}/{rows.Count}");
                    }
                });

                // Add parse errors to response
                if (parseErrors.Count > 0)
                    response.AddResponseItems(parseErrors);

                // Post-process deduplication (outer loop): preserve original order and apply duplicates removal
                for (var i = 0; i < results.Length; i++)
                {
                    var newRecord = results[i];
                    if (newRecord == null)
                        continue;

                    if (mapping.IgnoreDuplicates)
                    {
                        if (targetRecords.Any(r => r.GetFieldsInEntity().Except(new[] { "Sheet.RowNumber" }).All(f =>
                        {
                            var fieldValue1 = r.GetField(f);
                            var fieldValue2 = newRecord.GetField(f);
                            if (fieldValue1 is Lookup && fieldValue2 is Lookup)
                            {
                                return ((Lookup)fieldValue1).Name == ((Lookup)fieldValue2).Name;
                            }
                            else
                                return XrmRecordService.FieldsEqual(fieldValue1, fieldValue2);
                        })))
                        {
                            continue;
                        }
                    }
                    targetRecords.Add(newRecord);
                }
            }
            return response;
        }

        private void PopulateContactNameFields(Dictionary<IMapSourceImport, IEnumerable<IRecord>> mappings)
        {
            foreach (var mapping in mappings)
            {
                if (mapping.Key.TargetType == Entities.contact)
                {
                    foreach (var contact in mapping.Value)
                    {
                        if (contact.ContainsField(Fields.contact_.fullname)
                            && !contact.ContainsField(Fields.contact_.firstname)
                            && !contact.ContainsField(Fields.contact_.lastname))
                        {
                            //okay for these dudes lets split their name into first and last name somehow
                            var name = contact.GetStringField(Fields.contact_.fullname);
                            if (name != null)
                            {
                                name = name.Trim();
                                var lastSpaceIndex = name.LastIndexOf(" ");
                                if (lastSpaceIndex == -1)
                                {
                                    contact.SetField(Fields.contact_.firstname, name, XrmRecordService);
                                }
                                else
                                {
                                    contact.SetField(Fields.contact_.firstname, name.Substring(0, lastSpaceIndex), XrmRecordService);
                                    contact.SetField(Fields.contact_.lastname, name.Substring(lastSpaceIndex + 1), XrmRecordService);
                                }
                            }
                        }
                        if (!contact.ContainsField(Fields.contact_.fullname)
                            && (contact.ContainsField(Fields.contact_.firstname)
                                || contact.ContainsField(Fields.contact_.lastname)))
                        {
                            //okay for these dudes lets split their name into first and last name somehow
                            var name = contact.GetStringField(Fields.contact_.firstname) + " " + contact.GetStringField(Fields.contact_.lastname);
                            if (name != null)
                            {
                                name = name.Trim();
                                contact.SetField(Fields.contact_.fullname, name, XrmRecordService);
                            }
                        }
                    }
                }
            }
        }
    }
}
