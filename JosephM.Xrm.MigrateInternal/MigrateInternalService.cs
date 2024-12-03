using JosephM.Application.Desktop.Module.Crud.BulkCopyFieldValue;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.DataImportExport.MappedImport;
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
            controller.Controller.UpdateProgress(0, 1, "Loading Records For Import");
            var dictionary = LoadMappingDictionary(request, controller.Controller);
            
            //migrate internal data
            var importService = new MappedImportService(XrmRecordService);
            var responseItems = importService.DoImport(dictionary, false, request.MatchRecordsByName, false, controller, executeMultipleSetSize: request.ExecuteMultipleSetSize, targetCacheLimit: request.TargetCacheLimit, forceSubmitAllFields: request.SubmitUnchangedFields, parallelImportProcessCount: request.ParallelImportProcessCount ?? 1);
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

        public Dictionary<IMapSourceImport, IEnumerable<IRecord>> LoadMappingDictionary(MigrateInternalRequest request, LogController logController)
        {
            var dictionary = new Dictionary<IMapSourceImport, IEnumerable<IRecord>>();

            var toIterate = request.TypesToMigrate.Where(tm => tm.TargetType != null).ToArray();
            var countToDo = toIterate.Count();
            var countDone = 0;
            foreach (var sourceMapping in toIterate)
            {
                logController.LogLiteral($"Reading Source Data {++countDone}/{countToDo} {sourceMapping.SourceType.Key}");
                IEnumerable<IRecord> records = new IRecord[0];
                var requiredFields = new List<string>();
                if(sourceMapping.FieldMappings != null)
                {
                    requiredFields.AddRange(sourceMapping.FieldMappings.Select(k => k.SourceField.Key));
                }
                var primaryKey = XrmRecordService.GetPrimaryKey(sourceMapping.SourceType.Key);
                if(!string.IsNullOrWhiteSpace(primaryKey))
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
                    case SourceDatasetType.AllRecords:
                        {
                            records = XrmRecordService.RetrieveAll(sourceMapping.SourceType.Key, requiredFields);
                            break;
                        }
                    case SourceDatasetType.FetchXml:
                        {
                            var queryExpression = XrmRecordService.XrmService.ConvertFetchToQueryExpression(sourceMapping.FetchXml);
                            queryExpression.ColumnSet = new ColumnSet(requiredFields.ToArray());
                            var temp = queryExpression.PageInfo != null && queryExpression.PageInfo.Count > 0
                                ? XrmRecordService.XrmService.RetrieveFirstX(queryExpression, queryExpression.PageInfo.Count)
                                : XrmRecordService.XrmService.RetrieveAll(queryExpression);
                            records = XrmRecordService.ToIRecords(temp);
                            break;
                        }
                    case SourceDatasetType.SpecificRecords:
                        {
                            var ids = sourceMapping.SpecificRecordsToExport == null
                                ? new HashSet<string>()
                                : new HashSet<string>(sourceMapping.SpecificRecordsToExport
                                    .Select(r => r.Record == null ? null : r.Record.Id)
                                    .Where(s => !s.IsNullOrWhiteSpace()).Distinct());
                            records = ids.Any()
                                ? XrmRecordService.RetrieveAllOrClauses(sourceMapping.SourceType.Key,
                                    ids.Select(
                                        i => new Condition(primaryKey, ConditionType.Equal, new Guid(i))), requiredFields)
                                : new IRecord[0];
                            records = records.Where(e => ids.Contains(e.Id.ToString())).ToArray();
                            break;
                        }
                    default:
                        {
                            records = XrmRecordService.RetrieveAll(sourceMapping.SourceType.Key, requiredFields);
                            break;
                        }
                }
                if(!request.RetainPrimaryKey)
                {
                    foreach(var record in records)
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