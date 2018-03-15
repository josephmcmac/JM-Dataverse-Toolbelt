#region

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using $safeprojectname$.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading;

#endregion

namespace $safeprojectname$.Xrm
{
    public class XrmService : IOrganizationService
    {
        public static DateTime MinCrmDateTime = DateTime.SpecifyKind(new DateTime(1900, 1, 1), DateTimeKind.Utc);

        private readonly SortedDictionary<string, List<AttributeMetadata>>
            _entityFieldMetadata = new SortedDictionary<string, List<AttributeMetadata>>();

        private readonly List<EntityMetadata> _entityMetadata = new List<EntityMetadata>();

        private readonly SortedDictionary<string, RelationshipMetadataBase[]> _entityRelationships
            = new SortedDictionary<string, RelationshipMetadataBase[]>();

        private readonly Object _lockObject = new Object();

        private readonly List<ManyToManyRelationshipMetadata> _relationshipMetadata = new
            List<ManyToManyRelationshipMetadata>();

        private LogController _controller;


        /// <summary>
        ///     DONT USE CALL THE EXECUTE METHOD OR THE PROPERTY
        /// </summary>
        private IOrganizationService _service;

        internal XrmService(IOrganizationService actualService, LogController uiController)
        {
            _service = actualService;
            _controller = uiController;
            if (_controller == null)
                _controller = new LogController();
        }

        public XrmService(IXrmConfiguration crmConfig, LogController controller)
        {
            XrmConfiguration = crmConfig;
            _controller = controller;
        }

        public XrmService(IXrmConfiguration crmConfig)
        {
            XrmConfiguration = crmConfig;
            _controller = new LogController();
        }


        private object LockObject
        {
            get { return _lockObject; }
        }

        private SortedDictionary<string, List<AttributeMetadata>> EntityFieldMetadata
        {
            get { return _entityFieldMetadata; }
        }

        private List<EntityMetadata> EntityMetadata
        {
            get { return _entityMetadata; }
        }

        private List<ManyToManyRelationshipMetadata> RelationshipMetadata
        {
            get { return _relationshipMetadata; }
        }

        private SortedDictionary<string, RelationshipMetadataBase[]> EntityRelationships
        {
            get { return _entityRelationships; }
        }

        private LogController UIController
        {
            get { return _controller; }
            set { _controller = value; }
        }

        private IXrmConfiguration XrmConfiguration { get; set; }

        /// <summary>
        ///     DON'T USE CALL THE EXECUTE METHOD
        /// </summary>
        private IOrganizationService Service
        {
            get
            {
                lock (_lockObject)
                {
                    if (_service == null)
                        _service = XrmConnection.GetOrgServiceProxy(XrmConfiguration);
                }
                return _service;
            }
            set { _service = value; }
        }

        public virtual OrganizationResponse Execute(OrganizationRequest request)
        {
            var requestDescription = GetRequestDescription(request);
            _controller.LogDetail("Executing crm request - " + requestDescription);

            OrganizationResponse result;
            try
            {
                result = Service.Execute(request);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                lock (_lockObject)
                {
                    //if we don't have a XrmConfiguration We Are Probably Inside A Transaction So Don't Bother Retry
                    if (XrmConfiguration == null)
                        throw;
                    //I have seen this error thrown when the sand box server is busy, and subsequent calls are successful. Going to add a retry
                    _controller.LogLiteral("Received FaultException<OrganizationServiceFault> retrying.. Details:" +
                                          ex.DisplayString());
                    Thread.Sleep(50);
                    result = Service.Execute(request);
                    _controller.LogLiteral("Successful retry");
                }
            }
            catch (CommunicationException ex)
            {
                lock (_lockObject)
                {
                    //Error was being thrown after service running overnight with no activity
                    //adding logic to reconnect when this error thrown
                    _controller.LogLiteral("Received " + ex.GetType().Name + " checking for Crm config to reconnect..");
                    if (XrmConfiguration != null)
                    {
                        _controller.LogLiteral("Crm config found attempting to reconnect..");
                        Service = XrmConnection.GetOrgServiceProxy(XrmConfiguration);
                        result = Service.Execute(request);
                        _controller.LogLiteral("Reconnected..");
                    }
                    else
                    {
                        _controller.LogLiteral("No Crm config found unable to reconnect..");
                        throw;
                    }
                }
            }

            requestDescription = GetRequestDescription(request);
            _controller.LogDetail("Received crm response - " + requestDescription);

            return result;
        }

        public virtual ManyToManyRelationshipMetadata GetRelationshipMetadata(string relationship)
        {
            lock (LockObject)
            {
                if (RelationshipMetadata.All(rm => rm.SchemaName != relationship))
                {
                    var request = new RetrieveRelationshipRequest
                    {
                        Name = relationship
                    };
                    var response = (RetrieveRelationshipResponse)Execute(request);
                    RelationshipMetadata.Add((ManyToManyRelationshipMetadata)response.RelationshipMetadata);
                }
                return RelationshipMetadata.Single(rm => rm.SchemaName == relationship);
            }
        }

        public virtual EntityMetadata GetEntityMetadata(string entity)
        {
            lock (LockObject)
            {
                if (!EntityMetadata.Any(em => em.LogicalName == entity))
                {
                    _controller.LogLiteral("Retrieving " + entity + " entity metadata");
                    var request = new RetrieveEntityRequest
                    {
                        EntityFilters = EntityFilters.Default,
                        LogicalName = entity
                    };
                    var response = (RetrieveEntityResponse)Execute(request);
                    _controller.LogLiteral("Retrieved " + entity + " entity metadata");
                    EntityMetadata.Add(response.EntityMetadata);
                }
            }
            return EntityMetadata.First(em => em.LogicalName == entity);
        }

        public virtual AttributeMetadata GetFieldMetadata(string field, string entity)
        {
            var entityFieldMetadata = GetEntityFieldMetadata(entity);

            if (entityFieldMetadata.Any(efm => efm.LogicalName == field))
            {
                var fieldMetadata = entityFieldMetadata.First(efm => efm.LogicalName == field);
                return fieldMetadata;
            }

            throw new Exception("Error Getting field metadata\nEntity: " + entity + "\nField: " + field);
        }

        public string GetFieldLabel(string field, string entity)
        {
            return GetLabelDisplay(GetFieldMetadata(field, entity).DisplayName);
        }

        private string GetLabelDisplay(Label label)
        {
            return label.LocalizedLabels.Any(l => l.LanguageCode == 1033)
                ? label.LocalizedLabels.First(l => l.LanguageCode == 1033).Label
                : string.Empty;
        }

        public string GetOptionLabel(int optionValue, string field, string entity)
        {
            var metadata = GetFieldMetadata(field, entity);
            foreach (var option in ((EnumAttributeMetadata)metadata).OptionSet.Options)
            {
                if (option.Value == optionValue)
                    return GetOptionLabel(option);
            }
            throw new ArgumentOutOfRangeException("Field " + field + " in entity " + entity +
                                                  " does not contain option with value " + optionValue);
        }

        private static string GetOptionLabel(OptionMetadata option)
        {
            return option.Label.LocalizedLabels[0].Label;
        }

        public string GetEntityDisplayName(string recordType)
        {
            return GetLabelDisplay(GetEntityMetadata(recordType).DisplayName);
        }

        public string GetEntityCollectionName(string recordType)
        {
            return GetLabelDisplay(GetEntityMetadata(recordType).DisplayCollectionName);
        }

        public Guid WhoAmI()
        {
            return ((WhoAmIResponse)Execute(new WhoAmIRequest())).UserId;
        }

        public IEnumerable<Entity> RetrieveAll(QueryExpression query)
        {
            query.PageInfo.PageNumber = 1;
            var response = RetrieveMultiple(query);
            var result = response.Entities.ToArray();

            //If there is more than one page of records then keep retrieving until we get them all
            if (response.MoreRecords)
            {
                var tempHolder = new List<Entity>(result);
                while (response.MoreRecords)
                {
                    query.PageInfo.PagingCookie = response.PagingCookie;
                    query.PageInfo.PageNumber = query.PageInfo.PageNumber + 1;
                    response = RetrieveMultiple(query);
                    tempHolder.AddRange(response.Entities.ToArray());
                }
                result = tempHolder.ToArray();
            }
            return result;
        }

        public void ClearCache()
        {
            lock (LockObject)
            {
                EntityFieldMetadata.Clear();
                EntityMetadata.Clear();
                RelationshipMetadata.Clear();
                EntityRelationships.Clear();
            }
        }

        private object CreateOptionSetValue(int value)
        {
            return new OptionSetValue(value);
        }

        private EntityReference CreateLookup(string targetType, Guid id)
        {
            return new EntityReference(targetType, id);
        }

        public Entity Retrieve(string entityType, Guid id, IEnumerable<string> fields = null)
        {
            return Retrieve(entityType, id, CreateColumnSet(fields));
        }

        private ColumnSet CreateColumnSet(IEnumerable<string> fields)
        {
            if (fields != null)
                return new ColumnSet(fields.ToArray());
            else
                return new ColumnSet(true);
        }

        public Entity GetFirst(string entityType, string fieldName, object fieldValue, IEnumerable<string> fields = null)
        {
            var query = new QueryExpression(entityType);
            query.Criteria.AddCondition(fieldName, ConditionOperator.Equal, fieldValue);
            query.ColumnSet = CreateColumnSet(fields);
            return RetrieveFirst(query);
        }

        public Entity RetrieveFirst(QueryExpression query)
        {
            var r = RetrieveFirstX(query, 1);
            return !r.Any() ? null : r.ElementAt(0);
        }

        private IEnumerable<Entity> RetrieveFirstX(QueryExpression query, int x)
        {
            query.PageInfo.PageNumber = 1;
            if (x >= 0)
                query.PageInfo.Count = x;
            var response = RetrieveMultiple(query);
            var result = response.Entities.ToArray();

            //If there is more than one page of records then keep retrieving until we get them all
            if (response.MoreRecords)
            {
                var tempHolder = new List<Entity>(result);
                while (response.MoreRecords && (tempHolder.Count < x || x < 0))
                {
                    query.PageInfo.PagingCookie = response.PagingCookie;
                    query.PageInfo.PageNumber = query.PageInfo.PageNumber + 1;
                    response = RetrieveMultiple(query);
                    tempHolder.AddRange(response.Entities.ToArray());
                }
                result = tempHolder.ToArray();
            }
            return x >= 0 ? result.Take(x).ToArray() : result;
        }

        public void SetState(string entityType, Guid id, int state, int status = -1)
        {
            var setStateReq = new SetStateRequest
            {
                EntityMoniker = new EntityReference(entityType, id),
                State = new OptionSetValue(state),
                Status = new OptionSetValue(status)
            };

            Execute(setStateReq);
        }

        public IEnumerable<Entity> RetrieveAllEntityType(string entityType, IEnumerable<string> fields = null)
        {
            return RetrieveAll(new QueryExpression(entityType) { ColumnSet = CreateColumnSet(fields) });
        }

        public void SetField(string entityType, Guid guid, string fieldName, object value)
        {
            var entity = new Entity(entityType) { Id = guid };
            XrmEntity.SetField(entity, fieldName, value);
            Update(entity);
        }

        public IEnumerable<Entity> RetrieveAllAndConditions(string entityName, IEnumerable<ConditionExpression> conditions, IEnumerable<string> fields = null)
        {
            var query = BuildQuery(entityName, fields, conditions: conditions);
            return RetrieveAll(query);
        }

        public IEnumerable<Entity> RetrieveAllOrConditions(string entityName, IEnumerable<ConditionExpression> orConditions, IEnumerable<string> fields = null)
        {
            var filters = orConditions
                .Select(c =>
                {
                    var f = new FilterExpression();
                    f.AddCondition(c);
                    return f;
                }
                );
            return RetrieveAllOrFilters(entityName, filters, fields);
        }

        public IEnumerable<Entity> RetrieveAllOrFilters(string entityName, IEnumerable<FilterExpression> orFilters, IEnumerable<string> fields = null)
        {
            var results = new Dictionary<Guid, Entity>();
            var tempFilters = new List<FilterExpression>(orFilters);
            while (tempFilters.Any())
            {
                var i = 0;
                var query = new QueryExpression(entityName);
                query.ColumnSet = CreateColumnSet(fields);
                query.Criteria.FilterOperator = LogicalOperator.Or;
                while (tempFilters.Any() && i < 200)
                {
                    var filter = tempFilters.ElementAt(0);
                    tempFilters.RemoveAt(0);
                    query.Criteria.AddFilter(filter);
                    i++;
                }
                foreach (var entity in RetrieveAll(query))
                {
                    if (!results.ContainsKey(entity.Id))
                        results.Add(entity.Id, entity);
                }
            }
            return results.Values;
        }

        public void Assign(Entity entity, Guid ownerId, string ownerType)
        {
            var request = new AssignRequest
            {
                Assignee = CreateLookup(ownerType, ownerId),
                Target = new EntityReference(entity.LogicalName, entity.Id)
            };
            Execute(request);
        }

        public Guid Create(Entity entity, params string[] fieldsToSubmit)
        {
            if (fieldsToSubmit == null)
                return Create(entity);
            if (fieldsToSubmit.Any())
            {
                var submissionEntity = new Entity(entity.LogicalName) { Id = entity.Id };
                foreach (var field in fieldsToSubmit)
                {
                    if (entity.Contains(field))
                        submissionEntity.SetField(field, entity.GetField(field));
                }
                return Create(submissionEntity);
            }
            else
                throw new Exception($"{fieldsToSubmit} Is Empty");
        }

        #region standard methods

        public void Associate(string entityName, Guid entityId, Relationship relationship,
            EntityReferenceCollection relatedEntities)
        {
            if (relatedEntities.Count > 0)
            {
                var request = new AssociateRequest
                {
                    Target = CreateLookup(entityName, entityId),
                    Relationship = relationship,
                    RelatedEntities = relatedEntities
                };
                Execute(request);
            }
        }

        public Guid Create(Entity entity)
        {
            var request = new CreateRequest
            {
                Target = entity
            };
            return ((CreateResponse)Execute(request)).id;
        }

        public void Delete(string entityName, Guid id)
        {
            var request = new DeleteRequest
            {
                Target = CreateLookup(entityName, id)
            };
            Execute(request);
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship,
            EntityReferenceCollection relatedEntities)
        {
            if (relatedEntities.Count > 0)
            {
                var request = new DisassociateRequest
                {
                    Target = CreateLookup(entityName, entityId),
                    Relationship = relationship,
                    RelatedEntities = relatedEntities
                };
                Execute(request);
            }
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            var request = new RetrieveRequest
            {
                ColumnSet = columnSet,
                Target = CreateLookup(entityName, id)
            };
            return ((RetrieveResponse)Execute(request)).Entity;
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            var request = new RetrieveMultipleRequest
            {
                Query = query
            };
            return ((RetrieveMultipleResponse)Execute(request)).EntityCollection;
        }

        public void Update(Entity entity)
        {
            var request = new UpdateRequest
            {
                Target = entity
            };
            Execute(request);
        }

        public void Associate(string relationshipName, Guid idFrom, string typeFrom, string keyAttributeFrom, string typeTo, string keyAttributeTo, IEnumerable<Guid> relatedEntities, bool isReferencing = false)
        {
            var relationship = new Relationship(relationshipName)
            {
                PrimaryEntityRole =
                    isReferencing ? EntityRole.Referencing : EntityRole.Referenced
            };

            var entityReferenceCollection = new EntityReferenceCollection();
            foreach (var id in relatedEntities.Distinct())
                entityReferenceCollection.Add(CreateLookup(typeTo, id));

            Associate(typeFrom, idFrom, relationship, entityReferenceCollection);
        }

        public void Disassociate(string relationshipName, Guid idFrom, string typeFrom, string keyAttributeFrom, string typeTo, string keyAttributeTo, IEnumerable<Guid> relatedEntities, bool isReferencing = false)
        {
            var relationship = new Relationship(relationshipName)
            {
                PrimaryEntityRole =
                    isReferencing ? EntityRole.Referencing : EntityRole.Referenced
            };

            var entityReferenceCollection = new EntityReferenceCollection();
            foreach (var id in relatedEntities)
                entityReferenceCollection.Add(CreateLookup(typeTo, id));

            Disassociate(typeFrom, idFrom, relationship, entityReferenceCollection);
        }

        public void Delete(Entity entity)
        {
            Delete(entity.LogicalName, entity.Id);
        }

        public object LookupField(string entityType, Guid id, string fieldName)
        {
            return XrmEntity.GetField(Retrieve(entityType, id, new[] { fieldName }), fieldName);
        }

        private static string GetRequestDescription(OrganizationRequest request)
        {
            var result = request.GetType().Name;
            if (request is CreateRequest)
            {
                return result + " - Type = " + ((CreateRequest)request).Target.LogicalName;
            }
            else if (request is UpdateRequest)
            {
                var tRequest = ((UpdateRequest)request);
                return result + " Type = " + tRequest.Target.LogicalName + ", Id = " + tRequest.Target.Id;
            }
            else if (request is RetrieveRequest)
            {
                var tRequest = ((RetrieveRequest)request);
                return result + " Type = " + tRequest.Target.LogicalName + ", Id = " + tRequest.Target.Id;
            }
            else if (request is RetrieveMultipleRequest)
            {
                var tRequest = ((RetrieveMultipleRequest)request);
                if (tRequest.Query is QueryExpression)
                    return result + " Type = " + ((QueryExpression)tRequest.Query).EntityName;
            }
            else if (request is RetrieveEntityRequest)
            {
                var tRequest = ((RetrieveEntityRequest)request);
                return result + " Type = " + tRequest.LogicalName + ", Filters = " + tRequest.EntityFilters;
            }
            else if (request is AssociateRequest)
            {
                var tRequest = ((AssociateRequest)request);
                return result + " Relationship = " + tRequest.Relationship.SchemaName + ", Type = " +
                       tRequest.Target.LogicalName + ", Id = " + tRequest.Target.Id + ", Related = " +
                       String.Join<Guid?>(", ", tRequest.RelatedEntities.Select(XrmEntity.GetLookupGuid));
            }
            else if (request is DisassociateRequest)
            {
                var tRequest = ((DisassociateRequest)request);
                return result + " Relationship = " + tRequest.Relationship.SchemaName + ", Type = " +
                       tRequest.Target.LogicalName + ", Id = " + tRequest.Target.Id + ", Related = " +
                       String.Join<Guid?>(", ", tRequest.RelatedEntities.Select(XrmEntity.GetLookupGuid));
            }
            return result;
        }

        public void Update(Entity entity, params string[] fieldsToSubmit)
        {
            if (fieldsToSubmit != null && fieldsToSubmit.Any())
            {
                var submissionEntity = ReplicateWithFields(entity, fieldsToSubmit);
                Update(submissionEntity);
            }
        }

        public static Entity ReplicateWithFields(Entity entity, IEnumerable<string> fieldsToSubmit)
        {
            var submissionEntity = new Entity(entity.LogicalName) { Id = entity.Id };
            if (fieldsToSubmit != null)
            {
                foreach (var field in fieldsToSubmit)
                {
                    if (entity.Contains(field))
                        XrmEntity.SetField(submissionEntity, field, XrmEntity.GetField(entity, field));
                }
            }
            return submissionEntity;
        }

        #endregion

        public List<AttributeMetadata> GetEntityFieldMetadata(string entity)
        {
            lock (LockObject)
            {
                if (!EntityFieldMetadata.ContainsKey(entity))
                {
                    _controller.LogLiteral("Retrieving " + entity + " field metadata");
                    // Create the request
                    var request = new RetrieveEntityRequest
                    {
                        EntityFilters = EntityFilters.Attributes,
                        LogicalName = entity
                    };
                    var response = (RetrieveEntityResponse)Execute(request);
                    _controller.LogLiteral("Retrieved " + entity + " field metadata");
                    EntityFieldMetadata.Add(entity, new List<AttributeMetadata>(response.EntityMetadata.Attributes));
                }
            }
            return EntityFieldMetadata[entity];
        }

        public Entity GetFirst(string recordType, IEnumerable<string> fields = null)
        {
            return RetrieveFirst(BuildQuery(recordType, fields: fields));
        }

        public Entity UpdateAndRetrieve(Entity entity, params string[] fieldsToUpdate)
        {
            Update(entity, fieldsToUpdate);
            return Retrieve(entity.LogicalName, entity.Id);
        }

        public QueryExpression BuildQuery(string entityType, IEnumerable<string> fields = null,
            IEnumerable<ConditionExpression> conditions = null, IEnumerable<OrderExpression> sorts = null)
        {
            var query = new QueryExpression(entityType);
            if (conditions != null)
            {
                foreach (var condition in conditions)
                    query.Criteria.AddCondition(condition);
            }
            if (sorts != null)
            {
                foreach (var sort in sorts)
                    query.Orders.Add(sort);
            }
            if (fields != null)
                query.ColumnSet = new ColumnSet(fields.ToArray());
            else
                query.ColumnSet = new ColumnSet(true);

            return query;
        }

        public void StartWorkflow(Guid workflowId, Guid targetId)
        {
            var request = new ExecuteWorkflowRequest { EntityId = targetId, WorkflowId = workflowId };
            Execute(request);
        }

        public string GetPrimaryField(string targetType)
        {
            return GetEntityMetadata(targetType).PrimaryNameAttribute;
        }

        public bool FieldExists(string fieldName, string recordType)
        {
            return GetEntityFieldMetadata(recordType).Any(m => m.LogicalName == fieldName);
        }

        public int GetMaxLength(string fieldName, string entityType)
        {
            var metadata = GetFieldMetadata(fieldName, entityType);
            if (metadata is MemoAttributeMetadata)
            {
                var length = ((MemoAttributeMetadata)GetFieldMetadata(fieldName, entityType)).MaxLength;
                return length ?? int.MaxValue;
            }
            if (metadata is StringAttributeMetadata)
            {
                var length = ((StringAttributeMetadata)GetFieldMetadata(fieldName, entityType)).MaxLength;
                return length ?? int.MaxValue;
            }
            throw new ArgumentException("The field " + fieldName + " in entity " + entityType + " is not of string type");
        }
    }
}