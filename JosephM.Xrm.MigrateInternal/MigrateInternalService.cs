using JosephM.Application.Desktop.Module.Crud.BulkCopyFieldValue;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.DataImportExport.MappedImport;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.MigrateInternal
{
    public class MigrateInternalService :
        ServiceBase<MigrateInternalRequest, MigrateInternalResponse, MigrateInternalResponseItem>
    {
        public MigrateInternalService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public XrmRecordService XrmRecordService { get; }

        public override void ExecuteExtention(MigrateInternalRequest request, MigrateInternalResponse response,
            ServiceRequestController controller)
        {
            var dictionary = request.UnloadMappingDictionary();
            var importService = new MappedImportService(XrmRecordService);
            var responseItems = importService.DoImport(dictionary, request.UnloadValidationResponse(), false, request.MatchRecordsByName, false, controller, executeMultipleSetSize: request.ExecuteMultipleSetSize, targetCacheLimit: request.TargetCacheLimit, forceSubmitAllFields: request.SubmitUnchangedFields, parallelImportProcessCount: request.ParallelImportProcessCount ?? 1, bypassWorkflowsAndPlugins: request.BypassFlowsPluginsAndWorkflows, trustSourceLookupGuids: true);
            response.LoadSpreadsheetImport(responseItems);
            //copy lookup fields
            if (request.ReferenceFieldReplacements != null)
            {
                var dictionaryIt = new Dictionary<MigrateInternalRequest.ReferenceFieldsForCopy, MigratedLookupField>();
                foreach(var referenceFieldToCopy in request.ReferenceFieldReplacements)
                {
                    var migratedLookupField = new MigratedLookupField(new BulkCopyFieldValueResponse());
                    migratedLookupField.EntityType = referenceFieldToCopy.ReferencingType.Value;
                    migratedLookupField.SourceField = referenceFieldToCopy.OldField.Value;
                    migratedLookupField.TargetField = referenceFieldToCopy.NewField.Value;
                    dictionaryIt.Add(referenceFieldToCopy, migratedLookupField);
                }
                var migratingLookupFields = new MigratingLookupFields(dictionaryIt.Values);
                controller.AddObjectToUi(migratingLookupFields);

                var bulkCopyService = new BulkCopyFieldValueService(XrmRecordService);
                foreach(var referenceFieldToCopy in request.ReferenceFieldReplacements)
                {
                    controller.LogLiteral($"Loading records for field {referenceFieldToCopy.OldField.Value} on type {referenceFieldToCopy.ReferencingType.Value}");

                    var records = XrmRecordService.RetrieveAllAndClauses(referenceFieldToCopy.ReferencingType.Key,
                        new[]
                        {
                            new Condition(referenceFieldToCopy.OldField.Key, ConditionType.NotNull)
                        },
                        new [] { referenceFieldToCopy.OldField.Key, referenceFieldToCopy.NewField.Key, XrmRecordService.GetPrimaryKey(referenceFieldToCopy.ReferencingType.Key) });
                    
                    var bulkCopyRequest = new BulkCopyFieldValueRequest(referenceFieldToCopy.ReferencingType, records)
                    {
                        SourceField = referenceFieldToCopy.OldField,
                        TargetField = referenceFieldToCopy.NewField,
                        CopyIfNull = false,
                        AllowExecuteMultiples = true,
                        ExecuteMultipleSetSize = 25,
                        OverwriteIfPopulated = false
                    };

                    var temp = dictionaryIt[referenceFieldToCopy];
                    temp.CountToProcess = records.Count();
                    bulkCopyService.ExecuteExtention(bulkCopyRequest, temp.GetInternalResponse(), controller);
                    response.LoadBulkCopy(temp);
                }
            }
            response.Message = "The Import Process Has Completed";
        }

        public Dictionary<IMapSourceImport, IEnumerable<IRecord>> LoadMappingDictionaryWithSourceData(MigrateInternalRequest request, LogController logController)
        {
            var dictionary = new Dictionary<IMapSourceImport, IEnumerable<IRecord>>();

            var toIterate = request.TypesToMigrate.Where(tm => tm.TargetType != null).ToArray();
            var countToDo = toIterate.Count();
            var countDone = 0;
            foreach (var sourceMapping in toIterate)
            {
                logController.LogLiteral($"Reading Source Data {countDone}/{countToDo} {sourceMapping.SourceType.Key}");
                var records = new List<IRecord>();
                var requiredFields = new List<string>();
                if (sourceMapping.FieldMappings != null)
                {
                    requiredFields.AddRange(sourceMapping.FieldMappings.Select(k => k.SourceField.Key));
                }
                var primaryKey = XrmRecordService.GetPrimaryKey(sourceMapping.SourceType.Key);
                if (!string.IsNullOrWhiteSpace(primaryKey))
                {
                    requiredFields.Add(primaryKey);
                }
                var primaryField = XrmRecordService.GetPrimaryField(sourceMapping.SourceType.Key);
                if (!string.IsNullOrWhiteSpace(primaryField))
                {
                    requiredFields.Add(primaryField);
                }
                requiredFields = requiredFields.Distinct().ToList();
                switch (sourceMapping.SourceDatasetType)
                {
                    case SourceDatasetType.FetchXml:
                        {
                            var queryExpression = XrmRecordService.XrmService.ConvertFetchToQueryExpression(sourceMapping.FetchXml);
                            queryExpression.ColumnSet = new ColumnSet(requiredFields.ToArray());
                            if (queryExpression.PageInfo != null && queryExpression.PageInfo.Count > 0)
                            {
                                records.AddRange(XrmRecordService.XrmService.RetrieveFirstX(queryExpression, queryExpression.PageInfo.Count).Select(XrmRecordService.ToIRecord));
                            }
                            else
                            {
                                XrmRecordService.XrmService.ProcessQueryResults(queryExpression, (IEnumerable<Entity> ie) =>
                                {
                                    records.AddRange(ie.Select(XrmRecordService.ToIRecord));
                                    logController.LogLiteral($"Reading Source Data {countDone}/{countToDo} {sourceMapping.SourceType.Key} Count={records.Count}");
                                    return true;
                                });
                            }
                            break;
                        }
                    case SourceDatasetType.SpecificRecords:
                        {
                            var ids = sourceMapping.SpecificRecordsToExport == null
                                ? new HashSet<string>()
                                : new HashSet<string>(sourceMapping.SpecificRecordsToExport
                                    .Select(r => r.Record == null ? null : r.Record.Id)
                                    .Where(s => !s.IsNullOrWhiteSpace()).Distinct());
                            if (ids.Any())
                            {
                                var temp = XrmRecordService.RetrieveAllOrClauses(sourceMapping.SourceType.Key,
                                    ids.Select(
                                        i => new Condition(primaryKey, ConditionType.Equal, new Guid(i))), requiredFields);

                                records.AddRange(temp.Where(e => ids.Contains(e.Id.ToString())));
                            }
                            break;
                        }
                    default:
                        {
                            var queryExpression = new QueryExpression(sourceMapping.SourceType.Key)
                            {
                                ColumnSet = new ColumnSet(requiredFields.ToArray())
                            };
                            XrmRecordService.XrmService.ProcessQueryResults(queryExpression, (IEnumerable<Entity> ie) =>
                            {
                                records.AddRange(ie.Select(XrmRecordService.ToIRecord));
                                logController.LogLiteral($"Reading Source Data {countDone}/{countToDo} {sourceMapping.SourceType.Key} Count={records.Count}");
                                return true;
                            });
                            break;
                        }
                }
                if (!request.RetainPrimaryKey)
                {
                    foreach (var record in records)
                    {
                        record.Id = null;
                    }
                }
                dictionary.Add(sourceMapping, records);
            }

            return dictionary;
        }
    }
}