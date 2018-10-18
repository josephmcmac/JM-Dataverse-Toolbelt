using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using $safeprojectname$.Core;
using $safeprojectname$.Localisation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Schema;
using System.Text;

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
                    {
                        UIController.LogLiteral("Initialising Dynamics Connection");
                        _service = XrmConnection.GetOrgServiceProxy(XrmConfiguration);
                        UIController.LogLiteral("Dynamics Connection Created");
                    }
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

        public ColumnSet CreateColumnSet(IEnumerable<string> fields)
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

        public IEnumerable<Entity> Fetch(string fetchXmlQuery)
        {
            var query = new FetchExpression(fetchXmlQuery);
            return ((RetrieveMultipleResponse)Execute(new RetrieveMultipleRequest()
            {
                Query = query
            })).EntityCollection.Entities;
        }

        public void SetFieldIfChanging(string recordType, Guid id, string fieldName, object fieldValue)
        {
            var record = Retrieve(recordType, id, new[] { fieldName });
            var currentValue = record.GetField(fieldName);
            if (!XrmEntity.FieldsEqual(currentValue, fieldValue))
            {
                record.SetField(fieldName, fieldValue);
                Update(record);
            }
        }

        /// <summary>
        /// Returns list of key values giving the types and field name parsed for the given string of field joins
        /// key = type, value = field
        /// </summary>
        /// <param name="xrmService"></param>
        /// <param name="fieldPath"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, string>> GetTypeFieldPath(string fieldPath, string sourceType)
        {

            var list = new List<KeyValuePair<string, string>>();
            var splitOutFunction = fieldPath.Split(':');
            if (splitOutFunction.Count() > 1)
                fieldPath = splitOutFunction.ElementAt(1);
            var split = fieldPath.Split('.');
            var currentType = sourceType;
            list.Add(new KeyValuePair<string, string>(currentType, split.ElementAt(0).Split('|').First()));
            var i = 1;
            if (split.Length > 1)
            {
                foreach (var item in split.Skip(1).Take(split.Length - 1))
                {
                    var fieldName = item.Split('|').First();
                    if (split.ElementAt(i - 1).Contains("|"))
                    {
                        var targetType = split.ElementAt(i - 1).Split('|').Last();
                        list.Add(new KeyValuePair<string, string>(targetType, fieldName));
                        currentType = targetType;
                    }
                    else
                    {
                        var targetType = GetLookupTargets(list.ElementAt(i - 1).Value, currentType);
                        list.Add(new KeyValuePair<string, string>(targetType, fieldName));
                        currentType = targetType;
                    }
                    i++;
                }
            }
            return list;
        }

        /// <summary>
        /// Returns a query containing all the fields, and required joins for all the given fields
        /// field examples are "did_contactid.firstname" or "customerid|contact.lastname"
        public QueryExpression BuildSourceQuery(string sourceType, IEnumerable<string> fields)
        {
            var query = BuildQuery(sourceType, new string[0], null, null);
            foreach (var field in fields)
            {
                AddRequiredQueryJoins(query, field);
            }
            return query;
        }

        public void AddRequiredQueryJoins(QueryExpression query, string source)
        {
            var typeFieldPaths = GetTypeFieldPath(source, query.EntityName);
            var splitOutFunction = source.Split(':');
            if (splitOutFunction.Count() > 1)
                source = splitOutFunction.ElementAt(1);
            var splitTokens = source.Split('.');
            if (typeFieldPaths.Count() == 1)
                query.ColumnSet.AddColumn(typeFieldPaths.First().Value);
            else
            {
                LinkEntity thisLink = null;

                for (var i = 0; i < typeFieldPaths.Count() - 1; i++)
                {
                    var lookupField = typeFieldPaths.ElementAt(i).Value;
                    var path = string.Join(".", splitTokens.Take(i + 1)).Replace("|", "_");
                    var targetType = typeFieldPaths.ElementAt(i + 1).Key;
                    if (i == 0)
                    {
                        var matchingLinks = query.LinkEntities.Where(le => le.EntityAlias == path);

                        if (matchingLinks.Any())
                            thisLink = matchingLinks.First();
                        else
                        {
                            thisLink = query.AddLink(targetType, lookupField, GetPrimaryKey(targetType), JoinOperator.LeftOuter);
                            thisLink.EntityAlias = path;
                            thisLink.Columns = CreateColumnSet(new string[0]);
                        }
                    }
                    else
                    {
                        var matchingLinks = thisLink.LinkEntities.Where(le => le.EntityAlias == path);
                        if (matchingLinks.Any())
                            thisLink = matchingLinks.First();
                        else
                        {
                            thisLink = thisLink.AddLink(targetType, lookupField, GetPrimaryKey(targetType), JoinOperator.LeftOuter);
                            thisLink.EntityAlias = path;
                            thisLink.Columns = CreateColumnSet(new string[0]);

                        }

                    }
                }
                thisLink.Columns.AddColumn(typeFieldPaths.ElementAt(typeFieldPaths.Count() - 1).Value);
            }
        }

        public string GetLookupTargets(string field, string entity)
        {
            var result = "";
            var metadata = GetFieldMetadata(field, entity);
            if (metadata.AttributeType == AttributeTypeCode.Lookup
                || metadata.AttributeType == AttributeTypeCode.Owner
                || metadata.AttributeType == AttributeTypeCode.Customer)
            {
                var targets = ((LookupAttributeMetadata)metadata).Targets;
                result = targets.Any() ? string.Join(",", targets) : null;
            }
            return result;
        }

        public string GetPrimaryKey(string targetType)
        {
            return GetEntityMetadata(targetType).PrimaryIdAttribute;
        }

        public string GetPrimaryNameField(string targetType)
        {
            return GetEntityMetadata(targetType).PrimaryNameAttribute;
        }

        public bool IsDateIncludeTime(string fieldName, string recordType)
        {
            return ((DateTimeAttributeMetadata)GetFieldMetadata(fieldName, recordType)).Format ==
                   DateTimeFormat.DateAndTime;
        }

        public string GenerateEmailContent(string emailTemplateResourceName, string emailTemplateTargetType, Guid emailTemplateTargetId, LocalisationService localisationService, Dictionary<string, string> explicitTokenDictionary = null)
        {
            string activityDescription = null;
            var targetTokens = new List<string>();
            var staticTokens = new Dictionary<string, List<string>>();
            var ifTokens = new List<string>();
            var staticIdentifier = "static|";
            var ifIdentifier = "if|";
            var endifIdentifier = "endif";

            if (emailTemplateResourceName != null)
            {
                var resource = GetFirst(Entities.webresource, Fields.webresource_.name, emailTemplateResourceName);
                if (resource == null)
                    throw new NullReferenceException(string.Format("Could Not Find {0} With {1} '{2}'", GetEntityDisplayName(Entities.webresource), GetFieldLabel(Fields.webresource_.name, Entities.webresource), emailTemplateResourceName));
                var encoded = resource.GetStringField(Fields.webresource_.content);
                byte[] binary = Convert.FromBase64String(encoded);
                activityDescription = Encoding.UTF8.GetString(binary);

                if (explicitTokenDictionary != null)
                {
                    foreach (var item in explicitTokenDictionary)
                    {
                        activityDescription = activityDescription.Replace("[" + item.Key + "]", item.Value);
                    }
                }

                //parse out all tokens inside [] chars to replace in the email

                var i = 0;
                while (i < activityDescription.Length)
                {
                    if (activityDescription[i] == '[')
                    {
                        var startIndex = i;
                        while (i < activityDescription.Length)
                        {
                            if (activityDescription[i] == ']')
                            {
                                var endIndex = i;
                                var token = activityDescription.Substring(startIndex + 1, endIndex - startIndex - 1);

                                if (token.ToLower().StartsWith(ifIdentifier) || token.ToLower().StartsWith(endifIdentifier))
                                {
                                    ifTokens.Add(token);
                                }
                                else if (token.ToLower().StartsWith(staticIdentifier))
                                {
                                    token = token.Substring(staticIdentifier.Length);
                                    var split = token.Split('.');
                                    if (split.Count() != 2)
                                        throw new Exception(string.Format("The static token {0} is not formatted as expected. It should be of the form type.field", token));
                                    var staticType = split.First();
                                    var staticField = split.ElementAt(1);
                                    if (!staticTokens.ContainsKey(staticType))
                                        staticTokens.Add(staticType, new List<string>());
                                    staticTokens[staticType].Add(staticField);
                                }

                                else
                                    targetTokens.Add(token);
                                break;
                            }
                            i++;
                        }
                    }
                    else
                        i++;
                }
            }

            //query to get all the fields for replacing tokens
            var query = BuildSourceQuery(emailTemplateTargetType, targetTokens);
            query.Criteria.AddCondition(new ConditionExpression(GetPrimaryKey(emailTemplateTargetType), ConditionOperator.Equal, emailTemplateTargetId));
            var targetObject = RetrieveFirst(query);

            //process all the ifs (clear where not)
            while (ifTokens.Any())
            {
                var endIfTokenStackCount = 0;
                var removeAll = false;
                var token = ifTokens.First();
                if (token.ToLower() != endifIdentifier)
                {
                    var tokenIndex = activityDescription.IndexOf(token);
                    var indexOf = token.IndexOf("|");
                    if (indexOf > -1)
                    {
                        var field = token.Substring(indexOf + 1);
                        var fieldValue = targetObject.GetField(field);
                        var endIfTokenStack = 1;
                        var remainingTokens = ifTokens.Skip(1).ToList();
                        while (true && remainingTokens.Any())
                        {
                            if (remainingTokens.First().ToLower() == endifIdentifier)
                            {
                                endIfTokenStack--;
                                endIfTokenStackCount++;
                            }
                            else
                            {
                                endIfTokenStack++;
                            }
                            remainingTokens.RemoveAt(0);
                            if (endIfTokenStack == 0)
                                break;
                        }
                        //okay so starting at the current index need to find the end if
                        //and remove the content or the tokens
                        var currentStack = endIfTokenStackCount;
                        var currentIndex = activityDescription.IndexOf(token);
                        while (currentStack > 0)
                        {
                            var endIfIndex = activityDescription.IndexOf(endifIdentifier, currentIndex, StringComparison.OrdinalIgnoreCase);
                            if (endIfIndex > -1)
                            {
                                currentIndex = endIfIndex;
                                currentStack--;
                            }
                            else
                                break;
                        }
                        removeAll = fieldValue == null;
                        if (removeAll)
                        {
                            var startRemove = tokenIndex - 1;
                            var endRemove = currentIndex + endifIdentifier.Length + 1;
                            activityDescription = activityDescription.Substring(0, startRemove) + activityDescription.Substring(endRemove);
                        }
                        else
                        {
                            var startRemove = tokenIndex - 1;
                            var endRemove = currentIndex - 1;
                            activityDescription = activityDescription.Substring(0, startRemove)
                                + activityDescription.Substring(startRemove + token.Length + 2, endRemove - startRemove - token.Length - 2)
                                + activityDescription.Substring(endRemove + endifIdentifier.Length + 2);
                        }
                    }
                }
                ifTokens.RemoveRange(0, endIfTokenStackCount > 0 ? endIfTokenStackCount * 2 : 1);
            }

            //replace all the tokens
            foreach (var token in targetTokens)
            {
                var sourceType = emailTemplateTargetType;
                string displayString = GetDisplayStringForToken(targetObject, token, localisationService, isHtml: true);
                activityDescription = activityDescription.Replace("[" + token + "]", displayString);
            }

            foreach (var staticTargetTokens in staticTokens)
            {
                var staticType = staticTargetTokens.Key;
                var staticFields = staticTargetTokens.Value;

                //query to get all the fields for replacing tokens
                var staticQuery = BuildSourceQuery(staticType, staticFields);
                var staticTarget = RetrieveFirst(staticQuery);

                //replace all the tokens
                foreach (var staticField in staticFields)
                {
                    string staticFunc = null;
                    activityDescription = activityDescription.Replace("[static|" + string.Format("{0}.{1}", staticType, staticField) + "]", GetFieldAsDisplayString(staticType, staticField, staticTarget.GetField(staticField), localisationService, isHtml: true, func: staticFunc));
                }
            }
            string removeThisFunkyChar = "\xFEFF";
            if (activityDescription != null)
                activityDescription = activityDescription.Replace(removeThisFunkyChar, "");
            return activityDescription;
        }

        private string GetDisplayStringForToken(Entity targetObject, string token, LocalisationService localisationService, bool isHtml = false)
        {
            var fieldPaths = GetTypeFieldPath(token, targetObject.LogicalName);
            var thisFieldType = fieldPaths.Last().Key;
            var thisFieldName = fieldPaths.Last().Value;
            string func = null;
            var getFieldString = token.Replace("|", "_");
            var splitFunc = getFieldString.Split(':');
            if (splitFunc.Count() > 1)
            {
                func = splitFunc.First();
                getFieldString = splitFunc.ElementAt(1);
            }
            var displayString = GetFieldAsDisplayString(thisFieldType, thisFieldName, targetObject.GetField(getFieldString), localisationService, isHtml: isHtml, func: func);
            return displayString;
        }

        public string GetFieldAsDisplayString(string recordType, string fieldName, object value, LocalisationService localisationService, bool isHtml = false, string func = null)
        {
            if (value == null)
                return "";
            else if (value is string)
            {
                if (isHtml)
                    return ((string)value).Replace(Environment.NewLine, "<br />").Replace("\n", "<br />");
                else
                    return ((string)value);
            }
            else if (value is EntityReference)
            {
                return XrmEntity.GetLookupName(value);
            }
            else if (value is OptionSetValue)
            {
                if (value is OptionSetValue)
                    return GetOptionLabel(((OptionSetValue)value).Value, fieldName, recordType);
                throw new Exception("Value Type Not Matched For OptionSetValue " + value.GetType().Name);
            }
            else if (value is Money)
            {
                return XrmEntity.GetMoneyValue(value).ToString("$##,###,###,###,##0.00");
            }
            else if (value is DateTime)
            {
                var dt = (DateTime)value;
                if (dt.Kind == DateTimeKind.Utc)
                    dt = localisationService.ConvertToTargetTime(dt);
                if (func == "year")
                    return dt.ToString("yyyy");
                if (GetDateFormat(fieldName, recordType) == DateTimeFormat.DateAndTime)
                    return dt.ToString("dd/MM/yyyy hh:mm:ss tt");
                return dt.Date.ToString("dd/MM/yyyy");
            }
            else if (IsActivityParty(fieldName, recordType))
            {
                if (value is Entity[])
                {
                    var namesToOutput = new List<string>();
                    foreach (var party in (Entity[])value)
                    {
                        namesToOutput.Add(XrmEntity.GetLookupName(party, "partyid"));
                    }
                    return string.Join(", ", namesToOutput.Where(f => !string.IsNullOrWhiteSpace(f)));
                }
            }
            else if (value is bool)
            {
                var metadata = GetFieldMetadata(fieldName, recordType) as BooleanAttributeMetadata;
                if (metadata != null)
                {
                    var boolValue = (bool)value;
                    if (boolValue && metadata.OptionSet != null && metadata.OptionSet.TrueOption != null && metadata.OptionSet.TrueOption.Label != null)
                        return GetLabelDisplay(metadata.OptionSet.TrueOption.Label);
                    if (!boolValue && metadata.OptionSet != null && metadata.OptionSet.FalseOption != null && metadata.OptionSet.FalseOption.Label != null)
                        return GetLabelDisplay(metadata.OptionSet.FalseOption.Label);
                    return value.ToString();
                }
            }
            return value.ToString();
        }
        private DateTimeFormat? GetDateFormat(string fieldName, string recordType)
        {
            var metadata = (DateTimeAttributeMetadata)GetFieldMetadata(fieldName, recordType);
            return metadata.Format;
        }

        public bool IsActivityParty(string field, string recordType)
        {
            return GetFieldType(field, recordType) == AttributeTypeCode.PartyList;
        }

        public AttributeTypeCode GetFieldType(string field, string entity)
        {
            return (AttributeTypeCode)GetFieldMetadata(field, entity).AttributeType;
        }
    }
}