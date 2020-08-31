using JosephM.Application.Application;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Deployment.DataImport;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JosephM.Deployment.SpreadsheetImport
{
    public class SpreadsheetImportService
    {
        public SpreadsheetImportService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public XrmRecordService XrmRecordService { get; }
        public IApplicationController ApplicationController { get; }

        public SpreadsheetImportResponse DoImport(Dictionary<IMapSpreadsheetImport, IEnumerable<IRecord>> mappings, bool maskEmails, bool matchByName, bool updateOnly, ServiceRequestController controller, int? executeMultipleSetSize = null, bool useAmericanDates = false, int? targetCacheLimit = null)
        {
            var response = new SpreadsheetImportResponse();
            var parseResponse = ParseIntoEntities(mappings, controller.Controller, useAmericanDates: useAmericanDates);
            response.LoadParseResponse(parseResponse);
            var dataImportService = new DataImportService(XrmRecordService);
            var matchKeyDictionary = new Dictionary<string, IEnumerable<string>>();
            foreach(var map in mappings.Keys)
            {
                if(map.AltMatchKeys != null && map.AltMatchKeys.Any())
                {
                    if (matchKeyDictionary.ContainsKey(map.TargetType))
                        throw new NotSupportedException($"Error Type {map.TargetType} Is Defined With Multiple Match Keys");
                    matchKeyDictionary.Add(map.TargetType, map.AltMatchKeys.Select(mk => mk.TargetField).ToArray());
                }
            }
            response.LoadDataImport(dataImportService.DoImport(parseResponse.GetParsedEntities(), controller, maskEmails, matchOption: matchByName ? MatchOption.PrimaryKeyThenName : MatchOption.PrimaryKeyOnly, loadExistingErrorsIntoSummary: response.ResponseItems, altMatchKeyDictionary: matchKeyDictionary, updateOnly: updateOnly, includeOwner: true, containsExportedConfigFields: false, executeMultipleSetSize: executeMultipleSetSize, targetCacheLimit: targetCacheLimit));
            return response;
        }

        public ParseIntoEntitiesResponse ParseIntoEntities(Dictionary<IMapSpreadsheetImport, IEnumerable<IRecord>> mappings, LogController logController, bool useAmericanDates = false)
        {
            var response = new ParseIntoEntitiesResponse();
            foreach (var mapping in mappings)
            {
                response.AddEntities(MapToEntities(mapping.Value, mapping.Key, response, logController, useAmericanDates));
            }
            var entities = response.GetParsedEntities();
            PopulateEmptyNameFields(entities);
            PopulateIds(entities);
            return response;
        }

        private void PopulateIds(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                var primaryKeyField = XrmRecordService.GetPrimaryKey(entity.LogicalName);
                if(primaryKeyField != null)
                {
                    var primarykey = entity.GetGuidField(primaryKeyField);
                    entity.Id = primarykey;
                }
            }
        }

        private IEnumerable<Entity> MapToEntities(IEnumerable<IRecord> queryRows, IMapSpreadsheetImport mapping, ParseIntoEntitiesResponse response, LogController logController, bool useAmericanDates)
        {
            var result = new List<Entity>();

            var nNRelationshipEntityNames = XrmRecordService
                .GetManyToManyRelationships()
                .Select(m => m.IntersectEntityName)
                .ToArray();

            var rowNumber = 0;
            var rowCount = queryRows.Count();
            foreach (var row in queryRows)
            {           
                rowNumber++;
                var targetType = mapping.TargetType;
                logController.LogLiteral($"Mapping {targetType} Data Into Records {rowNumber}/{rowCount}");
                try
                {
                    var isNnRelation = nNRelationshipEntityNames.Contains(targetType);

                    var hasFieldValue = false;
                    var fieldValues = new ConcurrentDictionary<string, object>();
                    //this is used in the import to output the row number
                    //if the import throws an error
                    Parallel.ForEach(mapping.FieldMappings, (fieldMapping) => 
                    //foreach (var fieldMapping in mapping.FieldMappings)
                    {
                        var targetField = fieldMapping.TargetField;
                        if (fieldMapping.TargetField != null)
                        {
                            var stringValue = row.GetStringField(fieldMapping.SourceField);
                            if (stringValue != null)
                                stringValue = stringValue.Trim();

                            if(!stringValue.IsNullOrWhiteSpace())
                            {
                                hasFieldValue = true;
                            }
                            if (isNnRelation)
                            {
                                //bit of hack
                                //for csv relationships just set to a string and map it later
                                //as the referenced record may not be created yet
                                fieldValues[targetField] = stringValue;
                            }
                            else if (XrmRecordService.XrmService.IsLookup(targetField, targetType))
                            {
                                //for lookups am going to set to a empty guid and allow the import part to replace with a correct guid
                                if (!stringValue.IsNullOrWhiteSpace())
                                {
                                    var isGuid = Guid.Empty;
                                    if (Guid.TryParse(stringValue, out isGuid))
                                    {
                                        fieldValues[targetField] =
                                            new EntityReference(XrmRecordService.XrmService.GetLookupTargetEntity(targetField, targetType),
                                                isGuid)
                                            {
                                                Name = stringValue
                                            };
                                    }
                                    else
                                    {
                                        fieldValues[targetField] =
                                            new EntityReference(XrmRecordService.XrmService.GetLookupTargetEntity(targetField, targetType),
                                                Guid.Empty)
                                            {
                                                Name = stringValue
                                            };
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    fieldValues[targetField] = XrmRecordService.XrmService.ParseField(targetField, targetType, stringValue, useAmericanDates);
                                }
                                catch (Exception ex)
                                {
                                    response.AddResponseItem(new ParseIntoEntitiesResponse.ParseIntoEntitiesError(rowNumber, targetType, targetField, null, stringValue, "Error Parsing Field - " + ex.Message, ex));
                                }
                            }
                        }
                    });
                    var entity = new Entity(targetType);
                    foreach (var fieldValue in fieldValues)
                    {
                        entity[fieldValue.Key] = fieldValues[fieldValue.Key];
                    }
                    if(!hasFieldValue)
                    {
                        //ignore any where all fields emopty
                        continue;
                    }
                    //okay if remove duplicates
                    //any which are exact duplicates to previous ones lets ignore
                    if (mapping.IgnoreDuplicates)
                    {
                        if (result.Any(r => r.GetFieldsInEntity().Except(new[] { "Sheet.RowNumber" }).All(f =>
                        {
                        //since for entity references we may load the name with empty guid
                        //check the display name for them
                        var fieldValue1 = r.GetField(f);
                            var fieldValue2 = entity.GetField(f);
                            if (fieldValue1 is EntityReference && fieldValue2 is EntityReference)
                            {
                                return ((EntityReference)fieldValue1).Name == ((EntityReference)fieldValue2).Name;
                            }
                            else
                                return XrmRecordService.FieldsEqual(fieldValue1, fieldValue2);
                        })))
                        {
                            continue;
                        }
                    }
                    result.Add(entity);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(new ParseIntoEntitiesResponse.ParseIntoEntitiesError("Mapping Error", ex));
                }
            }
            return result;
        }

        private void PopulateEmptyNameFields(IEnumerable<Entity> entities)
        {
            foreach (var contact in entities.Where(e => e.LogicalName == Entities.contact))
            {
                if (contact.Contains(Fields.contact_.fullname)
                    && !contact.Contains(Fields.contact_.firstname)
                    && !contact.Contains(Fields.contact_.lastname))
                {
                    //okay for these dudes lets split their name into first and last name somehow
                    var name = contact.GetStringField(Fields.contact_.fullname);
                    if (name != null)
                    {
                        name = name.Trim();
                        var lastSpaceIndex = name.LastIndexOf(" ");
                        if (lastSpaceIndex == -1)
                        {
                            contact.SetField(Fields.contact_.firstname, name);
                        }
                        else
                        {
                            contact.SetField(Fields.contact_.firstname, name.Substring(0, lastSpaceIndex));
                            contact.SetField(Fields.contact_.lastname, name.Substring(lastSpaceIndex + 1));
                        }
                    }
                }
                if (!contact.Contains(Fields.contact_.fullname)
                    && (contact.Contains(Fields.contact_.firstname)
                        || contact.Contains(Fields.contact_.lastname)))
                {
                    //okay for these dudes lets split their name into first and last name somehow
                    var name = contact.GetStringField(Fields.contact_.firstname) + " " + contact.GetStringField(Fields.contact_.lastname);
                    if (name != null)
                    {
                        name = name.Trim();
                        contact.SetField(Fields.contact_.fullname, name);
                    }
                }
            }
        }
    }
}
