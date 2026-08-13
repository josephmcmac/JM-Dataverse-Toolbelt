using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Utility;
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
    public class DataImportService
    {
        public DataImportService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public XrmRecordService XrmRecordService { get; set; }

        public DataImportResponse DoImport(IEnumerable<IRecord> records, ServiceRequestController controller, bool maskEmails, MatchOption matchOption = MatchOption.PrimaryKeyThenName, IEnumerable<DataImportResponseItem> loadExistingErrorsIntoSummary = null, Dictionary<string, IEnumerable<KeyValuePair<string, bool>>> altMatchKeyDictionary = null, Dictionary<string, Dictionary<string, KeyValuePair<string, string>>> altLookupMatchKeyDictionary = null, bool updateOnly = false, bool includeOwner = false, bool includeOverrideCreatedOn = false, bool containsExportedConfigFields = true, int? executeMultipleSetSize = null, int? targetCacheLimit = null, bool onlyFieldMatchActive = false, bool forceSubmitAllFields = false, bool displayTimeEstimations = false, int parallelImportProcessCount = 1, bool bypassWorkflowsAndPlugins = false, bool trustSourceLookupGuids = false) 
        {
            var response = new DataImportResponse(records, loadExistingErrorsIntoSummary);
            controller.AddObjectToUi(response);
            try
            {
                controller.LogLiteral("Preparing Import");
                var dataImportContainer = new DataImportContainer(response,
                    XrmRecordService,
                    altMatchKeyDictionary ?? new Dictionary<string, IEnumerable<KeyValuePair<string, bool>>>(),
                    altLookupMatchKeyDictionary ?? new Dictionary<string, Dictionary<string, KeyValuePair<string, string>>>(),
                    records,
                    controller,
                    includeOwner,
                    includeOverrideCreatedOn,
                    maskEmails,
                    matchOption,
                    updateOnly,
                    containsExportedConfigFields,
                    executeMultipleSetSize ?? 1,
                    targetCacheLimit ?? 1000,
                    onlyFieldMatchActive,
                    forceSubmitAllFields,
                    displayTimeEstimations,
                    parallelImportProcessCount,
                    bypassWorkflowsAndPlugins,
                    trustSourceLookupGuids);

                ImportEntities(dataImportContainer);

                RetryUnresolvedFields(dataImportContainer);

                ImportAssociations(dataImportContainer);
            }
            finally
            {
                controller.RemoveObjectFromUi(response);
            }
            return response;
        }

        private void ImportAssociations(DataImportContainer dataImportContainer)
        {
            var countToImport = dataImportContainer.AssociationTypesToImport.Count();
            var countImported = 0;
            foreach (var relationshipEntityName in dataImportContainer.AssociationTypesToImport)
            {
                var thisEntityName = relationshipEntityName;

                var relationship = XrmRecordService.GetRelationshipMetadataForEntityName(thisEntityName);
                var type1 = relationship.RecordType1;
                var field1 = relationship.Entity1IntersectAttribute;
                var type2 = relationship.RecordType2;
                var field2 = relationship.Entity2IntersectAttribute;

                dataImportContainer.Controller.UpdateProgress(countImported++, countToImport, $"Associating {thisEntityName} Records");
                dataImportContainer.Controller.UpdateLevel2Progress(0, 1, "Loading");
                var thisTypeEntities = dataImportContainer.EntitiesToImport.Where(e => e.Type == thisEntityName).ToList();
                var countRecordsToImport = thisTypeEntities.Count;
                var countRecordsImported = 0;
                var estimator = new TaskEstimator(countRecordsToImport);

                while (thisTypeEntities.Any())
                {
                    var thisSetOfEntities = thisTypeEntities
                        .Take(dataImportContainer.ExecuteMultipleSetSize)
                        .ToList();
                    var countThisSet = thisSetOfEntities.Count;

                    thisTypeEntities.RemoveRange(0, thisSetOfEntities.Count());

                    var copiesForAssociate = new List<IRecord>();

                    foreach (var thisEntity in thisSetOfEntities)
                    {
                        try
                        {

                            //bit of hack
                            //when importing from csv just set the fields to the string name of the referenced record
                            //so either string when csv or guid when xml import/export
                            string matchType1 = type1;
                            string matchField1 = XrmRecordService.GetPrimaryField(type1);
                            if (dataImportContainer.AltLookupMatchKeyDictionary.ContainsKey(thisEntity.Type)
                                && dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.Type].ContainsKey(relationship.Entity1IntersectAttribute))
                            {
                                matchType1 = dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.Type][relationship.Entity1IntersectAttribute].Key;
                                matchField1 = dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.Type][relationship.Entity1IntersectAttribute].Value;
                            }
                            var value1 = thisEntity.GetField(relationship.Entity1IntersectAttribute);
                            var id1 = value1 is string
                                ? dataImportContainer.GetUniqueMatchingEntity(matchType1, matchField1, (string)value1).Id.ToString()
                                : thisEntity.GetIdField(relationship.Entity1IntersectAttribute);

                            string matchType2 = type2;
                            string matchField2 = XrmRecordService.GetPrimaryField(type2);
                            if (dataImportContainer.AltLookupMatchKeyDictionary.ContainsKey(thisEntity.Type)
                                && dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.Type].ContainsKey(relationship.Entity2IntersectAttribute))
                            {
                                matchType2 = dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.Type][relationship.Entity2IntersectAttribute].Key;
                                matchField2 = dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.Type][relationship.Entity2IntersectAttribute].Value;
                            }
                            var value2 = thisEntity.GetField(relationship.Entity2IntersectAttribute);
                            var id2 = value2 is string
                                ? dataImportContainer.GetUniqueMatchingEntity(matchType2, matchField2, (string)value2).Id.ToString()
                                : thisEntity.GetIdField(relationship.Entity2IntersectAttribute);

                            //add a where field lookup reference then look it up
                            if (dataImportContainer.IdSwitches.ContainsKey(type1) && dataImportContainer.IdSwitches[type1].ContainsKey(id1))
                                id1 = dataImportContainer.IdSwitches[type1][id1];
                            if (dataImportContainer.IdSwitches.ContainsKey(type2) && dataImportContainer.IdSwitches[type2].ContainsKey(id2))
                                id2 = dataImportContainer.IdSwitches[type2][id2];

                            var copyForAssociate = XrmRecordService.NewRecord(thisEntity.Type);
                            copyForAssociate.Id = thisEntity.Id;
                            copyForAssociate.SetField(field1, id1, XrmRecordService);
                            copyForAssociate.SetField(field2, id2, XrmRecordService);
                            copiesForAssociate.Add(copyForAssociate);
                        }
                        catch (Exception ex)
                        {
                            dataImportContainer.LogAssociationError(thisEntity, ex);
                        }
                        countRecordsImported++;
                        dataImportContainer.Controller.UpdateLevel2Progress(countRecordsImported, countRecordsToImport, estimator.GetProgressString(countRecordsImported));
                    }

                    var existingAssociationsQueries = copiesForAssociate.
                        Select(c =>
                        {
                            var q = new QueryDefinition(relationship.IntersectEntityName);
                            q.RootFilter.AddCondition(field1, ConditionType.Equal, c.GetIdField(field1));
                            q.RootFilter.AddCondition(field2, ConditionType.Equal, c.GetIdField(field2));
                            return q;
                        })
                        .ToArray();
                    var executeQueryResponses = XrmRecordService.ExecuteMultipleQueries(existingAssociationsQueries);

                    var notYetAssociated = new List<IRecord>();
                    var i = 0;
                    foreach (var queryResponse in executeQueryResponses)
                    {
                        var associationEntity = copiesForAssociate[i];
                        if (queryResponse.Exception != null)
                        {
                            dataImportContainer.LogAssociationError(associationEntity, queryResponse.Exception);
                        }
                        else if (!queryResponse.Records.Any())
                        {
                            notYetAssociated.Add(associationEntity);
                        }
                        else
                        {
                            associationEntity.Id = Guid.NewGuid().ToString();
                            dataImportContainer.Response.AddSkippedNoChange(associationEntity);
                        }
                        i++;
                    }

                    var is1Referencing = relationship.Entity1IntersectAttribute == field1;
                    var associateMultipleResponses = XrmRecordService.AssociateMultiple(relationship.IntersectEntityName, type1, notYetAssociated.Select(a => a.GetIdField(field1)), type2, notYetAssociated.Select(a => a.GetIdField(field2)), is1Referencing);

                    for (var j = 0; j < notYetAssociated.Count(); j++)
                    {
                        var associationEntity = notYetAssociated[j];
                        if (associateMultipleResponses.ContainsKey(j) && associateMultipleResponses[j] != null)
                        {
                            dataImportContainer.LogAssociationError(associationEntity, associateMultipleResponses[j]);
                        }
                        else
                        {
                            associationEntity.Id = Guid.NewGuid().ToString();
                            dataImportContainer.Response.AddCreated(associationEntity);
                        }
                        i++;
                    }
                }
            }
        }

        private void RetryUnresolvedFields(DataImportContainer dataImportContainer)
        {
            var countToImport = dataImportContainer.FieldsToRetry.Count;
            var countImported = 0;
            var estimator = new TaskEstimator(countToImport);

            dataImportContainer.Controller.UpdateProgress(countImported, countToImport, "Retrying Unresolved Fields");

            var types = dataImportContainer.FieldsToRetry.Keys.Select(e => e.Type).Distinct().ToArray();

            foreach(var type in types)
            {
                var thisTypeForRetry = dataImportContainer.FieldsToRetry.Where(kv => kv.Key.Type == type).ToList();

                while (thisTypeForRetry.Any())
                {
                    var thisSetOfEntities = thisTypeForRetry
                        .Take(dataImportContainer.ExecuteMultipleSetSize)
                        .ToList();
                    var countThisSet = thisSetOfEntities.Count;
                    thisTypeForRetry.RemoveRange(0, countThisSet);

                    var distinctFields = thisSetOfEntities.SelectMany(kv => kv.Value).Distinct().ToArray();

                    var indexToUpdateCopy = new Dictionary<IRecord, IRecord>();
                    foreach (var kv in thisSetOfEntities)
                    {
                        indexToUpdateCopy.Add(kv.Key, XrmRecordService.NewRecord(kv.Key.Type, kv.Key.Id));
                    }

                    foreach (var field in distinctFields)
                    {
                        var itemsWithThisFieldPopulated = thisSetOfEntities
                            .Where(e => dataImportContainer.FieldsToRetry[e.Key].Contains(field))
                            .Select(e => e.Key)
                            .ToList();
                        ParseLookupFields(XrmRecordService, dataImportContainer, itemsWithThisFieldPopulated, new[] { field }, isRetry: true, allowAddForRetry: false, doWhenResolved: (e, f) => indexToUpdateCopy[e].SetField(f, e.GetField(f), XrmRecordService));
                    }

                    var itemsForUpdate = indexToUpdateCopy.Where(kv => kv.Value.GetFieldsInEntity().Any()).ToArray();

                    if (itemsForUpdate.Any())
                    {
                        var updateEntities = itemsForUpdate.Select(kv => kv.Value).ToArray();
                        var responses = XrmRecordService.UpdateMultiple(updateEntities, null, bypassWorkflowsAndPlugins: dataImportContainer.BypassFlowsPluginsAndWorkflows);
                        for (var i = 0; i < updateEntities.Count(); i++)
                        {
                            var updateEntity = updateEntities[i];
                            var originalEntity = itemsForUpdate[i].Key;
                            foreach (var updatedField in updateEntity.GetFieldsInEntity())
                            {
                                dataImportContainer.Response.RemoveFieldForRetry(originalEntity, updatedField);
                            }
                            if (responses.ContainsKey(i) && responses[i] != null)
                            {
                                dataImportContainer.LogEntityError(originalEntity, responses[i]);
                            }
                            else
                            {
                                dataImportContainer.Response.AddUpdated(originalEntity);
                            }
                            i++;
                        }
                    }
                    countImported += countThisSet;
                    dataImportContainer.Controller.UpdateProgress(countImported, countToImport, estimator.GetProgressString(countImported, taskName: $"Retrying Unresolved Fields"));
                }
            }
        }

        private void ImportEntities(DataImportContainer dataImportContainer)
        {
            var orderedTypes = GetEntityTypesOrderedForImport(dataImportContainer);

            foreach (var recordType in orderedTypes)
            {
                dataImportContainer.Controller.UpdateLevel2Progress(0, 1, "Loading");

                dataImportContainer.RemoveFromCache(recordType);
                try
                {
                    if (!dataImportContainer.TrustSourceLookupGuids)
                    {
                        dataImportContainer.LoadTargetsToCache(recordType);
                    }

                    var thisTypeEntities = new List<IRecord>();
                    foreach (var entity in dataImportContainer.EntitiesToImport)
                    {
                        if (entity.Type == recordType)
                        {
                            thisTypeEntities.Add(entity);
                        }
                    }
                    var importFieldsForEntity = dataImportContainer.GetFieldsToImport(thisTypeEntities, recordType).ToArray();

                    thisTypeEntities = OrderEntitiesForImport(dataImportContainer, thisTypeEntities, importFieldsForEntity);

                    var countRecordsToImport = thisTypeEntities.Count;
                    var countRecordsImported = 0;
                    var estimator = new TaskEstimator(countRecordsToImport);

                    if (dataImportContainer.DisplayTimeEstimations)
                    {
                        dataImportContainer.Controller.UpdateProgress(countRecordsImported, countRecordsToImport, estimator.GetProgressString(countRecordsImported, taskName: $"Importing {recordType} Records"));
                    }
                    dataImportContainer.Controller.UpdateLevel2Progress(countRecordsImported, countRecordsToImport, estimator.GetProgressString(countRecordsImported));
                    if (dataImportContainer.ParallelImportProcessCount <= 1)
                    {
                        ImportEntitiesNestedProcess(dataImportContainer, recordType, thisTypeEntities, importFieldsForEntity, countRecordsToImport, ref countRecordsImported, estimator, XrmRecordService);
                    }
                    else
                    {
                        ParallelTaskHelper.RunParallelTasks(() =>
                        {
                            var parallelProcessXrmService = XrmRecordService.CloneForParellelProcessing() as XrmRecordService;
                            ImportEntitiesNestedProcess(dataImportContainer, recordType, thisTypeEntities, importFieldsForEntity, countRecordsToImport, ref countRecordsImported, estimator, parallelProcessXrmService);
                        }, dataImportContainer.ParallelImportProcessCount);
                    }
                }
                catch (Exception ex)
                {
                    dataImportContainer.Response.AddImportError(
                        new DataImportResponseItem(recordType, null, null, null, string.Format("Error Importing Type {0}", recordType), ex));
                }
                dataImportContainer.RemoveFromCache(recordType);
            }
            dataImportContainer.Controller.TurnOffLevel2();
        }

        private static void ImportEntitiesNestedProcess(DataImportContainer dataImportContainer, string recordType, List<IRecord> thisTypeEntities, string[] importFieldsForEntity, int countRecordsToImport, ref int countRecordsImported, TaskEstimator estimator, XrmRecordService xrmRecordService)
        {
            while (thisTypeEntities.Any())
            {
                var thisSetOfEntities = LoadNextSetToProcess(dataImportContainer, thisTypeEntities, xrmRecordService);
                var countThisSet = thisSetOfEntities.Count;
                try
                {
                    var matchDictionary = new Dictionary<IRecord, IRecord>();

                    MatchEntitiesToTarget(dataImportContainer, thisSetOfEntities, matchDictionary, xrmRecordService);

                    var currentEntityFields = thisSetOfEntities
                        .SelectMany(e => e.GetFieldsInEntity())
                        .Distinct()
                        .Where(f => !f.Contains(".") && importFieldsForEntity.Contains(f))
                        .ToArray();

                    var lookupFields = currentEntityFields
                        .Where(f => xrmRecordService.IsLookup(f, recordType))
                        .ToArray();

                    ParseLookupFields(xrmRecordService, dataImportContainer, thisSetOfEntities, lookupFields, isRetry: false, allowAddForRetry: true);

                    var activityPartyFields = currentEntityFields
                        .Where(f => xrmRecordService.IsActivityParty(f, recordType))
                        .ToArray();

                    foreach (var field in activityPartyFields)
                    {
                        string matchType = null;
                        string matchField = null;
                        if (dataImportContainer.AltLookupMatchKeyDictionary.ContainsKey(recordType)
                            && dataImportContainer.AltLookupMatchKeyDictionary[recordType].ContainsKey(field))
                        {
                            matchType = dataImportContainer.AltLookupMatchKeyDictionary[recordType][field].Key;
                            matchField = dataImportContainer.AltLookupMatchKeyDictionary[recordType][field].Value;
                        }

                        var dictionaryPartiesToParent = new Dictionary<IRecord, IRecord>();
                        foreach (var entity in thisSetOfEntities.ToArray())
                        {
                            var parties = entity.GetActivityParties(field);
                            foreach (var party in parties)
                            {
                                if (!dictionaryPartiesToParent.ContainsKey(party))
                                    dictionaryPartiesToParent.Add(party, entity);
                            }
                        }
                        ParseLookupFields(xrmRecordService, dataImportContainer, dictionaryPartiesToParent.Keys.ToList(), new[] { Fields.activityparty_.partyid }, isRetry: false, allowAddForRetry: false, doWhenNotResolved: (e, f) => thisSetOfEntities.Remove(dictionaryPartiesToParent[e]), getPartyParent: (e) => dictionaryPartiesToParent[e], usetargetType: matchType, usetargetField: matchField);
                    }

                    var forCreateEntitiesCopy = new Dictionary<IRecord, IRecord>();
                    var forUpdateEntitiesCopy = new Dictionary<IRecord, IRecord>();

                    var recordTypeFileFields = xrmRecordService
                        .GetFieldMetadata(recordType)
                        .Where(fmt => fmt.FieldType == Record.Metadata.RecordFieldType.FileRef || fmt.FieldType == Record.Metadata.RecordFieldType.Image)
                        .ToArray();

                    foreach (var entity in thisSetOfEntities.ToArray())
                    {
                        var fieldsToSet = new List<string>();
                        fieldsToSet.AddRange(entity.GetFieldsInEntity()
                            .Where(importFieldsForEntity.Contains));
                        if (dataImportContainer.FieldsToRetry.ContainsKey(entity))
                            fieldsToSet.RemoveAll(f => dataImportContainer.FieldsToRetry[entity].Contains(f));

                        if (dataImportContainer.MaskEmails)
                        {
                            var emailFields = new[] { "emailaddress1", "emailaddress2", "emailaddress3" };
                            foreach (var field in emailFields)
                            {
                                var theEmail = entity.GetStringField(field);
                                if (!string.IsNullOrWhiteSpace(theEmail))
                                {
                                    entity.SetField(field, theEmail.Replace("@", "_AT_") + "_@fakemaskedemail.com", xrmRecordService);
                                }
                            }
                        }

                        var isUpdate = matchDictionary.ContainsKey(entity);
                        if (!isUpdate)
                        {
                            PopulateRequiredCreateFields(dataImportContainer, entity, fieldsToSet, xrmRecordService);
                            try
                            {
                                CheckThrowValidForCreate(entity, fieldsToSet, xrmRecordService);
                            }
                            catch (Exception ex)
                            {
                                dataImportContainer.LogEntityError(entity, ex);
                                thisSetOfEntities.Remove(entity);
                                continue;
                            }
                            var copyEntity = xrmRecordService.NewRecord(entity.Type);
                            copyEntity.Id = entity.Id;
                            foreach (var field in entity.GetFieldsInEntity())
                            {
                                if (fieldsToSet.Contains(field) && entity.GetField(field) != null)
                                {
                                    copyEntity.SetField(field, entity.GetField(field), xrmRecordService);
                                }
                            }
                            forCreateEntitiesCopy.Add(copyEntity, entity);
                        }
                        else
                        {
                            var existingRecord = matchDictionary[entity];
                            var fieldsToSubmit = dataImportContainer.ForceSubmitAllFields
                                ? fieldsToSet.ToArray()
                                : fieldsToSet.Where(f =>
                                {
                                    if (f == "overriddencreatedon")
                                    {
                                        return false;
                                    }
                                    var newValue = entity.GetField(f);
                                    var oldValue = existingRecord.GetField(f);
                                    if (oldValue is Lookup er
                                        && newValue is Lookup erNew
                                        && ((erNew.Id == null || erNew.Id.ToString() == Guid.Empty.ToString()) && (er.Id != null && er.Id.ToString() != Guid.Empty.ToString()))
                                        && erNew.Name == er.Name)
                                    {
                                        return false;
                                    }
                                    else if (oldValue is bool b && newValue == null)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        return !xrmRecordService.FieldsEqual(newValue, oldValue);
                                    }
                                }).ToArray();
                            if (fieldsToSubmit.Any())
                            {
                                var copyEntity = xrmRecordService.NewRecord(entity.Type);
                                copyEntity.Id = entity.Id;
                                foreach (var fieldToSubmit in fieldsToSubmit)
                                {
                                    if (entity.ContainsField(fieldToSubmit))
                                    {
                                        copyEntity.SetField(fieldToSubmit, entity.GetField(fieldToSubmit), xrmRecordService);
                                    }
                                }
                                forUpdateEntitiesCopy.Add(copyEntity, entity);
                            }
                            else
                            {
                                if (ImportFileFields(entity, recordTypeFileFields, dataImportContainer, xrmRecordService))
                                {
                                    dataImportContainer.Response.AddUpdated(entity);
                                }
                                else
                                {
                                    dataImportContainer.Response.AddSkippedNoChange(entity);
                                }
                            }
                        }
                    }

                    if (forCreateEntitiesCopy.Any())
                    {
                        //remove status on create if product or not inactive state set
                        foreach (var forCreate in forCreateEntitiesCopy)
                        {
                            if (forCreate.Key.ContainsField("statuscode")
                                && forCreate.Key.GetOptionKey("statecode") != null
                                && forCreate.Key.GetOptionKey("statecode") != "-1"
                                    && (string.CompareOrdinal(forCreate.Key.GetOptionKey("statecode"), "0") > 0 || (forCreate.Key.Type == Entities.product || forCreate.Key.GetOptionKey("statecode") != "2")))
                            {
                                forCreate.Key.RemoveFields(new[] { "statuscode" });
                            }
                        }
                        IEnumerable<CreateRecordResponse> responses = null;
                        try
                        {
                            responses = xrmRecordService.CreateMultiple(forCreateEntitiesCopy.Keys, bypassWorkflowsAndPlugins: dataImportContainer.BypassFlowsPluginsAndWorkflows);
                        }
                        catch (Exception ex)
                        {
                            responses = forUpdateEntitiesCopy.Select(e => new CreateRecordResponse() { Exception = ex });
                        }
                        var i = 0;
                        foreach (var createResponse in responses)
                        {
                            var originalEntity = forCreateEntitiesCopy.ElementAt(i).Value;
                            if (createResponse.Exception != null)
                            {
                                dataImportContainer.LogEntityError(originalEntity, createResponse.Exception);
                            }
                            else
                            {
                                originalEntity.Id = createResponse.Id;
                                dataImportContainer.AddCreated(originalEntity);

                                ImportFileFields(originalEntity, recordTypeFileFields, dataImportContainer, xrmRecordService);
                            }
                            i++;
                        }
                    }
                    if (forUpdateEntitiesCopy.Any())
                    {
                        //if a custom set state message dont include state and status code in updates
                        foreach (var forUpdate in forUpdateEntitiesCopy)
                        {
                            if (forUpdate.Key.ContainsField("statecode")
                                && xrmRecordService.HasCustomSetStateConfiguration(forUpdate.Key.Type))
                            {
                                forUpdate.Key.RemoveFields(new[] { "statuscode", "statecode" });
                            }
                        }
                        IDictionary<int, Exception> responses = new Dictionary<int, Exception>();
                        try
                        {
                            responses = xrmRecordService.UpdateMultiple(forUpdateEntitiesCopy.Keys, null, bypassWorkflowsAndPlugins: dataImportContainer.BypassFlowsPluginsAndWorkflows);
                        }
                        catch (Exception ex)
                        {
                            for (var j = 0; j < forUpdateEntitiesCopy.Count; j++)
                            {
                                responses.Add(j, ex);
                            }
                        }
                        var i = 0;
                        foreach (var forUpdateEntity in forUpdateEntitiesCopy)
                        {
                            var originalEntity = forUpdateEntitiesCopy.ElementAt(i).Value;
                            if (responses.ContainsKey(i))
                            {
                                dataImportContainer.LogEntityError(originalEntity, responses[i]);
                            }
                            else
                            {
                                dataImportContainer.Response.AddUpdated(originalEntity);
                                ImportFileFields(originalEntity, recordTypeFileFields, dataImportContainer, xrmRecordService);
                            }
                            i++;
                        }
                    }

                    var updateStateForEntities = new List<IRecord>();
                    foreach (var entity in forCreateEntitiesCopy.Values.Union(forUpdateEntitiesCopy.Values).ToArray())
                    {
                        var isUpdate = matchDictionary.ContainsKey(entity);
                        if (!isUpdate)
                        {
                            if (dataImportContainer.Response.GetImportForType(entity.Type).HasBeenCreated(entity.Id))
                            {
                                if (entity.GetOptionKey("statecode") != null
                                    && (string.CompareOrdinal(entity.GetOptionKey("statecode"), "0") > 0
                                    || (entity.Type == Entities.product || entity.GetOptionKey("statecode") != "2")))
                                {
                                    updateStateForEntities.Add(entity);
                                }
                            }
                        }
                        else
                        {
                            var originalEntity = matchDictionary[entity];
                            if (xrmRecordService.HasCustomSetStateConfiguration(entity.Type) &&
                                entity.ContainsField("statecode") &&
                                (entity.GetOptionKey("statecode") != originalEntity.GetOptionKey("statecode")
                                    || (entity.ContainsField("statuscode") && entity.GetOptionKey("statuscode") != originalEntity.GetOptionKey("statuscode"))))
                            {
                                updateStateForEntities.Add(entity);
                            }
                        }
                    }
                    if (updateStateForEntities.Any())
                    {
                        var responses = xrmRecordService.UpdateMultipleRecordStatus(updateStateForEntities, fieldsToUpdate: new[] { "statecode", "statuscode"}, bypassWorkflowsAndPlugins: dataImportContainer.BypassFlowsPluginsAndWorkflows);
                        for(var i = 0; i < updateStateForEntities.Count; i++)
                        {
                            var originalEntity = updateStateForEntities.ElementAt(i);
                            if (responses.ContainsKey(i) && responses[i] != null)
                            {
                                dataImportContainer.LogEntityError(originalEntity, responses[i]);
                            }
                            else
                            {
                                dataImportContainer.Response.AddUpdated(originalEntity);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    foreach (var theSetEntity in thisSetOfEntities)
                    {
                        dataImportContainer.LogEntityError(theSetEntity, ex);
                    }
                }
                countRecordsImported += countThisSet;
                if (dataImportContainer.DisplayTimeEstimations)
                {
                    dataImportContainer.Controller.UpdateProgress(countRecordsImported, countRecordsToImport, estimator.GetProgressString(countRecordsImported, taskName: $"Importing {recordType} Records"));
                }
                dataImportContainer.Controller.UpdateLevel2Progress(countRecordsImported, countRecordsToImport, estimator.GetProgressString(countRecordsImported));
            }
        }

        private static bool ImportFileFields(IRecord importEntity, IEnumerable<IFieldMetadata> recordTypeFileFields, DataImportContainer dataImportContainer, XrmRecordService xrmRecordService)
        {
            var fileFieldUpdated = false;
            foreach(var fileField in recordTypeFileFields)
            {
                var importBase64String = importEntity.GetStringField($"{fileField.SchemaName}.base64");
                var importFileName = importEntity.GetStringField($"{fileField.SchemaName}.filename");
                if (!string.IsNullOrWhiteSpace(importBase64String))
                {
                    try
                    {
                        string targetFileName = null;
                            var targetBase64String = xrmRecordService.XrmService.GetFileFieldBase64(importEntity.Type, new Guid(importEntity.Id), fileField.SchemaName, out targetFileName);
                        if (importBase64String != targetFileName || importBase64String != targetBase64String)
                        {
                            xrmRecordService.XrmService.SetFileFieldBase64(importEntity.Type, new Guid(importEntity.Id), fileField.SchemaName, importFileName, importBase64String);
                            fileFieldUpdated = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        var recordName = importEntity.GetStringField(xrmRecordService.GetPrimaryField(importEntity.Type)) ?? importEntity.Id.ToString();
                        dataImportContainer.Response.AddImportError(importEntity,
                                 new DataImportResponseItem(importEntity.Type,
                                 fileField.SchemaName, recordName, importFileName, "Error setting file", ex));
                    }
                }
            }
            return fileFieldUpdated;
        }

        private static void ParseLookupFields(XrmRecordService xrmRecordService, DataImportContainer dataImportContainer, IEnumerable<IRecord> thisSetOfEntities, IEnumerable<string> lookupFields, bool isRetry, bool allowAddForRetry, Action<IRecord, string> doWhenResolved = null,
            Action<IRecord, string> doWhenNotResolved = null,
            Func<IRecord, IRecord> getPartyParent = null, string usetargetType = null, string usetargetField = null)
        {
            if (thisSetOfEntities.Any())
            {
                var recordType = thisSetOfEntities.First().Type;
                foreach (var lookupField in lookupFields)
                {
                    var recordsNotYetResolved = thisSetOfEntities
                        .Where(e => e.GetField(lookupField) != null)
                        .ToList();

                    string altMatchType = usetargetType;
                    string altMatchField = usetargetField;

                    if (dataImportContainer.AltLookupMatchKeyDictionary != null
                        && dataImportContainer.AltLookupMatchKeyDictionary.ContainsKey(recordType)
                        && dataImportContainer.AltLookupMatchKeyDictionary[recordType].ContainsKey(lookupField))
                    {
                        altMatchType = dataImportContainer.AltLookupMatchKeyDictionary[recordType][lookupField].Key;
                        altMatchField = dataImportContainer.AltLookupMatchKeyDictionary[recordType][lookupField].Value;
                    }


                    if (dataImportContainer.TrustSourceLookupGuids && string.IsNullOrWhiteSpace(altMatchField))
                    {
                        foreach(var recordNotYetResolved in recordsNotYetResolved.ToArray())
                        {
                            var lookupGuid = recordNotYetResolved.GetLookupId(lookupField);
                            var lookupType = recordNotYetResolved.GetLookupType(lookupField) ?? null;
                            if (lookupGuid != null && !string.IsNullOrWhiteSpace(lookupType) && !lookupType.Contains(","))
                            {
                                recordsNotYetResolved.Remove(recordNotYetResolved);
                            }
                        }
                    }

                    var isAltFieldLookup = altMatchField != null && xrmRecordService.IsLookup(altMatchField, altMatchType);

                    var targetTypes = altMatchType ?? xrmRecordService.GetLookupTargetType(lookupField, recordType);
                    if (targetTypes != null)
                    {
                        var targetTypeSplit = targetTypes.Split(',');
                        foreach (var targetType in targetTypeSplit)
                        {
                            if (!recordsNotYetResolved.Any())
                                break;

                            var thisTargetField = altMatchField ?? xrmRecordService.GetPrimaryField(targetType);
                            var thisTargetPrimarykey = xrmRecordService.GetPrimaryKey(targetType);

                            var recordsToTry = recordsNotYetResolved
                                .Where(e =>
                                {
                                    var referenceType = e.GetLookupType(lookupField);
                                    return referenceType == null
                                        || referenceType.Contains(",")
                                        || referenceType == targetType
                                        || isAltFieldLookup;
                                })
                                .ToArray();

                            var targetTypesConfig = xrmRecordService.GetTypeConfigs().GetFor(targetType);
                            var isCached = dataImportContainer.IsValidForCache(targetType);

                            //if has type config of not cached
                            //we will query the matches
                            var executeQueryResponses = targetTypesConfig != null || !isCached
                                ? xrmRecordService.ExecuteMultipleQueries(recordsToTry
                                    .Select(e => dataImportContainer.GetParseLookupQuery(e, lookupField, targetType, thisTargetField))
                                    .ToArray())
                                : new ExecuteQueryResponse[0];

                            var i = 0;
                            foreach (var entity in recordsToTry)
                            {
                                var thisEntity = entity;
                                var referencedName = thisEntity.GetLookupName(lookupField);
                                var referencedId = thisEntity.GetLookupId(lookupField) ?? Guid.Empty.ToString();
                                try
                                {
                                    IEnumerable<IRecord> matchRecords = new IRecord[0];
                                    if (executeQueryResponses.Any())
                                    {
                                        var thisOnesQueryResponse = executeQueryResponses.ElementAt(i);
                                        if (thisOnesQueryResponse.Exception != null)
                                            throw new Exception("Error querying for match: " + thisOnesQueryResponse.Exception.Message);
                                        else
                                        {
                                            matchRecords = thisOnesQueryResponse.Records;
                                            if (matchRecords.Count() != 1)
                                            {
                                                if (matchRecords.Any(e => e.Id == referencedId))
                                                {
                                                    matchRecords = matchRecords.Where(e => e.Id == referencedId).ToArray();
                                                }
                                                else
                                                {
                                                    dataImportContainer.FilterForNameMatch(matchRecords).ToArray();
                                                }
                                            }
                                        }
                                    }
                                    //else the cache will be used
                                    else
                                    {
                                        matchRecords = isAltFieldLookup ?
                                            new IRecord[0]
                                            : dataImportContainer.GetMatchingEntities(targetType, new Dictionary<string, object>
                                                    {
                                                        { thisTargetPrimarykey, referencedId }
                                                    });
                                        if (!matchRecords.Any())
                                        {
                                            matchRecords = dataImportContainer.GetMatchingEntities(targetType, new Dictionary<string, object>
                                                    {
                                                        { thisTargetField, isAltFieldLookup ? thisEntity.GetField(lookupField) : referencedName }
                                                    });
                                            matchRecords = dataImportContainer.FilterForNameMatch(matchRecords);
                                        }
                                    }

                                    if (matchRecords.Count() > 1)
                                    {
                                        var caseMatch = matchRecords.Where(m => string.CompareOrdinal(referencedName, isAltFieldLookup ? m.GetLookupName(thisTargetField) : m.GetStringField(thisTargetField)) == 0);
                                        var notCaseMatch = matchRecords.Where(m => string.CompareOrdinal(referencedName, isAltFieldLookup ? m.GetLookupName(thisTargetField) : m.GetStringField(thisTargetField)) != 0);
                                        if (caseMatch.Count() == 1 && notCaseMatch.Any())
                                        {
                                            matchRecords = caseMatch.ToArray();
                                        }
                                        else
                                        {
                                            var activeMatches = matchRecords.Where(m => m.GetOptionKey("statecode") == "0");
                                            if (activeMatches.Count() == 1)
                                            {
                                                matchRecords = activeMatches.ToArray();
                                            }
                                            else
                                            {
                                                throw new Exception($"Multiple matches for  field {lookupField} named '{referencedName}'. This field has not been set");
                                            }
                                        }
                                    }
                                    if (matchRecords.Count() == 1)
                                    {
                                        var matchedRecord = matchRecords.First();
                                        var matchedRecordEntityReference = matchedRecord.ToLookup();
                                        thisEntity.SetField(lookupField, matchedRecordEntityReference, xrmRecordService);
                                        string name = null;
                                        if(xrmRecordService.IsString(thisTargetField, matchedRecord.Type))
                                        {
                                            name = matchedRecord.GetStringField(thisTargetField);
                                        }
                                        else
                                        {
                                            name = matchedRecord.GetStringField(xrmRecordService.GetPrimaryField(matchedRecordEntityReference.RecordType));
                                        }
                                        matchedRecordEntityReference.Name = name;
                                        recordsNotYetResolved.Remove(thisEntity);
                                        doWhenResolved?.Invoke(thisEntity, lookupField);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    thisEntity.RemoveFields(lookupField);
                                    dataImportContainer.LogEntityError(thisEntity, ex);
                                    recordsNotYetResolved.Remove(thisEntity);
                                }
                                i++;
                            }
                        }
                    }
                    if (recordsNotYetResolved.Any())
                    {
                        foreach (var notResolved in recordsNotYetResolved)
                        {
                            doWhenNotResolved?.Invoke(notResolved, lookupField);
                            if (isRetry || !allowAddForRetry)
                            {
                                var rowNumber = notResolved.ContainsField("Sheet.RowNumber")
                                    ? notResolved.GetIntegerField("Sheet.RowNumber")
                                    : (int?)null;
                                var notResolvedLogEntity = getPartyParent != null
                                    ? getPartyParent(notResolved)
                                    : notResolved;
                                var notResolvedLogEntityPrimaryField = xrmRecordService.GetPrimaryField(notResolvedLogEntity.Type);
                                dataImportContainer.Response.AddImportError(notResolvedLogEntity,
                                     new DataImportResponseItem(notResolvedLogEntity.Type,
                                     lookupField,
                                     notResolvedLogEntity.GetStringField(notResolvedLogEntityPrimaryField) ?? notResolvedLogEntity.Id.ToString(), notResolved.GetLookupName(lookupField),
                                        "No Match Found For Lookup Field", null, rowNumber: rowNumber));
                            }
                            else
                            {
                                if (!dataImportContainer.FieldsToRetry.ContainsKey(notResolved))
                                    dataImportContainer.FieldsToRetry.Add(notResolved, new List<string>());
                                dataImportContainer.FieldsToRetry[notResolved].Add(lookupField);
                                dataImportContainer.Response.AddFieldForRetry(notResolved, lookupField);
                            }
                        }
                    }
                }
            }
        }

        private static readonly object _loadNextSetToProcessLock = new object();
        private static List<IRecord> LoadNextSetToProcess(DataImportContainer dataImportContainer, List<IRecord> orderedEntitiesForImport, XrmRecordService xrmRecordService)
        {
            lock (_loadNextSetToProcessLock)
            {
                var thisSetOfEntities = new List<IRecord>();
                if (orderedEntitiesForImport.Any())
                {
                    var recordType = orderedEntitiesForImport[0].Type;
                    var takeSomeCountDown = dataImportContainer.ExecuteMultipleSetSize;
                    while (takeSomeCountDown > 0 && orderedEntitiesForImport.Any())
                    {
                        bool dontGetMoreThisSet = false;

                        var addToSet = orderedEntitiesForImport[0];

                        var referenceFields = addToSet
                            .GetFields()
                            .Where(kv => kv.Value is Core.FieldType.Lookup)
                            .Select(kv => kv.Value as Core.FieldType.Lookup)
                            .ToArray();
                        foreach (var referenceField in referenceFields)
                        {
                            var logicalName = referenceField.RecordType;
                            if (logicalName != null)
                            {
                                var targets = logicalName.Split(',');
                                foreach (var target in targets)
                                {
                                    if (target == recordType)
                                    {
                                        var id = referenceField.Id;
                                        var name = referenceField.Name;
                                        var primaryField = xrmRecordService.GetPrimaryField(recordType);
                                        if (thisSetOfEntities.Exists(e =>
                                            (id != null && id != Guid.Empty.ToString() && e.Id == id)
                                            || (primaryField != null && e.GetStringField(primaryField) == name)))
                                        {
                                            dontGetMoreThisSet = true;
                                        }
                                    }
                                }
                            }
                        }
                        if (dontGetMoreThisSet)
                        {
                            break;
                        }
                        else
                        {
                            thisSetOfEntities.Add(addToSet);
                            orderedEntitiesForImport.RemoveAt(0);
                            takeSomeCountDown--;
                        }
                    }
                }
                return thisSetOfEntities;
            }
        }

        private static void MatchEntitiesToTarget(DataImportContainer dataImportContainer, List<IRecord> thisSetOfEntities, Dictionary<IRecord, IRecord> matchDictionary, XrmRecordService xrmRecordService)
        {
            if (!thisSetOfEntities.Any())
                return;
            var recordType = thisSetOfEntities[0].Type;
            var primaryField = xrmRecordService.GetPrimaryField(recordType);

            var thisTypesConfig = xrmRecordService.GetTypeConfigs().GetFor(recordType);
            var isCached = dataImportContainer.IsValidForCache(recordType);

            //if has type config of not cached
            //we will query the matches
            var executeQueryResponses = thisTypesConfig != null || !isCached
                ? xrmRecordService.ExecuteMultipleQueries(
                    thisSetOfEntities.Select(e => dataImportContainer.GetMatchQueryExpression(e, dataImportContainer))
                    .ToArray())
                : new ExecuteQueryResponse[0];

            var i = 0;
            foreach (var entity in thisSetOfEntities.ToArray())
            {
                var thisEntity = entity;
                try
                {
                    IEnumerable<IRecord> matchRecords = new IRecord[0];
                    if (executeQueryResponses.Any())
                    {
                        var thisOnesQueryResponse = executeQueryResponses.ElementAt(i);
                        if (thisOnesQueryResponse.Exception != null)
                            throw new Exception("Error querying for match: " + thisOnesQueryResponse.Exception.Message);
                        else
                        {
                            matchRecords = thisOnesQueryResponse.Records;
                            if (matchRecords.Any(e => e.Id == entity.Id))
                                matchRecords = matchRecords.Where(e => e.Id == entity.Id).ToArray();
                        }
                    }
                    //else the cache will be used
                    else if (dataImportContainer.AltMatchKeyDictionary.ContainsKey(thisEntity.Type))
                    {
                        var matchKeyFieldDictionary = dataImportContainer.AltMatchKeyDictionary[thisEntity.Type]
                            .Distinct().ToDictionary(f => f.Key, f => thisEntity.GetField(f.Key));
                        if (matchKeyFieldDictionary.Any(kv => xrmRecordService.FieldsEqual(null, kv.Value)))
                        {
                            throw new Exception("Match Key Field Is Empty");
                        }
                        matchRecords = dataImportContainer.GetMatchingEntities(thisEntity.Type, matchKeyFieldDictionary);
                    }
                    else if (dataImportContainer.MatchOption == MatchOption.PrimaryKeyThenName || thisTypesConfig != null)
                    {
                        matchRecords = dataImportContainer.GetMatchingEntities(thisEntity.Type, new Dictionary<string, object>
                            {
                                {  xrmRecordService.GetPrimaryKey(thisEntity.Type), thisEntity.Id }
                            });
                        if (!matchRecords.Any())
                        {
                            matchRecords = dataImportContainer.GetMatchingEntities(thisEntity.Type, new Dictionary<string, object>
                                        {
                                            { primaryField, thisEntity.GetStringField(primaryField) }
                                        });
                            matchRecords = dataImportContainer.FilterForNameMatch(matchRecords);
                        }
                    }
                    else if (dataImportContainer.MatchOption == MatchOption.PrimaryKeyOnly && thisEntity.Id != null && thisEntity.Id != Guid.Empty.ToString())
                    {
                        matchRecords = dataImportContainer.GetMatchingEntities(thisEntity.Type, new Dictionary<string, object>
                                    {
                                        {  xrmRecordService.GetPrimaryKey(thisEntity.Type), thisEntity.Id }
                                    });
                    }

                    //special case for business unit
                    if (!matchRecords.Any() && thisEntity.Type == Entities.businessunit && thisEntity.GetField(Fields.businessunit_.parentbusinessunitid) == null)
                    {
                        matchRecords = new[] { dataImportContainer.GetRootBusinessUnit() };
                    }

                    //verify and process match results
                    if (!matchRecords.Any() && dataImportContainer.UpdateOnly)
                    {
                        throw new Exception("Updates Only And No Matching Record Found");
                    }
                    if(dataImportContainer.AltMatchKeyDictionary.ContainsKey(thisEntity.Type))
                    {
                        var caseSensitiveMatches = dataImportContainer.AltMatchKeyDictionary[thisEntity.Type]
                            .Where(kv => kv.Value).ToArray();
                        if(caseSensitiveMatches.Any())
                        {
                            var notCaseMatches = matchRecords
                                .Where(m => caseSensitiveMatches.Any(csm => string.CompareOrdinal(thisEntity.GetStringField(csm.Key), m.GetStringField(csm.Key)) != 0))
                                .ToArray();
                            matchRecords = matchRecords.Except(notCaseMatches).ToArray();
                        }
                    }
                    if (matchRecords.Count() > 1)
                    {
                        var matchStringFields = (dataImportContainer.AltMatchKeyDictionary.ContainsKey(thisEntity.Type)
                            ? dataImportContainer.AltMatchKeyDictionary[thisEntity.Type].Select(kv => kv.Key)
                            : new[] { primaryField })
                            .Where(s => xrmRecordService.IsString(s, recordType)).ToArray();

                        var caseMatch = matchRecords.Where(m => matchStringFields.All(ms => string.CompareOrdinal(thisEntity.GetStringField(ms), m.GetStringField(ms)) == 0));
                        var notCaseMatch = matchRecords.Where(m => matchStringFields.All(ms => string.CompareOrdinal(thisEntity.GetStringField(ms), m.GetStringField(ms)) != 0));

                        if (matchStringFields.Any() && caseMatch.Count() == 1 && notCaseMatch.Any())
                        {
                            matchRecords = caseMatch.ToArray();
                        }
                        else
                        {
                            throw new Exception("Multiple Matches Were Found In The Target");
                        }
                        
                    }
                    if (matchRecords.Any())
                    {
                        var matchRecord = matchRecords.First();
                        if (thisEntity.Id != null && thisEntity.Id != Guid.Empty.ToString())
                            dataImportContainer.IdSwitches[recordType].Add(thisEntity.Id, matchRecord.Id);
                        thisEntity.Id = matchRecord.Id;
                        thisEntity.SetField(xrmRecordService.GetPrimaryKey(thisEntity.Type), thisEntity.Id, xrmRecordService);
                        if (thisTypesConfig != null)
                        {
                            if (thisTypesConfig.ParentLookupField != null)
                                thisEntity.SetField(thisTypesConfig.ParentLookupField, matchRecord.GetField(thisTypesConfig.ParentLookupField), xrmRecordService);
                            if (thisTypesConfig.UniqueChildFields != null)
                            {
                                foreach (var childField in thisTypesConfig.UniqueChildFields)
                                {
                                    var oldValue = thisEntity.GetField(childField);
                                    var newValue = matchRecord.GetField(childField);
                                    if (oldValue is Lookup oldEr
                                        && newValue is Lookup newEr
                                        && newEr.Name == null)
                                    {
                                        //this just fixing case on notes where the new query didnt populate trhe reference name
                                        newEr.Name = oldEr.Name;
                                    }
                                    thisEntity.SetField(childField, matchRecord.GetField(childField), xrmRecordService);
                                }
                            }
                        }
                        matchDictionary.Add(thisEntity, matchRecord);
                    }
                }
                catch (Exception ex)
                {
                    dataImportContainer.LogEntityError(thisEntity, ex);
                    thisSetOfEntities.Remove(thisEntity);
                }
                i++;
            }
        }

        private List<IRecord> OrderEntitiesForImport(DataImportContainer dataImportContainer, List<IRecord> thisTypeEntities, IEnumerable<string> importFieldsForEntity)
        {
            var orderedEntities = new List<IRecord>();
            if (!thisTypeEntities.Any())
            {
                return orderedEntities;
            }
            var recordType = thisTypeEntities.First().Type;
            var primaryField = XrmRecordService.GetPrimaryField(recordType);
            var ignoreFields = dataImportContainer.GetIgnoreFields(recordType);
            var fieldsDontExist = dataImportContainer.GetFieldsInEntities(thisTypeEntities)
                .Where(f => !f.Contains("."))
                .Where(f => !XrmRecordService.FieldExists(f, recordType))
                .Where(f => !ignoreFields.Contains(f))
                .Distinct()
                .ToArray();
            foreach (var field in fieldsDontExist)
            {
                dataImportContainer.Response.AddImportError(
                    new DataImportResponseItem(recordType, field, null, null,
                        $"Field {field} On Entity {recordType} Doesn't Exist In Target Instance And Will Be Ignored",
                        new NullReferenceException($"Field {field} On Entity {recordType} Doesn't Exist In Target Instance And Will Be Ignored")));
            }

            var selfReferenceFields = importFieldsForEntity.Where(
                f => XrmRecordService.IsLookup(f, recordType) &&
                     XrmRecordService.GetLookupTargetType(f, recordType) == recordType)
                .ToArray();

            if (!selfReferenceFields.Any())
                return thisTypeEntities.ToList();

            // Build fast lookup indexes
            var n = thisTypeEntities.Count;
            var indexById = new Dictionary<string, int>(n);
            var nameToIndexes = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
            
            for (int i = 0; i < n; i++)
            {
                var e = thisTypeEntities[i];
                if (e.Id != null && !indexById.ContainsKey(e.Id))
                {
                    indexById[e.Id] = i;
                }

                var pName = e.GetStringField(primaryField) ?? string.Empty;
                if (!string.IsNullOrEmpty(pName))
                {
                    if (!nameToIndexes.TryGetValue(pName, out var list))
                    {
                        list = new List<int>();
                        nameToIndexes[pName] = list;
                    }
                    list.Add(i);
                }
            }

            // adjacency list: edge from target -> dependent
            var adj = new List<int>[n];
            var indegree = new int[n];
            for (int i = 0; i < n; i++)
                adj[i] = new List<int>();

            // Build edges
            // For each entity, inspect its self-reference lookup fields and create directed edges
            // from the referenced target -> dependent (target must be created before dependent).
            // Prefer GUID matches when present. If the GUID is present but does not match any
            // entity in this batch and TrustSourceLookupGuids is false, fall back to name matching.
            // If TrustSourceLookupGuids is true, do not fall back to name matching for GUIDs that
            // do not resolve in this batch.
            for (int i = 0; i < n; i++)
            {
                var e = thisTypeEntities[i];
                foreach (var f in selfReferenceFields)
                {
                    var lookupId = e.GetLookupId(f);
                    if (lookupId != null)
                    {
                        // Prefer GUID match
                        if (indexById.TryGetValue(lookupId, out var targetIdx))
                        {
                            adj[targetIdx].Add(i);
                            indegree[i]++;
                        }
                        else if (!dataImportContainer.TrustSourceLookupGuids)
                        {
                            // GUID present but didn't resolve in this batch; fall back to name matching
                            var lookupName = e.GetLookupName(f);
                            if (!string.IsNullOrEmpty(lookupName) && nameToIndexes.TryGetValue(lookupName, out var targets))
                            {
                                foreach (var targetIndex in targets)
                                {
                                    adj[targetIndex].Add(i);
                                    indegree[i]++;
                                }
                            }
                        }
                        // else: TrustSourceLookupGuids == true -> do not attempt name fallback
                    }
                    else
                    {
                        // No GUID available, match by primary name
                        var lookupName = e.GetLookupName(f);
                        if (!string.IsNullOrEmpty(lookupName) && nameToIndexes.TryGetValue(lookupName, out var targets))
                        {
                            foreach (var targetIndex in targets)
                            {
                                adj[targetIndex].Add(i);
                                indegree[i]++;
                            }
                        }
                    }
                }

                if (i > 0 && i % 10000 == 0)
                {
                    dataImportContainer.Controller.LogLiteral($"Building reference graph for sorting {i}/{n}");
                    dataImportContainer.Controller.UpdateLevel2Progress(0, 1, $"Building reference graph for sorting {i}/{n}");
                }
            }

            // Kahn's algorithm
            var q = new Queue<int>();
            for (int i = 0; i < n; i++)
                if (indegree[i] == 0)
                    q.Enqueue(i);

            var resultIndexes = new List<int>(n);
            while (q.Count > 0)
            {
                var u = q.Dequeue();
                resultIndexes.Add(u);
                foreach (var v in adj[u])
                {
                    indegree[v]--;
                    if (indegree[v] == 0)
                        q.Enqueue(v);
                }

                if (resultIndexes.Count % 10000 == 0)
                {
                    dataImportContainer.Controller.LogLiteral($"Sorting for import {resultIndexes.Count}/{n}");
                    dataImportContainer.Controller.UpdateLevel2Progress(0, 1, $"Sorting for import {resultIndexes.Count}/{n}");
                }
            }

            if (resultIndexes.Count != n)
            {
                // Cycles detected: there are remaining nodes with indegree > 0.
                // We do not treat this as an import error; append remaining items in original input order
                // so they are still included in the import. This yields a best-effort ordering while
                // ensuring no items are dropped.
                var remaining = new List<int>();
                for (int i = 0; i < n; i++)
                    if (indegree[i] > 0)
                        remaining.Add(i);

                if (remaining.Any())
                {
                    dataImportContainer.Controller.LogLiteral($"Detected cycles among {remaining.Count} {recordType} records; appending remaining items.");
                    dataImportContainer.Controller.UpdateLevel2Progress(0, 1, $"Detected cycles among {remaining.Count} {recordType} records; appending remaining items.");
                    foreach (var idx in remaining)
                        resultIndexes.Add(idx);
                }
            }

            orderedEntities = resultIndexes.Select(i => thisTypeEntities[i]).ToList();

            dataImportContainer.Controller.LogLiteral($"Finished ordering {orderedEntities.Count}/{n}");
            dataImportContainer.Controller.UpdateLevel2Progress(0, 1, $"Finished ordering {orderedEntities.Count}/{n}");

            return orderedEntities;
        }

        private IEnumerable<string> GetEntityTypesOrderedForImport(DataImportContainer dataImportContainer)
        {
            var orderedTypes = new List<string>();

            var dependencyDictionary = dataImportContainer.EntityTypesToImport
                .ToDictionary(s => s, s => new List<string>());
            var dependentTo = dataImportContainer.EntityTypesToImport
                .ToDictionary(s => s, s => new List<string>());

            var toDo = dataImportContainer.EntityTypesToImport.Count();
            var done = 0;
            var fieldsToImport = new Dictionary<string, IEnumerable<string>>();
            foreach (var type in dataImportContainer.EntityTypesToImport)
            {
                dataImportContainer.Controller.LogLiteral($"Loading Fields For Import {done++}/{toDo}");
                var thatTypeEntities = dataImportContainer.EntitiesToImport.Where(e => e.Type == type).ToList();
                var fields = dataImportContainer.GetFieldsToImport(thatTypeEntities, type)
                    .Where(f => XrmRecordService.FieldExists(f, type) &&
                        (XrmRecordService.IsLookup(f, type) || XrmRecordService.IsActivityParty(f, type)));
                fieldsToImport.Add(type, fields.ToArray());
            }

            toDo = dataImportContainer.EntityTypesToImport.Count();
            done = 0;
            foreach (var type in dataImportContainer.EntityTypesToImport)
            {
                dataImportContainer.Controller.LogLiteral($"Ordering Types For Import {done++}/{toDo}");
                //iterate through the types and if any of them have a lookup which references this type
                //then insert this one before it for import first
                //otherwise just append to the end
                foreach (var otherType in dataImportContainer.EntityTypesToImport.Where(s => s != type))
                {
                    var fields = fieldsToImport[otherType];
                    var thatTypeEntities = dataImportContainer.EntitiesToImport.Where(e => e.Type == otherType).ToList();
                    foreach (var field in fields)
                    {
                        if (thatTypeEntities.Any(e =>
                            (XrmRecordService.IsLookup(field, otherType) && (e.GetLookupType(field)?.Split(',').Contains(type) ?? false))
                            || (XrmRecordService.IsActivityParty(field, otherType) && e.GetActivityParties(field).Any(p => p.GetLookupType(Fields.activityparty_.partyid) == type))))
                        {
                            dependencyDictionary[type].Add(otherType);
                            dependentTo[otherType].Add(type);
                            break;
                        }
                    }
                }
            }
            foreach (var dependency in dependencyDictionary)
            {
                if (!dependentTo[dependency.Key].Any())
                    orderedTypes.Insert(0, dependency.Key);
                if (orderedTypes.Contains(dependency.Key))
                    continue;
                foreach (var otherType in orderedTypes.ToArray())
                {
                    if (dependency.Value.Contains(otherType))
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
            prioritiseOver.Add(new KeyValuePair<string, string>(Entities.email, Entities.activitymimeattachment));
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

            return orderedTypes;
        }

        private static void PopulateRequiredCreateFields(DataImportContainer dataImportContainer, IRecord thisEntity, List<string> fieldsToSet, XrmRecordService xrmRecordService)
        {
            if (thisEntity.Type == Entities.team
                && !fieldsToSet.Contains(Fields.team_.businessunitid)
                && xrmRecordService.FieldExists(Fields.team_.businessunitid, Entities.team))
            {
                thisEntity.SetLookup(Fields.team_.businessunitid, dataImportContainer.GetRootBusinessUnit().Id, Entities.businessunit);
                fieldsToSet.Add(Fields.team_.businessunitid);
                if (dataImportContainer.FieldsToRetry.ContainsKey(thisEntity)
                    && dataImportContainer.FieldsToRetry[thisEntity].Contains(Fields.team_.businessunitid))
                    dataImportContainer.FieldsToRetry[thisEntity].Remove(Fields.team_.businessunitid);
            }
            if (thisEntity.Type == Entities.subject
                    && !fieldsToSet.Contains(Fields.subject_.featuremask)
                    && xrmRecordService.FieldExists(Fields.subject_.featuremask, Entities.subject))
            {
                thisEntity.SetField(Fields.subject_.featuremask, 1, xrmRecordService);
                fieldsToSet.Add(Fields.subject_.featuremask);
                if (dataImportContainer.FieldsToRetry.ContainsKey(thisEntity)
                    && dataImportContainer.FieldsToRetry[thisEntity].Contains(Fields.subject_.featuremask))
                    dataImportContainer.FieldsToRetry[thisEntity].Remove(Fields.subject_.featuremask);
            }
            if (thisEntity.Type == Entities.uomschedule)
            {
                fieldsToSet.Add(Fields.uomschedule_.baseuomname);
            }
            if (thisEntity.Type == Entities.uom)
            {
                var unitGroupName = thisEntity.GetLookupName(Fields.uom_.uomscheduleid);
                if (string.IsNullOrWhiteSpace(unitGroupName))
                {
                    throw new NullReferenceException($"Error The {xrmRecordService.GetFieldLabel(Fields.uom_.uomscheduleid, Entities.uom)} Name Is Not Populated");
                }
                fieldsToSet.Add(Fields.uom_.uomscheduleid);

                var baseUnitName = thisEntity.GetLookupName(Fields.uom_.baseuom);
                var baseUnitMatchQuery = new QueryDefinition(Entities.uom);
                if(dataImportContainer.ContainsExportedConfigFields)
                {
                    var configUniqueFields = xrmRecordService.GetTypeConfigs().GetFor(Entities.uom).UniqueChildFields;
                    dataImportContainer.AddUniqueFieldConfigJoins(thisEntity, baseUnitMatchQuery, configUniqueFields, prefixFieldInEntity: $"{Fields.uom_.baseuom}.");
                }
                else
                {
                    if (baseUnitName == null)
                    {
                        throw new NullReferenceException($"{xrmRecordService.GetFieldLabel(Fields.uom_.baseuom, Entities.uom)} is required");
                    }
                    baseUnitMatchQuery.RootFilter.AddCondition(Fields.uom_.name, ConditionType.Equal, baseUnitName);
                    var unitGroupLink = new Join(Fields.uom_.uomscheduleid, Entities.uomschedule, Fields.uomschedule_.uomscheduleid);
                    baseUnitMatchQuery.Joins.Add(unitGroupLink);
                    unitGroupLink.RootFilter.AddCondition(Fields.uomschedule_.name, ConditionType.Equal, unitGroupName);
                }
                var baseUnitMatches = xrmRecordService.RetreiveAll(baseUnitMatchQuery);
                if (!baseUnitMatches.Any())
                {
                    throw new Exception($"Could Not Identify The {xrmRecordService.GetFieldLabel(Fields.uom_.baseuom, Entities.uom)} {baseUnitName}. No Match Found For The {xrmRecordService.GetFieldLabel(Fields.uom_.uomscheduleid, Entities.uom)}");
                }
                if (baseUnitMatches.Count() > 1)
                {
                    throw new Exception($"Could Not Identify The {xrmRecordService.GetFieldLabel(Fields.uom_.baseuom, Entities.uom)} {baseUnitName}. Multiple Matches Found For The {xrmRecordService.GetFieldLabel(Fields.uom_.uomscheduleid, Entities.uom)}");
                }
                thisEntity.SetLookup(Fields.uom_.baseuom, baseUnitMatches.First().Id, baseUnitMatches.First().Type);
                thisEntity.SetField(Fields.uom_.uomscheduleid, baseUnitMatches.First().GetField(Fields.uom_.uomscheduleid), xrmRecordService);
                fieldsToSet.Add(Fields.uom_.baseuom);
            }
            if (thisEntity.Type == Entities.product)
            {
                var unitGroupId = thisEntity.GetLookupId(Fields.product_.defaultuomscheduleid);
                if(unitGroupId != null)
                    fieldsToSet.Add(Fields.product_.defaultuomscheduleid);
                var unitId = thisEntity.GetLookupId(Fields.product_.defaultuomid);
                if (unitId != null)
                    fieldsToSet.Add(Fields.product_.defaultuomid);
            }
            if (thisEntity.Type == Entities.list)
            {
                if(!fieldsToSet.Contains(Fields.list_.createdfromcode))
                {
                    if(!thisEntity.ContainsField(Fields.list_.createdfromcode))
                    {
                        throw new NullReferenceException($"{xrmRecordService.GetFieldLabel(Fields.list_.createdfromcode, Entities.list)} is required");
                    }
                    fieldsToSet.Add(Fields.list_.createdfromcode);
                }
                if (!fieldsToSet.Contains(Fields.list_.type))
                {
                    if (!thisEntity.ContainsField(Fields.list_.type))
                    {
                        throw new NullReferenceException($"{xrmRecordService.GetFieldLabel(Fields.list_.type, Entities.list)} is required");
                    }
                    fieldsToSet.Add(Fields.list_.type);
                }
            }
            if (thisEntity.Type == Entities.activitymimeattachment)
            {
                if (fieldsToSet.Contains(Fields.activitymimeattachment_.activityid))
                {
                    if (fieldsToSet.Contains(Fields.activitymimeattachment_.objecttypecode))
                    {
                        fieldsToSet.Remove(Fields.activitymimeattachment_.objecttypecode);
                    }
                    if (fieldsToSet.Contains(Fields.activitymimeattachment_.objectid))
                    {
                        fieldsToSet.Remove(Fields.activitymimeattachment_.objectid);
                    }
                }
            }
        }

        private static void CheckThrowValidForCreate(IRecord thisEntity, List<string> fieldsToSet, XrmRecordService xrmRecordService)
        {
            if (thisEntity != null)
            {
                switch (thisEntity.Type)
                {
                    case Entities.annotation:
                        {
                            if (!fieldsToSet.Contains(Fields.annotation_.objectid))
                                throw new NullReferenceException($"Cannot create {xrmRecordService.GetDisplayName(thisEntity.Type)} {thisEntity.GetStringField(xrmRecordService.GetPrimaryField(thisEntity.Type))} as its parent {(thisEntity.GetStringField(Fields.annotation_.objecttypecode) != null ? xrmRecordService.GetDisplayName(thisEntity.GetStringField(Fields.annotation_.objecttypecode)) : "Unknown Type")} does not exist");
                            break;
                        }
                    case Entities.productpricelevel:
                        {
                            if (!fieldsToSet.Contains(Fields.productpricelevel_.pricelevelid))
                                throw new NullReferenceException($"Cannot create {xrmRecordService.GetDisplayName(thisEntity.Type)} {thisEntity.GetStringField(xrmRecordService.GetPrimaryField(thisEntity.Type))} as its parent {xrmRecordService.GetDisplayName(Entities.pricelevel)} is empty");
                            break;
                        }
                    case Entities.product:
                        {
                            if (!fieldsToSet.Contains(Fields.product_.defaultuomid))
                                throw new NullReferenceException($"{xrmRecordService.GetFieldLabel(Fields.product_.defaultuomid, Entities.product)} is required on the {xrmRecordService.GetDisplayName(Entities.product)}");
                            if (!fieldsToSet.Contains(Fields.product_.defaultuomscheduleid))
                                throw new NullReferenceException($"{xrmRecordService.GetFieldLabel(Fields.product_.defaultuomscheduleid, Entities.product)} is required on the {xrmRecordService.GetDisplayName(Entities.product)}");
                            if (!fieldsToSet.Contains(Fields.product_.quantitydecimal))
                                throw new NullReferenceException($"{xrmRecordService.GetFieldLabel(Fields.product_.quantitydecimal, Entities.product)} is required on the {xrmRecordService.GetDisplayName(Entities.product)}");
                            break;
                        }
                }
            }
        }
    }
}
