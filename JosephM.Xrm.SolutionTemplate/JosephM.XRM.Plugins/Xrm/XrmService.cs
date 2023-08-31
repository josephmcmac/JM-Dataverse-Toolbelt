using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using $safeprojectname$.Core;
using $safeprojectname$.Localisation;
using Schema;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading;

namespace $safeprojectname$.Xrm
{
    public class XrmService : IOrganizationService
    {
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

        public XrmService(IOrganizationService actualService, LogController uiController)
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

        private Entity _organisationEntity;
        private Entity OrganisationEntity
        {
            get
            {
                if (_organisationEntity == null)
                {
                    _organisationEntity = GetFirst(Entities.organization);
                }
                return _organisationEntity;
            }
        }

        public Guid BaseCurrencyId
        {
            get
            {
                var value = OrganisationEntity.GetLookupGuid(Fields.organization_.basecurrencyid);
                if (!value.HasValue)
                {
                    throw new NullReferenceException($"Error getting the {GetFieldLabel(Fields.organization_.basecurrencyid, Entities.organization)} from the {GetEntityDisplayName(Entities.organization)} record");
                }
                return value.Value;
            }
        }

        private Dictionary<Guid, Entity> _currencies = new Dictionary<Guid, Entity>();
        private Entity GetCurrency(Guid currencyId)
        {
            lock (_lockObject)
            {
                if (!_currencies.ContainsKey(currencyId))
                {
                    _currencies.Add(currencyId, Retrieve(Entities.transactioncurrency, currencyId));
                }
                return _currencies[currencyId];
            }
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
                        _service = new XrmConnection(XrmConfiguration).GetOrganisationConnection();
                        UIController.LogLiteral("Dynamics Connection Created");
                    }
                }
                return _service;
            }
            set { _service = value; }
        }

        public virtual OrganizationResponse Execute(OrganizationRequest request)
        {
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
                    _service = null;
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
                    _controller.LogLiteral("Received " + ex.GetType().Name + " checking for XrmConfiguration to reconnect");
                    if (XrmConfiguration != null)
                    {
                        _controller.LogLiteral("XrmConfiguration found attempting to reconnect");
                        _service = new XrmConnection(XrmConfiguration).GetOrganisationConnection();
                        UIController.LogLiteral("Dynamics Connection Created");
                        result = Service.Execute(request);
                        _controller.LogLiteral("Reconnected");
                    }
                    else
                    {
                        _controller.LogLiteral("No Crm config found unable to reconnect..");
                        throw;
                    }
                }
            }

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
            return label.UserLocalizedLabel?.Label;
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

        public string GetFieldAsDisplayString(string recordType, string fieldName, object value, LocalisationService localisationService, bool isHtml = false, string formatString = null, Guid? currencyId = null)
        {
            if (value == null)
            {
                return string.Empty;
            }
            else if (value is string stringValue)
            {
                return isHtml
                    ? stringValue.Replace(Environment.NewLine, "<br />").Replace("\n", "<br />")
                    : stringValue;
            }
            else if (value is EntityReference entityReferenceValue)
            {
                return entityReferenceValue.Name;
            }
            else if (value is OptionSetValue optionSetValue)
            {
                return GetOptionLabel(optionSetValue.Value, fieldName, recordType);
            }
            else if (value is DateTime dateTimeValue)
            {
                var dt = (DateTime)value;
                if (dt.Kind == DateTimeKind.Utc)
                {
                    dt = localisationService.ConvertToTargetTime(dt);
                }
                if (!string.IsNullOrWhiteSpace(formatString))
                {
                    return dt.ToString(formatString);
                }
                if (GetDateFormat(fieldName, recordType) == DateTimeFormat.DateAndTime)
                {
                    return localisationService.ToDateTimeDisplayString(dt);
                }
                return localisationService.ToDateDisplayString(dt);
            }
            else if (value is int integerValue)
            {
                if (!string.IsNullOrWhiteSpace(formatString))
                {
                    return integerValue.ToString(formatString, localisationService.NumberFormatInfo);
                }
                else
                {
                    return integerValue.ToString("n0", localisationService.NumberFormatInfo);
                }
            }
            else if (value is Money moneyValue)
            {
                var moneyNumberInfo = (NumberFormatInfo)localisationService.NumberFormatInfo.Clone();
                var currencyEntity = GetCurrency(currencyId ?? BaseCurrencyId);
                moneyNumberInfo.CurrencySymbol = GetCurrency(currencyId ?? BaseCurrencyId).GetStringField(Fields.transactioncurrency_.currencysymbol);
                moneyNumberInfo.CurrencyDecimalDigits = GetCurrencyPrecision(currencyEntity.Id);
                if (!string.IsNullOrWhiteSpace(formatString))
                {
                    return moneyValue.Value.ToString(formatString, moneyNumberInfo);
                }
                else
                {
                    return moneyValue.Value.ToString("c", moneyNumberInfo);
                }
            }
            else if (value is decimal decimalValue)
            {
                if (!string.IsNullOrWhiteSpace(formatString))
                {
                    return decimalValue.ToString(formatString, localisationService.NumberFormatInfo);
                }
                else
                {
                    return decimalValue.ToString($"n{GetPrecision(fieldName, recordType)}");
                }
            }
            else if (value is double doubleValue)
            {
                if (!string.IsNullOrWhiteSpace(formatString))
                {
                    return doubleValue.ToString(formatString, localisationService.NumberFormatInfo);
                }
                else
                {
                    return doubleValue.ToString($"n{GetPrecision(fieldName, recordType)}");
                }
            }
            else if (IsActivityParty(fieldName, recordType))
            {
                if (value is IEnumerable<Entity> entityEnumerableValue)
                {
                    var namesToOutput = new List<string>();
                    foreach (var party in entityEnumerableValue)
                    {
                        namesToOutput.Add(XrmEntity.GetLookupName(party, "partyid"));
                    }
                    return string.Join(", ", namesToOutput.Where(f => !string.IsNullOrWhiteSpace(f)));
                }
            }
            else if (value is bool booleanValue)
            {
                var metadata = GetFieldMetadata(fieldName, recordType) as BooleanAttributeMetadata;
                if (metadata != null)
                {
                    if (booleanValue && metadata.OptionSet != null && metadata.OptionSet.TrueOption != null && metadata.OptionSet.TrueOption.Label != null)
                    {
                        return GetLabelDisplay(metadata.OptionSet.TrueOption.Label);
                    }
                    if (!booleanValue && metadata.OptionSet != null && metadata.OptionSet.FalseOption != null && metadata.OptionSet.FalseOption.Label != null)
                    {
                        return GetLabelDisplay(metadata.OptionSet.FalseOption.Label);
                    }
                    return value.ToString();
                }
                return booleanValue.ToString();
            }
            else if (value is byte[] byteValue)
            {
                if (isHtml)
                {
                    return $"<img src='data:image/png;base64,{Convert.ToBase64String(byteValue)}' alt='Image' />";
                }
                else
                {
                    return "File";
                }
            }
            return value.ToString();
        }

        public int GetPrecision(string field, string entity)
        {
            var fieldType = GetFieldType(field, entity);
            switch (fieldType)
            {
                case AttributeTypeCode.Decimal:
                    {
                        return ((DecimalAttributeMetadata)GetFieldMetadata(field, entity)).Precision ?? 0;
                    }
                case AttributeTypeCode.Double:
                    {
                        return ((DoubleAttributeMetadata)GetFieldMetadata(field, entity)).Precision ?? 0;
                    }
            }
            throw new NotImplementedException($"Get precision not implemented for field of type {fieldType}");
        }

        public int GetCurrencyPrecision(Guid? currencyGuid)
        {
            return GetCurrency(currencyGuid ?? BaseCurrencyId).GetInt(Fields.transactioncurrency_.currencyprecision);
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

        public IEnumerable<ExecuteMultipleResponseItem> ExecuteMultiple(IEnumerable<OrganizationRequest> requests)
        {
            var responses = new List<ExecuteMultipleResponseItem>();
            if (requests.Any())
            {
                var requestsArray = requests.ToArray();
                var requestsArrayCount = requestsArray.Count();

                var request = CreateExecuteMultipleRequest();

                var currentSetSize = 0;
                for (var i = 0; i < requestsArrayCount; i++)
                {
                    var organizationRequest = requestsArray.ElementAt(i);

                    request.Requests.Add(organizationRequest);
                    currentSetSize++;
                    if (currentSetSize == 1000 || i == requestsArrayCount - 1)
                    {
                        var response = (ExecuteMultipleResponse)Execute(request);
                        foreach (var r in response.Responses)
                            r.RequestIndex = i - currentSetSize + r.RequestIndex + 1;
                        responses.AddRange(response.Responses);
                        request = CreateExecuteMultipleRequest();
                        currentSetSize = 0;
                    }
                }
            }
            return responses;
        }

        private static ExecuteMultipleRequest CreateExecuteMultipleRequest()
        {
            var request = new ExecuteMultipleRequest()
            {
                // Assign settings that define execution behavior: continue on error, return responses. 
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = true,
                    ReturnResponses = true
                },
                // Create an empty organization request collection.
                Requests = new OrganizationRequestCollection()
            };
            return request;
        }

        public IEnumerable<ExecuteMultipleResponseItem> CreateMultiple(IEnumerable<Entity> entities)
        {
            var response = ExecuteMultiple(entities.Where(e => e != null).Select(e => new CreateRequest() { Target = e }));
            return response.ToArray();
        }

        public IEnumerable<ExecuteMultipleResponseItem> UpdateMultiple(IEnumerable<Entity> entities,
        IEnumerable<string> fields)
        {
            var responses = ExecuteMultiple(entities
                .Select(e => fields == null ? e : ReplicateWithFields(e, fields))
                .Select(e => new UpdateRequest() { Target = e }));

            return responses.ToArray();
        }

        public IEnumerable<Entity> RetrieveMultiple(string recordType, IEnumerable<Guid> ids, IEnumerable<string> fields)
        {
            var responses = ExecuteMultiple(ids
                .Select(id => new RetrieveRequest()
                {
                    Target = new EntityReference(recordType, id),
                    ColumnSet = CreateColumnSet(fields)
                }).ToArray());

            foreach (var item in responses.Cast<ExecuteMultipleResponseItem>())
            {
                if (item.Fault != null)
                    throw new FaultException<OrganizationServiceFault>(item.Fault, item.Fault.Message);

                yield return ((RetrieveResponse)item.Response).Entity;
            }
        }

        public IEnumerable<ExecuteMultipleResponseItem> DeleteMultiple(IEnumerable<Entity> entities)
        {
            var response =
                ExecuteMultiple(
                    entities.Where(e => e != null)
                        .Select(e => new DeleteRequest() { Target = new EntityReference(e.LogicalName, e.Id) }));
            return response.ToArray();
        }
    }
}