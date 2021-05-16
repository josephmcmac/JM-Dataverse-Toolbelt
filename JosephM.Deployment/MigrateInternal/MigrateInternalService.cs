using JosephM.Application.Desktop.Module.Crud.BulkCopyFieldValue;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Deployment.SpreadsheetImport;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Sql;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment.MigrateInternal
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
            var importService = new SourceImportService(XrmRecordService);
            var responseItems = importService.DoImport(dictionary, false, request.MatchRecordsByName, false, controller, executeMultipleSetSize: request.ExecuteMultipleSetSize, targetCacheLimit: request.TargetCacheLimit);
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
                var queryRows = XrmRecordService.RetrieveAll(sourceMapping.SourceType.Key, null);
                dictionary.Add(sourceMapping, queryRows);
            }

            return dictionary;
        }
    }
}