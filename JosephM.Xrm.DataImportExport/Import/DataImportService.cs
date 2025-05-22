using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.ServiceModel;

namespace JosephM.Xrm.DataImportExport.Import
{
    public class DataImportService
    {
        public DataImportService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public XrmRecordService XrmRecordService { get; set; }

        protected XrmService XrmService
        {
            get
            {
                return XrmRecordService.XrmService;
            }
        }

        private readonly Dictionary<string, Dictionary<string, Dictionary<string, List<Entity>>>> _cachedRecords = new Dictionary<string, Dictionary<string, Dictionary<string, List<Entity>>>>();

        public DataImportResponse DoImport(IEnumerable<Entity> entities, ServiceRequestController controller, bool maskEmails, MatchOption matchOption = MatchOption.PrimaryKeyThenName, IEnumerable<DataImportResponseItem> loadExistingErrorsIntoSummary = null, Dictionary<string, IEnumerable<KeyValuePair<string, bool>>> altMatchKeyDictionary = null, Dictionary<string, Dictionary<string, KeyValuePair<string, string>>> altLookupMatchKeyDictionary = null, bool updateOnly = false, bool includeOwner = false, bool includeOverrideCreatedOn = false, bool containsExportedConfigFields = true, int? executeMultipleSetSize = null, int? targetCacheLimit = null, bool onlyFieldMatchActive = false, bool forceSubmitAllFields = false, bool displayTimeEstimations = false, int parallelImportProcessCount = 1, bool bypassWorkflowsAndPlugins = false) 
        {
            var response = new DataImportResponse(entities, loadExistingErrorsIntoSummary);
            controller.AddObjectToUi(response);
            try
            {
                controller.LogLiteral("Preparing Import");
                var dataImportContainer = new DataImportContainer(response,
                    XrmRecordService,
                    altMatchKeyDictionary ?? new Dictionary<string, IEnumerable<KeyValuePair<string, bool>>>(),
                    altLookupMatchKeyDictionary ?? new Dictionary<string, Dictionary<string, KeyValuePair<string, string>>>(),
                    entities,
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
                    bypassWorkflowsAndPlugins);

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

                var relationship = XrmService.GetRelationshipMetadataForEntityName(thisEntityName);
                var type1 = relationship.Entity1LogicalName;
                var field1 = relationship.Entity1IntersectAttribute;
                var type2 = relationship.Entity2LogicalName;
                var field2 = relationship.Entity2IntersectAttribute;

                dataImportContainer.Controller.UpdateProgress(countImported++, countToImport, $"Associating {thisEntityName} Records");
                dataImportContainer.Controller.UpdateLevel2Progress(0, 1, "Loading");
                var thisTypeEntities = dataImportContainer.EntitiesToImport.Where(e => e.LogicalName == thisEntityName).ToList();
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

                    var copiesForAssociate = new List<Entity>();

                    foreach (var thisEntity in thisSetOfEntities)
                    {
                        try
                        {

                            //bit of hack
                            //when importing from csv just set the fields to the string name of the referenced record
                            //so either string when csv or guid when xml import/export
                            string matchType1 = type1;
                            string matchField1 = XrmRecordService.GetPrimaryField(type1);
                            if (dataImportContainer.AltLookupMatchKeyDictionary.ContainsKey(thisEntity.LogicalName)
                                && dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.LogicalName].ContainsKey(relationship.Entity1IntersectAttribute))
                            {
                                matchType1 = dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.LogicalName][relationship.Entity1IntersectAttribute].Key;
                                matchField1 = dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.LogicalName][relationship.Entity1IntersectAttribute].Value;
                            }
                            var value1 = thisEntity.GetField(relationship.Entity1IntersectAttribute);
                            var id1 = value1 is string
                                ? dataImportContainer.GetUniqueMatchingEntity(matchType1, matchField1, (string)value1).Id
                                : thisEntity.GetGuidField(relationship.Entity1IntersectAttribute);

                            string matchType2 = type2;
                            string matchField2 = XrmRecordService.GetPrimaryField(type2);
                            if (dataImportContainer.AltLookupMatchKeyDictionary.ContainsKey(thisEntity.LogicalName)
                                && dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.LogicalName].ContainsKey(relationship.Entity2IntersectAttribute))
                            {
                                matchType2 = dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.LogicalName][relationship.Entity2IntersectAttribute].Key;
                                matchField2 = dataImportContainer.AltLookupMatchKeyDictionary[thisEntity.LogicalName][relationship.Entity2IntersectAttribute].Value;
                            }
                            var value2 = thisEntity.GetField(relationship.Entity2IntersectAttribute);
                            var id2 = value2 is string
                                ? dataImportContainer.GetUniqueMatchingEntity(matchType2, matchField2, (string)value2).Id
                                : thisEntity.GetGuidField(relationship.Entity2IntersectAttribute);

                            //add a where field lookup reference then look it up
                            if (dataImportContainer.IdSwitches.ContainsKey(type1) && dataImportContainer.IdSwitches[type1].ContainsKey(id1))
                                id1 = dataImportContainer.IdSwitches[type1][id1];
                            if (dataImportContainer.IdSwitches.ContainsKey(type2) && dataImportContainer.IdSwitches[type2].ContainsKey(id2))
                                id2 = dataImportContainer.IdSwitches[type2][id2];

                            var copyForAssociate = new Entity(thisEntity.LogicalName) { Id = thisEntity.Id };
                            copyForAssociate.SetField(field1, id1);
                            copyForAssociate.SetField(field2, id2);
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
                            var q = new QueryByAttribute(relationship.IntersectEntityName);
                            q.AddAttributeValue(field1, c.GetGuidField(field1));
                            q.AddAttributeValue(field2, c.GetGuidField(field2));
                            return new RetrieveMultipleRequest()
                            {
                                Query = q
                            };
                        })
                        .ToArray();

                    var executeMultipleResponses = XrmService.ExecuteMultiple(existingAssociationsQueries);

                    var notYetAssociated = new List<Entity>();
                    var i = 0;
                    foreach (var queryResponse in executeMultipleResponses)
                    {
                        var associationEntity = copiesForAssociate[i];
                        if (queryResponse.Fault != null)
                        {
                            dataImportContainer.LogAssociationError(associationEntity, new FaultException<OrganizationServiceFault>(queryResponse.Fault, queryResponse.Fault.Message));
                        }
                        else if (!((RetrieveMultipleResponse)queryResponse.Response).EntityCollection.Entities.Any())
                        {
                            notYetAssociated.Add(associationEntity);
                        }
                        else
                        {
                            associationEntity.Id = Guid.NewGuid();
                            dataImportContainer.Response.AddSkippedNoChange(associationEntity);
                        }
                        i++;
                    }

                    var associateRequests = notYetAssociated.
                        Select(e =>
                        {
                            var isReferencing = relationship.Entity1IntersectAttribute == field1;

                            var r = new AssociateRequest
                            {
                                Relationship = new Relationship(relationship.SchemaName)
                                {
                                    PrimaryEntityRole =
                                    isReferencing ? EntityRole.Referencing : EntityRole.Referenced
                                },
                                Target = new EntityReference(type1, e.GetGuidField(field1)),
                                RelatedEntities = new EntityReferenceCollection(new[] { new EntityReference(type2, e.GetGuidField(field2)) })
                            };
                            return r;
                        })
                        .ToArray();

                    var associateMultipleResponses = XrmService.ExecuteMultiple(associateRequests);

                    i = 0;
                    foreach (var associateResponse in associateMultipleResponses)
                    {
                        var associationEntity = notYetAssociated[i];
                        if (associateResponse.Fault != null)
                        {
                            dataImportContainer.LogAssociationError(associationEntity, new FaultException<OrganizationServiceFault>(associateResponse.Fault, associateResponse.Fault.Message));
                        }
                        else
                        {
                            associationEntity.Id = Guid.NewGuid();
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

            var types = dataImportContainer.FieldsToRetry.Keys.Select(e => e.LogicalName).Distinct().ToArray();

            foreach(var type in types)
            {
                var thisTypeForRetry = dataImportContainer.FieldsToRetry.Where(kv => kv.Key.LogicalName == type).ToList();

                while (thisTypeForRetry.Any())
                {
                    var thisSetOfEntities = thisTypeForRetry
                        .Take(dataImportContainer.ExecuteMultipleSetSize)
                        .ToList();
                    var countThisSet = thisSetOfEntities.Count;
                    thisTypeForRetry.RemoveRange(0, countThisSet);

                    var distinctFields = thisSetOfEntities.SelectMany(kv => kv.Value).Distinct().ToArray();

                    var indexToUpdateCopy = new Dictionary<Entity, Entity>();
                    foreach (var kv in thisSetOfEntities)
                    {
                        indexToUpdateCopy.Add(kv.Key, new Entity(kv.Key.LogicalName) { Id = kv.Key.Id });
                    }

                    foreach (var field in distinctFields)
                    {
                        var itemsWithThisFieldPopulated = thisSetOfEntities
                            .Where(e => dataImportContainer.FieldsToRetry[e.Key].Contains(field))
                            .Select(e => e.Key)
                            .ToList();
                        ParseLookupFields(XrmRecordService, dataImportContainer, itemsWithThisFieldPopulated, new[] { field }, isRetry: true, allowAddForRetry: false, doWhenResolved: (e, f) => indexToUpdateCopy[e].SetField(f, e.GetField(f)));
                    }

                    var itemsForUpdate = indexToUpdateCopy.Where(kv => kv.Value.GetFieldsInEntity().Any()).ToArray();

                    if (itemsForUpdate.Any())
                    {
                        var updateEntities = itemsForUpdate.Select(kv => kv.Value).ToArray();
                        var responses = XrmService.UpdateMultiple(updateEntities, null, bypassWorkflowsAndPlugins: dataImportContainer.BypassFlowsPluginsAndWorkflows);

                        var i = 0;
                        foreach (var updateResponse in responses)
                        {
                            var updateEntity = updateEntities[i];
                            var originalEntity = itemsForUpdate[i].Key;
                            foreach (var updatedField in updateEntity.GetFieldsInEntity())
                                dataImportContainer.Response.RemoveFieldForRetry(originalEntity, updatedField);
                            if (updateResponse.Fault != null)
                            {
                                dataImportContainer.LogEntityError(originalEntity, new FaultException<OrganizationServiceFault>(updateResponse.Fault, updateResponse.Fault.Message));
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

                if (_cachedRecords.ContainsKey(recordType))
                {
                    _cachedRecords.Remove(recordType);
                }
                try
                {
                    dataImportContainer.LoadTargetsToCache(recordType);

                    var thisTypeEntities = new List<Entity>();
                    foreach (var entity in dataImportContainer.EntitiesToImport)
                    {
                        if (entity.LogicalName == recordType)
                        {
                            thisTypeEntities.Add(entity);
                        }
                    }
                    var importFieldsForEntity = dataImportContainer.GetFieldsToImport(thisTypeEntities, recordType).ToArray();

                    thisTypeEntities = OrderEntitiesForImport(dataImportContainer, thisTypeEntities, importFieldsForEntity);

                    var countRecordsToImport = thisTypeEntities.Count;
                    var countRecordsImported = 0;
                    var estimator = new TaskEstimator(countRecordsToImport);


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
                if (_cachedRecords.ContainsKey(recordType))
                {
                    _cachedRecords.Remove(recordType);
                }
            }
            dataImportContainer.Controller.TurnOffLevel2();
        }

        private static void ImportEntitiesNestedProcess(DataImportContainer dataImportContainer, string recordType, List<Entity> thisTypeEntities, string[] importFieldsForEntity, int countRecordsToImport, ref int countRecordsImported, TaskEstimator estimator, XrmRecordService xrmRecordService)
        {
            while (thisTypeEntities.Any())
            {
                var thisSetOfEntities = LoadNextSetToProcess(dataImportContainer, thisTypeEntities, xrmRecordService);
                var countThisSet = thisSetOfEntities.Count;
                try
                {
                    var matchDictionary = new Dictionary<Entity, Entity>();

                    MatchEntitiesToTarget(dataImportContainer, thisSetOfEntities, matchDictionary, xrmRecordService);

                    var currentEntityFields = thisSetOfEntities
                        .SelectMany(e => e.GetFieldsInEntity())
                        .Distinct()
                        .Where(f => !f.Contains(".") && importFieldsForEntity.Contains(f))
                        .ToArray();

                    var lookupFields = currentEntityFields
                        .Where(f => xrmRecordService.XrmService.IsLookup(f, recordType))
                        .ToArray();

                    ParseLookupFields(xrmRecordService, dataImportContainer, thisSetOfEntities, lookupFields, isRetry: false, allowAddForRetry: true);

                    var activityPartyFields = currentEntityFields
                        .Where(f => xrmRecordService.XrmService.IsActivityParty(f, recordType))
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

                        var dictionaryPartiesToParent = new Dictionary<Entity, Entity>();
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

                    var forCreateEntitiesCopy = new Dictionary<Entity, Entity>();
                    var forUpdateEntitiesCopy = new Dictionary<Entity, Entity>();

                    var recordTypeFileFields = xrmRecordService.XrmService
                        .GetEntityFieldMetadata(recordType)
                        .Where(fmt => fmt.Value is ImageAttributeMetadata || fmt.Value is FileAttributeMetadata)
                        .Select(kv => kv.Value)
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
                                    entity.SetField(field, theEmail.Replace("@", "_AT_") + "_@fakemaskedemail.com");
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
                            var copyEntity = XrmEntity.ReplicateToNewEntity(entity);
                            copyEntity.Id = entity.Id;
                            copyEntity.RemoveFields(copyEntity.GetFieldsInEntity().Except(fieldsToSet));
                            copyEntity.RemoveFields(copyEntity.GetFieldsInEntity().Where(f => copyEntity.GetField(f) == null));
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
                                    var oldValue = entity.GetField(f);
                                    var newValue = existingRecord.GetField(f);
                                    if (oldValue is EntityReference er
                                                && newValue is EntityReference erNew
                                                && er.Id == Guid.Empty && erNew.Id != Guid.Empty
                                                && er.Name == erNew.Name)
                                        return false;
                                    else
                                        return !XrmEntity.FieldsEqual(existingRecord.GetField(f), entity.GetField(f));
                                }).ToArray();
                            if (fieldsToSubmit.Any())
                            {
                                var copyEntity = XrmEntity.ReplicateToNewEntity(entity);
                                copyEntity.Id = entity.Id;
                                copyEntity.RemoveFields(copyEntity.GetFieldsInEntity().Except(fieldsToSubmit));
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
                            if (forCreate.Key.Contains("statuscode")
                                && forCreate.Key.GetOptionSetValue("statecode") > -1
                                    && (forCreate.Key.GetOptionSetValue("statecode") > 0
                                    || (forCreate.Key.LogicalName == Entities.product || forCreate.Key.GetOptionSetValue("statecode") != 2)))
                            {
                                forCreate.Key.RemoveFields(new[] { "statuscode" });
                            }
                        }
                        IEnumerable<ExecuteMultipleResponseItem> responses = null;
                        try
                        {
                            responses = xrmRecordService.XrmService.CreateMultiple(forCreateEntitiesCopy.Keys, bypassWorkflowsAndPlugins: dataImportContainer.BypassFlowsPluginsAndWorkflows);
                        }
                        catch (FaultException<OrganizationServiceFault> fex)
                        {
                            responses = forUpdateEntitiesCopy.Select(e => new ExecuteMultipleResponseItem() { Fault = fex.Detail }).ToArray();
                        }
                        catch (Exception ex)
                        {
                            responses = forUpdateEntitiesCopy.Select(e => new ExecuteMultipleResponseItem() { Fault = new OrganizationServiceFault { Message = ex.Message } });
                        }
                        var i = 0;
                        foreach (var createResponse in responses)
                        {
                            var originalEntity = forCreateEntitiesCopy.ElementAt(i).Value;
                            if (createResponse.Fault != null)
                            {
                                dataImportContainer.LogEntityError(originalEntity, new FaultException<OrganizationServiceFault>(createResponse.Fault, createResponse.Fault.Message));
                            }
                            else
                            {
                                originalEntity.Id = ((CreateResponse)createResponse.Response).id;
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
                            if (forUpdate.Key.Contains("statecode")
                                && _customSetStateConfigurations.ContainsKey(forUpdate.Key.LogicalName))
                            {
                                forUpdate.Key.RemoveFields(new[] { "statuscode", "statecode" });
                            }
                        }
                        IEnumerable<ExecuteMultipleResponseItem> responses = null;
                        try
                        {
                            responses = xrmRecordService.XrmService.UpdateMultiple(forUpdateEntitiesCopy.Keys, null, bypassWorkflowsAndPlugins: dataImportContainer.BypassFlowsPluginsAndWorkflows);
                        }
                        catch (FaultException<OrganizationServiceFault> fex)
                        {
                            responses = forUpdateEntitiesCopy.Select(e => new ExecuteMultipleResponseItem() { Fault = fex.Detail }).ToArray();
                        }
                        catch (Exception ex)
                        {
                            responses = forUpdateEntitiesCopy.Select(e => new ExecuteMultipleResponseItem() { Fault = new OrganizationServiceFault { Message = ex.Message } });
                        }
                        var i = 0;
                        foreach (var updateResponse in responses)
                        {
                            var originalEntity = forUpdateEntitiesCopy.ElementAt(i).Value;
                            if (updateResponse.Fault != null)
                            {
                                dataImportContainer.LogEntityError(originalEntity, new FaultException<OrganizationServiceFault>(updateResponse.Fault, updateResponse.Fault.Message));
                            }
                            else
                            {
                                dataImportContainer.Response.AddUpdated(originalEntity);
                                ImportFileFields(originalEntity, recordTypeFileFields, dataImportContainer, xrmRecordService);
                            }
                            i++;
                        }
                    }

                    var checkStateForEntities = new List<Entity>();
                    foreach (var entity in forCreateEntitiesCopy.Values.Union(forUpdateEntitiesCopy.Values).ToArray())
                    {
                        var isUpdate = matchDictionary.ContainsKey(entity);
                        if (!isUpdate)
                        {
                            if (dataImportContainer.Response.GetImportForType(entity.LogicalName).HasBeenCreated(entity.Id))
                            {
                                if (entity.GetOptionSetValue("statecode") > -1
                                    && (entity.GetOptionSetValue("statecode") > 0
                                    || (entity.LogicalName == Entities.product || entity.GetOptionSetValue("statecode") != 2)))
                                {
                                    checkStateForEntities.Add(entity);
                                }
                            }
                        }
                        else
                        {
                            var originalEntity = matchDictionary[entity];
                            if (entity.Contains("statecode") &&
                                (entity.GetOptionSetValue("statecode") != originalEntity.GetOptionSetValue("statecode")
                                    || (entity.Contains("statuscode") && entity.GetOptionSetValue("statuscode") != originalEntity.GetOptionSetValue("statuscode"))))
                            {
                                checkStateForEntities.Add(entity);
                            }
                        }
                    }
                    var setStateMessages = checkStateForEntities
                        .Select(e => GetSetStateRequest(e, dataImportContainer.BypassFlowsPluginsAndWorkflows))
                        .ToArray();
                    if (setStateMessages.Any())
                    {
                        var responses = xrmRecordService.XrmService.ExecuteMultiple(setStateMessages);
                        var i = 0;
                        foreach (var updateResponse in responses)
                        {
                            var originalEntity = checkStateForEntities.ElementAt(i);
                            if (updateResponse.Fault != null)
                            {
                                dataImportContainer.LogEntityError(originalEntity, new FaultException<OrganizationServiceFault>(updateResponse.Fault, updateResponse.Fault.Message));
                            }
                            else
                            {
                                dataImportContainer.Response.AddUpdated(originalEntity);
                            }
                            i++;
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

        private static bool ImportFileFields(Entity importEntity, IEnumerable<AttributeMetadata> recordTypeFileFields, DataImportContainer dataImportContainer, XrmRecordService xrmRecordService)
        {
            var fileFieldUpdated = false;
            foreach(var fileField in recordTypeFileFields)
            {
                var importBase64String = importEntity.GetStringField($"{fileField.LogicalName}.base64");
                var importFileName = importEntity.GetStringField($"{fileField.LogicalName}.filename");
                if (!string.IsNullOrWhiteSpace(importBase64String))
                {
                    try
                    {
                        string targetFileName = null;
                            var targetBase64String = xrmRecordService.XrmService.GetFileFieldBase64(importEntity.LogicalName, importEntity.Id, fileField.LogicalName, out targetFileName);
                        if (importBase64String != targetFileName || importBase64String != targetBase64String)
                        {
                            xrmRecordService.XrmService.SetFileFieldBase64(importEntity.LogicalName, importEntity.Id, fileField.LogicalName, importFileName, importBase64String);
                            fileFieldUpdated = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        var recordName = importEntity.GetStringField(xrmRecordService.GetPrimaryField(importEntity.LogicalName)) ?? importEntity.Id.ToString();
                        dataImportContainer.Response.AddImportError(importEntity,
                                 new DataImportResponseItem(importEntity.LogicalName,
                                 fileField.LogicalName, recordName, importFileName, "Error setting file", ex));
                    }
                }
            }
            return fileFieldUpdated;
        }

        private static void ParseLookupFields(XrmRecordService xrmRecordService, DataImportContainer dataImportContainer, IEnumerable<Entity> thisSetOfEntities, IEnumerable<string> lookupFields, bool isRetry, bool allowAddForRetry, Action<Entity, string> doWhenResolved = null,
            Action<Entity, string> doWhenNotResolved = null,
            Func<Entity, Entity> getPartyParent = null, string usetargetType = null, string usetargetField = null)
        {
            if (thisSetOfEntities.Any())
            {
                var recordType = thisSetOfEntities.First().LogicalName;
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
                                        || referenceType == targetType;
                                })
                                .ToArray();

                            var targetTypesConfig = xrmRecordService.GetTypeConfigs().GetFor(targetType);
                            var isCached = dataImportContainer.IsValidForCache(targetType);

                            //if has type config of not cached
                            //we will query the matches
                            var querySetResponses = targetTypesConfig != null || !isCached
                                ? xrmRecordService.XrmService.ExecuteMultiple(recordsToTry
                                    .Select(e => dataImportContainer.GetParseLookupQuery(e, lookupField, targetType, thisTargetField))
                                    .Select(q => new RetrieveMultipleRequest() { Query = q })
                                    .ToArray())
                                : new ExecuteMultipleResponseItem[0];

                            var i = 0;
                            foreach (var entity in recordsToTry)
                            {
                                var thisEntity = entity;
                                var referencedValue = thisEntity.GetLookupName(lookupField);
                                var referencedId = thisEntity.GetLookupGuid(lookupField) ?? Guid.Empty;
                                try
                                {
                                    IEnumerable<Entity> matchRecords = new Entity[0];
                                    if (querySetResponses.Any())
                                    {
                                        var thisOnesExecuteMultipleResponse = querySetResponses.ElementAt(i);
                                        if (thisOnesExecuteMultipleResponse.Fault != null)
                                            throw new Exception("Error Querying For Match - " + thisOnesExecuteMultipleResponse.Fault.Message);
                                        else
                                        {
                                            matchRecords = ((RetrieveMultipleResponse)thisOnesExecuteMultipleResponse.Response).EntityCollection.Entities;
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
                                    //else the cache will be used
                                    else
                                    {
                                        matchRecords = dataImportContainer.GetMatchingEntities(targetType, new Dictionary<string, object>
                                                    {
                                                        {  thisTargetPrimarykey, referencedId }
                                                    });
                                        if (!matchRecords.Any())
                                        {
                                            matchRecords = dataImportContainer.GetMatchingEntities(targetType, new Dictionary<string, object>
                                                    {
                                                        { thisTargetField, referencedValue }
                                                    });
                                            matchRecords = dataImportContainer.FilterForNameMatch(matchRecords);
                                        }
                                    }

                                    if (matchRecords.Count() > 1)
                                    {
                                        var caseMatch = matchRecords.Where(m => string.CompareOrdinal(referencedValue, m.GetStringField(thisTargetField)) == 0);
                                        var notCaseMatch = matchRecords.Where(m => string.CompareOrdinal(referencedValue, m.GetStringField(thisTargetField)) != 0);
                                        if (caseMatch.Count() == 1 && notCaseMatch.Any())
                                        {
                                            matchRecords = caseMatch.ToArray();
                                        }
                                        else
                                        {
                                            var activeMatches = matchRecords.Where(m => m.GetOptionSetValue("statecode") == 0);
                                            if (activeMatches.Count() == 1)
                                            {
                                                matchRecords = activeMatches.ToArray();
                                            }
                                            else
                                            {
                                                throw new Exception($"Multiple matches for  field {lookupField} named '{referencedValue}'. This field has not been set");
                                            }
                                        }
                                    }
                                    if (matchRecords.Count() == 1)
                                    {
                                        var matchedRecord = matchRecords.First();
                                        var matchedRecordEntityReference = matchedRecord.ToEntityReference();
                                        thisEntity.SetField(lookupField, matchedRecordEntityReference);
                                        string name = null;
                                        if(xrmRecordService.IsString(thisTargetField, matchedRecord.LogicalName))
                                        {
                                            name = matchedRecord.GetStringField(thisTargetField);
                                        }
                                        else
                                        {
                                            name = matchedRecord.GetStringField(xrmRecordService.GetPrimaryField(matchedRecordEntityReference.LogicalName));
                                        }
                                        matchedRecordEntityReference.Name = name;
                                        recordsNotYetResolved.Remove(thisEntity);
                                        doWhenResolved?.Invoke(thisEntity, lookupField);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    thisEntity.Attributes.Remove(lookupField);
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
                                var rowNumber = notResolved.Contains("Sheet.RowNumber")
                                    ? notResolved.GetInt("Sheet.RowNumber")
                                    : (int?)null;
                                var notResolvedLogEntity = getPartyParent != null
                                    ? getPartyParent(notResolved)
                                    : notResolved;
                                var notResolvedLogEntityPrimaryField = xrmRecordService.GetPrimaryField(notResolvedLogEntity.LogicalName);
                                dataImportContainer.Response.AddImportError(notResolvedLogEntity,
                                     new DataImportResponseItem(notResolvedLogEntity.LogicalName,
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
        private static List<Entity> LoadNextSetToProcess(DataImportContainer dataImportContainer, List<Entity> orderedEntitiesForImport, XrmRecordService xrmRecordService)
        {
            lock (_loadNextSetToProcessLock)
            {
                var thisSetOfEntities = new List<Entity>();
                if (orderedEntitiesForImport.Any())
                {
                    var recordType = orderedEntitiesForImport[0].LogicalName;
                    var takeSomeCountDown = dataImportContainer.ExecuteMultipleSetSize;
                    while (takeSomeCountDown > 0 && orderedEntitiesForImport.Any())
                    {
                        bool dontGetMoreThisSet = false;

                        var addToSet = orderedEntitiesForImport[0];

                        var referenceFields = addToSet
                            .Attributes
                            .Where(kv => kv.Value is EntityReference)
                            .Select(kv => kv.Value as EntityReference)
                            .ToArray();
                        foreach (var referenceField in referenceFields)
                        {
                            var logicalName = referenceField.LogicalName;
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
                                            (id != Guid.Empty && e.Id == id)
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

        private static void MatchEntitiesToTarget(DataImportContainer dataImportContainer, List<Entity> thisSetOfEntities, Dictionary<Entity, Entity> matchDictionary, XrmRecordService xrmRecordService)
        {
            if (!thisSetOfEntities.Any())
                return;
            var recordType = thisSetOfEntities[0].LogicalName;
            var primaryField = xrmRecordService.GetPrimaryField(recordType);

            var thisTypesConfig = xrmRecordService.GetTypeConfigs().GetFor(recordType);
            var isCached = dataImportContainer.IsValidForCache(recordType);

            //if has type config of not cached
            //we will query the matches
            var querySetResponses = thisTypesConfig != null || !isCached
                ? xrmRecordService.XrmService.ExecuteMultiple(thisSetOfEntities
                    .Select(e => dataImportContainer.GetMatchQueryExpression(e, dataImportContainer))
                    .Select(q => new RetrieveMultipleRequest() { Query = q })
                    .ToArray())
                : new ExecuteMultipleResponseItem[0];

            var i = 0;
            foreach (var entity in thisSetOfEntities.ToArray())
            {
                var thisEntity = entity;
                try
                {
                    IEnumerable<Entity> matchRecords = new Entity[0];
                    if (querySetResponses.Any())
                    {
                        var thisOnesExecuteMultipleResponse = querySetResponses.ElementAt(i);
                        if (thisOnesExecuteMultipleResponse.Fault != null)
                            throw new Exception("Error Querying For Match - " + thisOnesExecuteMultipleResponse.Fault.Message);
                        else
                        {
                            matchRecords = ((RetrieveMultipleResponse)thisOnesExecuteMultipleResponse.Response).EntityCollection.Entities;
                            if (matchRecords.Any(e => e.Id == entity.Id))
                                matchRecords = matchRecords.Where(e => e.Id == entity.Id).ToArray();
                        }
                    }
                    //else the cache will be used
                    else if (dataImportContainer.AltMatchKeyDictionary.ContainsKey(thisEntity.LogicalName))
                    {
                        var matchKeyFieldDictionary = dataImportContainer.AltMatchKeyDictionary[thisEntity.LogicalName]
                            .Distinct().ToDictionary(f => f.Key, f => thisEntity.GetField(f.Key));
                        if (matchKeyFieldDictionary.Any(kv => XrmEntity.FieldsEqual(null, kv.Value)))
                        {
                            throw new Exception("Match Key Field Is Empty");
                        }
                        matchRecords = dataImportContainer.GetMatchingEntities(thisEntity.LogicalName, matchKeyFieldDictionary);
                    }
                    else if (dataImportContainer.MatchOption == MatchOption.PrimaryKeyThenName || thisTypesConfig != null)
                    {
                        matchRecords = dataImportContainer.GetMatchingEntities(thisEntity.LogicalName, new Dictionary<string, object>
                            {
                                {  xrmRecordService.GetPrimaryKey(thisEntity.LogicalName), thisEntity.Id }
                            });
                        if (!matchRecords.Any())
                        {
                            matchRecords = dataImportContainer.GetMatchingEntities(thisEntity.LogicalName, new Dictionary<string, object>
                                        {
                                            { primaryField, thisEntity.GetStringField(primaryField) }
                                        });
                            matchRecords = dataImportContainer.FilterForNameMatch(matchRecords);
                        }
                    }
                    else if (dataImportContainer.MatchOption == MatchOption.PrimaryKeyOnly && thisEntity.Id != Guid.Empty)
                    {
                        matchRecords = dataImportContainer.GetMatchingEntities(thisEntity.LogicalName, new Dictionary<string, object>
                                    {
                                        {  xrmRecordService.GetPrimaryKey(thisEntity.LogicalName), thisEntity.Id }
                                    });
                    }

                    //special case for business unit
                    if (!matchRecords.Any() && thisEntity.LogicalName == Entities.businessunit && thisEntity.GetField(Fields.businessunit_.parentbusinessunitid) == null)
                    {
                        matchRecords = new[] { dataImportContainer.GetRootBusinessUnit() };
                    }

                    //verify and process match results
                    if (!matchRecords.Any() && dataImportContainer.UpdateOnly)
                    {
                        throw new Exception("Updates Only And No Matching Record Found");
                    }
                    if(dataImportContainer.AltMatchKeyDictionary.ContainsKey(thisEntity.LogicalName))
                    {
                        var caseSensitiveMatches = dataImportContainer.AltMatchKeyDictionary[thisEntity.LogicalName]
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
                        var matchStringFields = (dataImportContainer.AltMatchKeyDictionary.ContainsKey(thisEntity.LogicalName)
                            ? dataImportContainer.AltMatchKeyDictionary[thisEntity.LogicalName].Select(kv => kv.Key)
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
                        if (thisEntity.Id != Guid.Empty)
                            dataImportContainer.IdSwitches[recordType].Add(thisEntity.Id, matchRecord.Id);
                        thisEntity.Id = matchRecord.Id;
                        thisEntity.SetField(xrmRecordService.GetPrimaryKey(thisEntity.LogicalName), thisEntity.Id);
                        if (thisTypesConfig != null)
                        {
                            if (thisTypesConfig.ParentLookupField != null)
                                thisEntity.SetField(thisTypesConfig.ParentLookupField, matchRecord.GetField(thisTypesConfig.ParentLookupField));
                            if (thisTypesConfig.UniqueChildFields != null)
                            {
                                foreach (var childField in thisTypesConfig.UniqueChildFields)
                                {
                                    var oldValue = thisEntity.GetField(childField);
                                    var newValue = matchRecord.GetField(childField);
                                    if (oldValue is EntityReference oldEr
                                        && newValue is EntityReference newEr
                                        && newEr.Name == null)
                                    {
                                        //this just fixing case on notes where the new query didnt populate trhe reference name
                                        newEr.Name = oldEr.Name;
                                    }
                                    thisEntity.SetField(childField, matchRecord.GetField(childField));
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

        private List<Entity> OrderEntitiesForImport(DataImportContainer dataImportContainer, List<Entity> thisTypeEntities, IEnumerable<string> importFieldsForEntity)
        {
            var orderedEntities = new List<Entity>();
            if (thisTypeEntities.Any())
            {
                var recordType = thisTypeEntities.First().LogicalName;
                var primaryField = XrmService.GetPrimaryNameField(recordType);
                var ignoreFields = dataImportContainer.GetIgnoreFields(recordType);
                var fieldsDontExist = dataImportContainer.GetFieldsInEntities(thisTypeEntities)
                    .Where(f => !f.Contains("."))
                    .Where(f => !XrmService.FieldExists(f, recordType))
                    .Where(f => !ignoreFields.Contains(f))
                    .Distinct()
                    .ToArray();
                foreach (var field in fieldsDontExist)
                {
                    dataImportContainer.Response.AddImportError(
                            new DataImportResponseItem(recordType, field, null, null,
                            string.Format("Field {0} On Entity {1} Doesn't Exist In Target Instance And Will Be Ignored", field, recordType),
                            new NullReferenceException(string.Format("Field {0} On Entity {1} Doesn't Exist In Target Instance And Will Be Ignored", field, recordType))));
                }

                var selfReferenceFields = importFieldsForEntity.Where(
                            f =>
                                XrmService.IsLookup(f, recordType) &&
                                XrmService.GetLookupTargetEntity(f, recordType) == recordType).ToArray();
                if (!selfReferenceFields.Any())
                {
                    orderedEntities = thisTypeEntities.ToList();
                }
                else
                {
                    foreach (var entity in thisTypeEntities)
                    {
                        var isAdded = false;
                        foreach (var entity2 in orderedEntities)
                        {
                            foreach (var selfReferenceField in selfReferenceFields)
                            {
                                var id = entity2.GetLookupGuid(selfReferenceField);
                                var name = entity2.GetLookupName(selfReferenceField);
                                if (id == entity.Id || (id == Guid.Empty && name == entity.GetStringField(primaryField)))
                                {
                                    orderedEntities.Insert(orderedEntities.IndexOf(entity2), entity);
                                    isAdded = true;
                                    break;
                                }
                            }
                            if (isAdded)
                            {
                                break;
                            }
                        }
                        if (!isAdded)
                        {
                            orderedEntities.Add(entity);
                        }
                        dataImportContainer.Controller.LogLiteral($"Sorting for import {orderedEntities.Count}/{thisTypeEntities.Count}");
                        dataImportContainer.Controller.UpdateLevel2Progress(0, 1, $"Sorting for import {orderedEntities.Count}/{thisTypeEntities.Count}");
                    }
                }
            }
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
                var thatTypeEntities = dataImportContainer.EntitiesToImport.Where(e => e.LogicalName == type).ToList();
                var fields = dataImportContainer.GetFieldsToImport(thatTypeEntities, type)
                    .Where(f => XrmService.FieldExists(f, type) &&
                        (XrmService.IsLookup(f, type) || XrmService.IsActivityParty(f, type)));
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
                    var thatTypeEntities = dataImportContainer.EntitiesToImport.Where(e => e.LogicalName == otherType).ToList();
                    foreach (var field in fields)
                    {
                        if (thatTypeEntities.Any(e =>
                            (XrmService.IsLookup(field, otherType) && e.GetLookupType(field).Split(',').Contains(type))
                            || (XrmService.IsActivityParty(field, otherType) && e.GetActivityParties(field).Any(p => p.GetLookupType(Fields.activityparty_.partyid) == type))))
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

        private static OrganizationRequest GetSetStateRequest(Entity thisEntity, bool bypassWorkflowsAndPlugins = false)
        {
            if(_customSetStateConfigurations.ContainsKey(thisEntity.LogicalName))
            {
                return _customSetStateConfigurations[thisEntity.LogicalName](thisEntity);
            }
            else
            {
                var theState = thisEntity.GetOptionSetValue("statecode");
                var theStatus = thisEntity.GetOptionSetValue("statuscode");
                var request = new SetStateRequest()
                {
                    EntityMoniker = thisEntity.ToEntityReference(),
                    State = new OptionSetValue(theState),
                    Status = new OptionSetValue(theStatus)
                };
                if (bypassWorkflowsAndPlugins)
                {
                    request.Parameters.Add("SuppressCallbackRegistrationExpanderJob", true);
                    request.Parameters.Add("BypassBusinessLogicExecution", "CustomSync,CustomAsync");
                }
                return request;
            }
        }

        private static Dictionary<string, Func<Entity, OrganizationRequest>> _customSetStateConfigurations = new Dictionary<string, Func<Entity, OrganizationRequest>>
        {
            {
                Entities.incident,
                (e) =>
                {
                    var theState = e.GetOptionSetValue("statecode");
                    var theStatus = e.GetOptionSetValue("statuscode");
                    if (theState == OptionSets.Case.Status.Resolved)
                    {
                        var closeIt = new Entity(Entities.incidentresolution);
                        closeIt.SetLookupField(Fields.incidentresolution_.incidentid, e);
                        closeIt.SetField(Fields.incidentresolution_.subject, "Close By Data Import");
                        return new CloseIncidentRequest
                        {
                            IncidentResolution = closeIt,
                            Status = new OptionSetValue(theStatus)
                        };
                    }
                    else
                    {
                        return new SetStateRequest()
                        {
                            EntityMoniker = e.ToEntityReference(),
                            State = new OptionSetValue(theState),
                            Status = new OptionSetValue(theStatus)
                        };
                    }
                }
            }
        };

        private static void PopulateRequiredCreateFields(DataImportContainer dataImportContainer, Entity thisEntity, List<string> fieldsToSet, XrmRecordService xrmRecordService)
        {
            if (thisEntity.LogicalName == Entities.team
                && !fieldsToSet.Contains(Fields.team_.businessunitid)
                && xrmRecordService.FieldExists(Fields.team_.businessunitid, Entities.team))
            {
                thisEntity.SetLookupField(Fields.team_.businessunitid, dataImportContainer.GetRootBusinessUnit().Id, Entities.businessunit);
                fieldsToSet.Add(Fields.team_.businessunitid);
                if (dataImportContainer.FieldsToRetry.ContainsKey(thisEntity)
                    && dataImportContainer.FieldsToRetry[thisEntity].Contains(Fields.team_.businessunitid))
                    dataImportContainer.FieldsToRetry[thisEntity].Remove(Fields.team_.businessunitid);
            }
            if (thisEntity.LogicalName == Entities.subject
                    && !fieldsToSet.Contains(Fields.subject_.featuremask)
                    && xrmRecordService.FieldExists(Fields.subject_.featuremask, Entities.subject))
            {
                thisEntity.SetField(Fields.subject_.featuremask, 1);
                fieldsToSet.Add(Fields.subject_.featuremask);
                if (dataImportContainer.FieldsToRetry.ContainsKey(thisEntity)
                    && dataImportContainer.FieldsToRetry[thisEntity].Contains(Fields.subject_.featuremask))
                    dataImportContainer.FieldsToRetry[thisEntity].Remove(Fields.subject_.featuremask);
            }
            if (thisEntity.LogicalName == Entities.uomschedule)
            {
                fieldsToSet.Add(Fields.uomschedule_.baseuomname);
            }
            if (thisEntity.LogicalName == Entities.uom)
            {
                var unitGroupName = thisEntity.GetLookupName(Fields.uom_.uomscheduleid);
                if (string.IsNullOrWhiteSpace(unitGroupName))
                {
                    throw new NullReferenceException($"Error The {xrmRecordService.GetFieldLabel(Fields.uom_.uomscheduleid, Entities.uom)} Name Is Not Populated");
                }
                fieldsToSet.Add(Fields.uom_.uomscheduleid);

                var baseUnitName = thisEntity.GetLookupName(Fields.uom_.baseuom);
                var baseUnitMatchQuery = XrmService.BuildQuery(Entities.uom, null, null, null);
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
                    baseUnitMatchQuery.Criteria.AddCondition(new ConditionExpression(Fields.uom_.name, ConditionOperator.Equal, baseUnitName));
                    var unitGroupLink = baseUnitMatchQuery.AddLink(Entities.uomschedule, Fields.uom_.uomscheduleid, Fields.uomschedule_.uomscheduleid);
                    unitGroupLink.LinkCriteria.AddCondition(new ConditionExpression(Fields.uomschedule_.name, ConditionOperator.Equal, unitGroupName));
                }
                var baseUnitMatches = xrmRecordService.XrmService.RetrieveAll(baseUnitMatchQuery);
                if (!baseUnitMatches.Any())
                {
                    throw new Exception($"Could Not Identify The {xrmRecordService.GetFieldLabel(Fields.uom_.baseuom, Entities.uom)} {baseUnitName}. No Match Found For The {xrmRecordService.GetFieldLabel(Fields.uom_.uomscheduleid, Entities.uom)}");
                }
                if (baseUnitMatches.Count() > 1)
                {
                    throw new Exception($"Could Not Identify The {xrmRecordService.GetFieldLabel(Fields.uom_.baseuom, Entities.uom)} {baseUnitName}. Multiple Matches Found For The {xrmRecordService.GetFieldLabel(Fields.uom_.uomscheduleid, Entities.uom)}");
                }
                thisEntity.SetLookupField(Fields.uom_.baseuom, baseUnitMatches.First());
                thisEntity.SetField(Fields.uom_.uomscheduleid, baseUnitMatches.First().GetField(Fields.uom_.uomscheduleid));
                fieldsToSet.Add(Fields.uom_.baseuom);
            }
            if (thisEntity.LogicalName == Entities.product)
            {
                var unitGroupId = thisEntity.GetLookupGuid(Fields.product_.defaultuomscheduleid);
                if(unitGroupId.HasValue)
                    fieldsToSet.Add(Fields.product_.defaultuomscheduleid);
                var unitId = thisEntity.GetLookupGuid(Fields.product_.defaultuomid);
                if (unitId.HasValue)
                    fieldsToSet.Add(Fields.product_.defaultuomid);
            }
            if (thisEntity.LogicalName == Entities.list)
            {
                if(!fieldsToSet.Contains(Fields.list_.createdfromcode))
                {
                    if(!thisEntity.Contains(Fields.list_.createdfromcode))
                    {
                        throw new NullReferenceException($"{xrmRecordService.GetFieldLabel(Fields.list_.createdfromcode, Entities.list)} is required");
                    }
                    fieldsToSet.Add(Fields.list_.createdfromcode);
                }
                if (!fieldsToSet.Contains(Fields.list_.type))
                {
                    if (!thisEntity.Contains(Fields.list_.type))
                    {
                        throw new NullReferenceException($"{xrmRecordService.GetFieldLabel(Fields.list_.type, Entities.list)} is required");
                    }
                    fieldsToSet.Add(Fields.list_.type);
                }
            }
            if (thisEntity.LogicalName == Entities.activitymimeattachment)
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

        private static void CheckThrowValidForCreate(Entity thisEntity, List<string> fieldsToSet, XrmRecordService xrmRecordService)
        {
            if (thisEntity != null)
            {
                switch (thisEntity.LogicalName)
                {
                    case Entities.annotation:
                        {
                            if (!fieldsToSet.Contains(Fields.annotation_.objectid))
                                throw new NullReferenceException(string.Format("Cannot create {0} {1} as its parent {2} does not exist"
                                    , xrmRecordService.GetDisplayName(thisEntity.LogicalName), thisEntity.GetStringField(xrmRecordService.GetPrimaryField(thisEntity.LogicalName))
                                    , thisEntity.GetStringField(Fields.annotation_.objecttypecode) != null ? xrmRecordService.GetDisplayName(thisEntity.GetStringField(Fields.annotation_.objecttypecode)) : "Unknown Type"));
                            break;
                        }
                    case Entities.productpricelevel:
                        {
                            if (!fieldsToSet.Contains(Fields.productpricelevel_.pricelevelid))
                                throw new NullReferenceException(string.Format("Cannot create {0} {1} as its parent {2} is empty"
                                    , xrmRecordService.GetDisplayName(thisEntity.LogicalName), thisEntity.GetStringField(xrmRecordService.GetPrimaryField(thisEntity.LogicalName))
                                    , xrmRecordService.GetDisplayName(Entities.pricelevel)));
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
