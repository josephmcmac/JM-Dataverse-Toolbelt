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
        private string TimeZoneId
        {
            get { return "AUS Eastern Standard Time"; }
        }

        private OrganisationSettings _organisationSettings;

        private OrganisationSettings OrganisationSettings
        {
            get
            {
                if (_organisationSettings == null)
                    _organisationSettings = new OrganisationSettings(this);
                return _organisationSettings;
            }
        }

        public static DateTime MinCrmDateTime = DateTime.SpecifyKind(new DateTime(1900, 1, 1), DateTimeKind.Utc);

        private readonly SortedDictionary<string, List<AttributeMetadata>>
            _entityFieldMetadata = new SortedDictionary<string, List<AttributeMetadata>>();

        private bool _loadedAllEntities;
        private readonly List<EntityMetadata> _entityMetadata = new List<EntityMetadata>();

        private readonly SortedDictionary<string, RelationshipMetadataBase[]> _entityRelationships
            = new SortedDictionary<string, RelationshipMetadataBase[]>();

        private readonly Object _lockObject = new Object();

        private readonly List<ManyToManyRelationshipMetadata> _relationshipMetadata = new
            List<ManyToManyRelationshipMetadata>();

        protected LogController Controller;


        /// <summary>
        ///     DONT USE CALL THE EXECUTE METHOD OR THE PROPERTY
        /// </summary>
        private IOrganizationService _service;

        internal XrmService(IOrganizationService actualService, LogController uiController)
        {
            _service = actualService;
            Controller = uiController;
            if (Controller == null)
                Controller = new LogController();
        }

        public XrmService(IXrmConfiguration crmConfig, LogController controller)
        {
            XrmConfiguration = crmConfig;
            Controller = controller;
        }

        public XrmService(IXrmConfiguration crmConfig)
        {
            XrmConfiguration = crmConfig;
            Controller = new LogController();
        }


        protected object LockObject
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

        protected LogController UIController
        {
            get { return Controller; }
            set { Controller = value; }
        }

        public IXrmConfiguration XrmConfiguration { get; set; }

        /// <summary>
        ///     DON'T USE CALL THE EXECUTE METHOD
        /// </summary>
        public IOrganizationService Service
        {
            get
            {
                lock (_lockObject)
                {
                    if (_service == null)
                        _service = new XrmConnection(XrmConfiguration).GetOrgServiceProxy();
                }
                return _service;
            }
            set { _service = value; }
        }

        public virtual OrganizationResponse Execute(OrganizationRequest request)
        {
            var requestDescription = GetRequestDescription(request);
            Controller.LogDetail("Executing crm request - " + requestDescription);

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
                    Controller.LogLiteral("Received FaultException<OrganizationServiceFault> retrying.. Details:" +
                                          ex.DisplayString());
                    Thread.Sleep(50);
                    result = Service.Execute(request);
                    Controller.LogLiteral("Successful retry");
                }
            }
            catch (CommunicationException ex)
            {
                lock (_lockObject)
                {
                    //Error was being thrown after service running overnight with no activity
                    //adding logic to reconnect when this error thrown
                    Controller.LogLiteral("Received " + ex.GetType().Name + " checking for Crm config to reconnect..");
                    if (XrmConfiguration != null)
                    {
                        Controller.LogLiteral("Crm config found attempting to reconnect..");
                        Service = new XrmConnection(XrmConfiguration).GetOrgServiceProxy();
                        result = Service.Execute(request);
                        Controller.LogLiteral("Reconnected..");
                    }
                    else
                    {
                        Controller.LogLiteral("No Crm config found unable to reconnect..");
                        throw;
                    }
                }
            }

            requestDescription = GetRequestDescription(request);
            Controller.LogDetail("Received crm response - " + requestDescription);

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
                    Controller.LogLiteral("Retrieving " + entity + " entity metadata");
                    var request = new RetrieveEntityRequest
                    {
                        EntityFilters = EntityFilters.Default,
                        LogicalName = entity
                    };
                    var response = (RetrieveEntityResponse)Execute(request);
                    Controller.LogLiteral("Retrieved " + entity + " entity metadata");
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

        public string GetRelationshipFieldName(string relationshipName, string entityType, bool isReferencing)
        {
            var rMetadata = GetRelationshipMetadata(relationshipName);
            if (entityType == rMetadata.Entity2LogicalName && entityType != rMetadata.Entity1LogicalName)
                return rMetadata.Entity2IntersectAttribute;
            if (entityType == rMetadata.Entity1LogicalName && entityType != rMetadata.Entity2LogicalName)
                return rMetadata.Entity1IntersectAttribute;
            if (isReferencing)
                return rMetadata.Entity1IntersectAttribute;
            else
                return rMetadata.Entity2IntersectAttribute;
        }

        public string GetFieldLabel(string field, string entity)
        {
            return GetLabelDisplay(GetFieldMetadata(field, entity).DisplayName);
        }

        public string GetFieldDescription(string field, string entity)
        {
            return GetLabelDisplay(GetFieldMetadata(field, entity).Description);
        }

        public string GetLabelDisplay(Label label)
        {
            return label.LocalizedLabels.Any(l => l.LanguageCode == 1033)
                ? label.LocalizedLabels.First(l => l.LanguageCode == 1033).Label
                : string.Empty;
        }

        public AttributeTypeCode GetFieldType(string field, string entity)
        {
            var fieldMetadata = GetFieldMetadata(field, entity);
            if (fieldMetadata.AttributeType == null)
                throw new NullReferenceException("Error AttributeType Is Null");
            return (AttributeTypeCode)fieldMetadata.AttributeType;
        }

        protected IEnumerable<KeyValuePair<int, string>> OptionSetToKeyValues(IEnumerable<OptionMetadata> options)
        {
            var result = new List<KeyValuePair<int, string>>();
            foreach (var item in options)
            {
                var option = item.Value;
                if (option != null)
                    result.Add(new KeyValuePair<int, string>(option.Value, GetOptionLabel(item)));
            }
            return result;
        }

        public IEnumerable<KeyValuePair<int, string>> GetPicklistKeyValues(string entityType, string fieldName)
        {
            var fieldType = GetFieldType(fieldName, entityType);
            switch (fieldType)
            {
                case AttributeTypeCode.Picklist:
                    {
                        var metadata = (PicklistAttributeMetadata)GetFieldMetadata(fieldName, entityType);
                        return OptionSetToKeyValues(metadata.OptionSet.Options);
                    }
                case AttributeTypeCode.Status:
                    {
                        var metadata = (StatusAttributeMetadata)GetFieldMetadata(fieldName, entityType);
                        return
                            OptionSetToKeyValues(
                                metadata.OptionSet.Options);
                    }
            }
            throw new ArgumentException("Field type not implemented: " + fieldType);
        }

        public bool IsMandatory(string field, string entity)
        {
            var result = false;
            var fieldMetadata = GetFieldMetadata(field, entity);
            var level = fieldMetadata.RequiredLevel;
            if (level.Value == AttributeRequiredLevel.ApplicationRequired ||
                level.Value == AttributeRequiredLevel.SystemRequired)
                result = true;
            return result;
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

        public decimal? GetMaxDecimalValue(string field, string entity)
        {
            return ((DecimalAttributeMetadata)GetFieldMetadata(field, entity)).MaxValue;
        }

        public decimal? GetMinDecimalValue(string field, string entity)
        {
            return ((DecimalAttributeMetadata)GetFieldMetadata(field, entity)).MinValue;
        }

        public double? GetMaxMoneyValue(string field, string entity)
        {
            return ((MoneyAttributeMetadata)GetFieldMetadata(field, entity)).MaxValue;
        }

        public double? GetMinMoneyValue(string field, string entity)
        {
            return ((MoneyAttributeMetadata)GetFieldMetadata(field, entity)).MinValue;
        }

        public int GetMaxIntValue(string fieldName, string entityType)
        {
            var max = ((IntegerAttributeMetadata)GetFieldMetadata(fieldName, entityType)).MaxValue;
            if (!max.HasValue)
                throw new NullReferenceException("MaxValue Is Null");
            return max.Value;
        }

        public int GetMinIntValue(string fieldName, string entityType)
        {
            var min = ((IntegerAttributeMetadata)GetFieldMetadata(fieldName, entityType)).MinValue;
            if (!min.HasValue)
                throw new NullReferenceException("MinValue Is Null");
            return min.Value;
        }

        public double? GetMaxDoubleValue(string field, string entity)
        {
            return ((DoubleAttributeMetadata)GetFieldMetadata(field, entity)).MaxValue;
        }

        public double? GetMinDoubleValue(string field, string entity)
        {
            return ((DoubleAttributeMetadata)GetFieldMetadata(field, entity)).MinValue;
        }

        public bool IsString(string fieldName, string entityType)
        {
            var fieldType = GetFieldType(fieldName, entityType);
            return fieldType == AttributeTypeCode.String || fieldType == AttributeTypeCode.Memo;
        }

        public bool IsLookup(string field, string entity)
        {
            var result = false;
            var type = GetFieldType(field, entity);
            if (new[] { AttributeTypeCode.Customer, AttributeTypeCode.Lookup, AttributeTypeCode.Owner }.Contains(type))
                result = true;
            return result;
        }

        public int GetMatchingOptionValue(string value, string field, string entity)
        {
            var metadata = GetFieldMetadata(field, entity);
            foreach (var option in ((EnumAttributeMetadata)metadata).OptionSet.Options)
            {
                if (GetOptionLabel(option).ToLower() == value.ToLower() || option.Value.ToString() == value)
                {
                    if (option.Value != null)
                    {
                        return (int)option.Value;
                    }
                }
            }
            if (!String.IsNullOrEmpty(value))
                throw new ArgumentOutOfRangeException("Field " + field + " in entity " + entity +
                                                      " has no matching picklist option for label " + value);
            return -1;
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

        public SortedDictionary<int, string> GetOptions(string field, string entity)
        {
            var result = new SortedDictionary<int, string>();

            var metadata = (EnumAttributeMetadata)GetFieldMetadata(field, entity);
            foreach (var option in metadata.OptionSet.Options)
            {
                if (option.Value != null)
                    result.Add((int)option.Value, GetOptionLabel(option));
            }
            return result;
        }

        public string GetLookupTargetEntity(string field, string entity)
        {
            var result = "";
            var metadata = GetFieldMetadata(field, entity);
            if (metadata.AttributeType == AttributeTypeCode.Lookup)
            {
                result = ((LookupAttributeMetadata)metadata).Targets[0];
            }
            else if (metadata.AttributeType == AttributeTypeCode.Owner)
            {
                result = "systemuser";
            }
            return result;
        }

        private static string GetOptionLabel(OptionMetadata option)
        {
            return option.Label.LocalizedLabels[0].Label;
        }

        public string GetRelationshipEntityName(string relationshipName)
        {
            //This is in case any of the relationships had their entity name cut off
            if (relationshipName == "systemuserroles_association")
                return "systemuserroles";
            if (relationshipName == "product_entitlementtemplate_association")
                return "entitlementtemplateproducts";
            else if (relationshipName.Length > 47)
                return relationshipName.Substring(0, 47);
            else
                return relationshipName;
        }

        public string GetPrimaryKeyName(string entityTo)
        {
            return XrmEntity.GetPrimaryKeyName(entityTo);
        }

        public string GetFilteredViewName(string entity)
        {
            return GetEntityMetadata(entity).ReportViewName;
        }

        public bool IsRealNumber(string fieldName, string entityType)
        {
            var fieldType = GetFieldType(fieldName, entityType);
            return fieldType == AttributeTypeCode.Double || fieldType == AttributeTypeCode.Decimal;
        }

        public int GetDecimalPrecision(string field, string entity)
        {
            int? temp = null;
            var internalType = GetFieldType(field, entity);
            switch (internalType)
            {
                case AttributeTypeCode.Decimal:
                    {
                        temp = ((DecimalAttributeMetadata)GetFieldMetadata(field, entity)).Precision;
                        break;
                    }
                case AttributeTypeCode.Double:
                    {
                        temp = ((DoubleAttributeMetadata)GetFieldMetadata(field, entity)).Precision;
                        break;
                    }
            }
            if (!temp.HasValue)
                throw new NullReferenceException("Precision Is Null");
            return temp.Value;
        }

        public string GetPrimaryNameField(string targetType)
        {
            return GetEntityMetadata(targetType).PrimaryNameAttribute;
        }

        public string GetPrimaryKeyField(string targetType)
        {
            return GetEntityMetadata(targetType).PrimaryIdAttribute;
        }

        public IsValidResponse VerifyConnection()
        {
            var response = new IsValidResponse();
            try
            {
                Execute(new WhoAmIRequest());
            }
            catch (Exception ex)
            {
                response.AddInvalidReason(ex.DisplayString());
            }
            return response;
        }

        public bool IsMultilineText(string fieldName, string recordType)
        {
            var metadata = GetFieldMetadata(fieldName, recordType);
            return metadata.AttributeType == AttributeTypeCode.String &&
                   ((StringAttributeMetadata)metadata).Format == StringFormat.TextArea;
        }

        public string GetEntityDisplayName(string recordType)
        {
            return GetLabelDisplay(GetEntityMetadata(recordType).DisplayName);
        }

        public string GetEntityCollectionName(string recordType)
        {
            return GetLabelDisplay(GetEntityMetadata(recordType).DisplayCollectionName);
        }

        public string GetEntityLabel(string entity)
        {
            return GetLabelDisplay(GetEntityMetadata(entity).DisplayName);
        }

        public Guid WhoAmI()
        {
            return ((WhoAmIResponse)Execute(new WhoAmIRequest())).UserId;
        }

        public bool IsEntityAuditOn(string entityName)
        {
            return GetEntityMetadata(entityName).IsAuditEnabled.Value;
        }

        public bool IsFieldAuditOn(string fieldName, string recordType)
        {
            return GetFieldMetadata(fieldName, recordType).IsAuditEnabled.Value;
        }

        public bool IsFieldSearchable(string fieldName, string recordType)
        {
            return GetFieldMetadata(fieldName, recordType).IsValidForAdvancedFind.Value;
        }

        public bool IsDateIncludeTime(string fieldName, string recordType)
        {
            return ((DateTimeAttributeMetadata)GetFieldMetadata(fieldName, recordType)).Format ==
                   DateTimeFormat.DateAndTime;
        }

        public StringFormat? GetTextFormat(string fieldName, string recordType)
        {
            return ((StringAttributeMetadata)GetFieldMetadata(fieldName, recordType)).Format;
        }

        public IntegerFormat GetIntegerFormat(string fieldName, string recordType)
        {
            var field = ((IntegerAttributeMetadata)GetFieldMetadata(fieldName, recordType));
            if (!field.Format.HasValue)
                throw new NullReferenceException("Format Is Null");
            return field.Format.Value;
        }

        public IEnumerable<Entity> RetrieveAllActive(string entityType, string[] fields,
            IEnumerable<ConditionExpression> filters, string[] sortFields)
        {
            return RetrieveAll(BuildQueryActive(entityType, fields, filters, sortFields));
        }

        public static QueryExpression BuildQueryActive(string entityType, IEnumerable<string> fields,
            IEnumerable<ConditionExpression> filters, string[] sortFields)
        {
            var query = new QueryExpression(entityType);
            var status = new List<object>();
            status.Add(XrmPicklists.State.Active);
            switch (entityType)
            {
                case "quote":
                    status = new List<object>() { XrmPicklists.QuoteState.Active, XrmPicklists.QuoteState.Draft };
                    break;
                case "salesorder":
                    status = new List<object>() { XrmPicklists.OrderStateState.Active, XrmPicklists.OrderStateState.Invoiced, XrmPicklists.OrderStateState.Submitted };
                    break;
            }
            query.Criteria.AddCondition("statecode", ConditionOperator.In, status.ToArray());
            if (filters != null)
            {
                foreach (var condition in filters)
                    query.Criteria.AddCondition(condition);
            }
            if (sortFields != null)
            {
                foreach (var sortField in sortFields)
                    query.AddOrder(sortField, OrderType.Ascending);
            }
            if (fields != null)
                query.ColumnSet = new ColumnSet(fields.ToArray());
            else
                query.ColumnSet = new ColumnSet(true);

            return query;
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
                AllRelationshipMetadata.Clear();
                SharedOptionSets.Clear();
                _loadedAllEntities = false;
            }
        }

        public object ParseField(string fieldName, string entityType, object value, bool datesAmericanFormat = false)
        {
            var fieldType = GetFieldType(fieldName, entityType);
            if (!value.IsEmpty())
            {
                switch (fieldType)
                {
                    case AttributeTypeCode.String:
                        {
                            var maxLength = GetMaxLength(fieldName, entityType);
                            var temp = value.ToString();
                            if (temp.Length > maxLength)
                                throw new ArgumentOutOfRangeException("Field " + fieldName +
                                                                      " exceeded maximum length of " + maxLength);
                            return temp;
                        }
                    case AttributeTypeCode.Memo:
                        {
                            var maxLength = GetMaxLength(fieldName, entityType);
                            var temp = value.ToString();
                            if (temp.Length > maxLength)
                                throw new ArgumentOutOfRangeException("Field " + fieldName +
                                                                      " exceeded maximum length of " + maxLength);
                            return temp;
                        }
                    case AttributeTypeCode.Integer:
                        {
                            int temp;
                            if (value is int)
                                temp = (int)value;
                            else if (value is string && value.ToString().IsNullOrWhiteSpace())
                                return null;
                            else
                                temp = int.Parse(value.ToString().Replace(",", ""));
                            if (!IntInRange(fieldName, entityType, temp))
                                throw new ArgumentOutOfRangeException("Field " + fieldName +
                                                                      " outside permitted range of " +
                                                                      ((IntegerAttributeMetadata)
                                                                          GetFieldMetadata(fieldName, entityType)).MinValue +
                                                                      " to " +
                                                                      ((IntegerAttributeMetadata)
                                                                          GetFieldMetadata(fieldName, entityType)).MaxValue);
                            return temp;
                        }
                    case AttributeTypeCode.DateTime:
                        {
                            DateTime? temp = null;
                            if (value is DateTime)
                                temp = (DateTime)value;
                            else
                            {
                                if (!String.IsNullOrWhiteSpace(value.ToString()))
                                    try
                                    {
                                        temp = DateTime.Parse(value.ToString(), new CultureInfo(datesAmericanFormat ? "en-US" : "en-GB"));
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new ArgumentException(
                                            "Error Parsing Field " + fieldName + " in entity " + entityType + " value " +
                                            value, ex);
                                    }
                            }
                            if (temp != null && temp < MinCrmDateTime)
                                throw new ArgumentOutOfRangeException("Field " + fieldName + " in entity " + entityType +
                                                                      " below lowest permitted value of " +
                                                                      MinCrmDateTime);
                            //remove the second fractions as crm strips them out
                            if (temp.HasValue)
                                temp = temp.Value.AddMilliseconds(-1 * temp.Value.Millisecond).ToUniversalTime();
                            return temp;
                        }
                    case AttributeTypeCode.Lookup:
                    case AttributeTypeCode.Customer:
                    case AttributeTypeCode.Owner:
                        {
                            if (value is EntityReference)
                                return value;
                            if (value is Guid && fieldType == AttributeTypeCode.Lookup)
                                return new EntityReference(GetLookupTargetEntity(fieldName, entityType), (Guid)value);
                            else if (value is string)
                            {
                                var types = new List<string>();
                                if (fieldType == AttributeTypeCode.Lookup)
                                    types.Add(GetLookupTargetEntity(fieldName, entityType));
                                else if (fieldType == AttributeTypeCode.Owner)
                                    types.AddRange(new[] { "team", "systemuser" });
                                else if (fieldType == AttributeTypeCode.Customer)
                                    types.AddRange(new[] { "account", "contact" });

                                var matchingRecords = new List<EntityReference>();
                                foreach (var type in types)
                                {
                                    Guid tryGetGuid = Guid.Empty;
                                    if (Guid.TryParse(value.ToString(), out tryGetGuid))
                                    {
                                        if (types.Count() == 1)
                                            matchingRecords.Add(CreateLookup(type, tryGetGuid));
                                        else
                                        {
                                            var match = GetFirst(type, GetPrimaryKeyField(type), tryGetGuid);
                                            if (match != null)
                                                matchingRecords.Add(match.ToEntityReference());
                                        }
                                    }
                                    else
                                    {
                                        var conditions = new List<ConditionExpression>(new[]
                                        {
                                            new ConditionExpression(GetPrimaryNameField(type), ConditionOperator.Equal,
                                                value.ToString())
                                        });
                                        if (type == "account" || type == "contact")
                                            conditions.Add(new ConditionExpression("merged", ConditionOperator.NotEqual, true));
                                        matchingRecords.AddRange(RetrieveAllAndClauses(type, conditions
                                            , new string[0]).Select(e => e.ToEntityReference()).ToArray());

                                    }
                                }
                                if (matchingRecords.Count() == 1)
                                    return matchingRecords.First();
                                throw new Exception(
                                    string.Format(
                                        "Error Parsing Field {0}. The String Value Was Not A Guid And Did Not Match To A Unique {1} Records Name. The value was {2}",
                                        fieldName, string.Join(",", types), value));
                            }
                            else
                                throw new ArgumentException(
                                    string.Format("Parse {0} not implemented for argument type of {1} ", fieldType,
                                        value.GetType().Name));
                        }
                    case AttributeTypeCode.Picklist:
                        {
                            if (value is OptionSetValue)
                            {
                                return value;
                            }
                            if (value is string)
                            {
                                return
                                    CreateOptionSetValue(GetMatchingOptionValue((string)value, fieldName, entityType));
                            }
                            else if (value is int)
                            {
                                return CreateOptionSetValue((int)value);
                            }
                            else
                                throw new ArgumentException("Parse picklist not implemented for argument type: " +
                                                            value.GetType().Name);
                        }
                    case AttributeTypeCode.Decimal:
                        {
                            decimal newValue = 0;
                            if (value is decimal)
                                newValue = (decimal)value;
                            else
                                newValue = decimal.Parse(value.ToString().Replace(",", ""));
                            newValue = decimal.Round(newValue, GetDecimalPrecision(fieldName, entityType));
                            if (!DecimalInRange(fieldName, entityType, newValue))
                                throw new ArgumentOutOfRangeException("Field " + fieldName +
                                                                      " outside permitted range of " +
                                                                      ((DecimalAttributeMetadata)
                                                                          GetFieldMetadata(fieldName, entityType)).MinValue +
                                                                      " to " +
                                                                      ((DecimalAttributeMetadata)
                                                                          GetFieldMetadata(fieldName, entityType)).MaxValue);
                            return newValue;
                        }
                    case AttributeTypeCode.Double:
                        {
                            double temp;
                            if (value is double)
                                temp = (double)value;
                            else
                                temp = double.Parse(value.ToString().Replace(",", ""));
                            if (!DoubleInRange(fieldName, entityType, temp))
                                throw new ArgumentOutOfRangeException("Field " + fieldName +
                                                                      " outside permitted range of " +
                                                                      ((DoubleAttributeMetadata)
                                                                          GetFieldMetadata(fieldName, entityType)).MinValue +
                                                                      " to " +
                                                                      ((DoubleAttributeMetadata)
                                                                          GetFieldMetadata(fieldName, entityType)).MaxValue);
                            return temp;
                        }
                    case AttributeTypeCode.Money:
                        {
                            Money temp;
                            if (value is Money)
                                temp = (Money)value;
                            else if (value is Decimal)
                                temp = new Money((decimal)value);
                            else
                                temp = new Money(decimal.Parse(value.ToString().Replace(",", "")));
                            if (!MoneyInRange(fieldName, entityType, temp))
                                throw new ArgumentOutOfRangeException("Field " + fieldName +
                                                                      " outside permitted range of " +
                                                                      ((MoneyAttributeMetadata)
                                                                          GetFieldMetadata(fieldName, entityType)).MinValue +
                                                                      " to " +
                                                                      ((MoneyAttributeMetadata)
                                                                          GetFieldMetadata(fieldName, entityType)).MaxValue);
                            return temp;
                        }
                    case AttributeTypeCode.Boolean:
                        {
                            if (value is bool)
                                return value;
                            else if (value is string)
                                return new[] { "true", "yes", "1", "of course" }.Contains(((string)value).ToLower());
                            else
                                throw new ArgumentException("Parse bool not implemented for argument type: " +
                                                            value.GetType().Name);
                        }
                    case AttributeTypeCode.Status:
                        {
                            if (value is OptionSetValue)
                                return value;
                            else if (value is int)
                                return CreateOptionSetValue((int)value);
                            else if (value is string)
                                return
                                    CreateOptionSetValue(GetMatchingOptionValue((string)value, fieldName, entityType));
                            else
                                throw new ArgumentException("Parse status not implemented for argument type: " +
                                                            value.GetType().Name);
                        }
                    case AttributeTypeCode.Uniqueidentifier:
                        {
                            if (value is Guid)
                                return value;
                            if (value is string)
                                return new Guid((string)value);
                            else
                                throw new ArgumentException(
                                    "Parse UniqueIdentifier not implemented for argument type: " + value.GetType().Name);
                        }
                    case AttributeTypeCode.State:
                        {
                            if (value is OptionSetValue)
                                return value;
                            else if (value is int)
                                return CreateOptionSetValue((int)value);
                            else if (value is string)
                                return
                                    CreateOptionSetValue(GetMatchingOptionValue((string)value, fieldName, entityType));
                            else
                                throw new ArgumentException("Parse state not implemented for argument type: " +
                                                            value.GetType().Name);
                        }
                    case AttributeTypeCode.PartyList:
                        {
                            if (value is IEnumerable<EntityReference>)
                            {
                                return ((IEnumerable<EntityReference>)value).Select(XrmEntity.CreatePartyEntity).ToArray();
                            }
                            if (value is IEnumerable<Entity> || value is EntityCollection)
                            {
                                return value;
                            }
                            else
                                throw new ArgumentException("Parse partylist not implemented for argument type: " +
                                                            value.GetType().Name);
                        }
                }
                return value;
            }
            else
                return null;
        }

        public object ConvertToQueryValue(string fieldName, string entityType, object value)
        {
            var parsedValue = ParseField(fieldName, entityType, value);
            if (parsedValue is EntityReference)
                parsedValue = ((EntityReference)parsedValue).Id;
            else if (parsedValue is OptionSetValue)
                parsedValue = ((OptionSetValue)parsedValue).Value;
            else if (parsedValue is Money)
                parsedValue = ((Money)parsedValue).Value;
            return parsedValue;
        }

        public bool DecimalInRange(string field, string entity, decimal value)
        {
            return
                value >= GetMinDecimalValue(field, entity)
                && value <= GetMaxDecimalValue(field, entity);
        }

        public bool MoneyInRange(string field, string entity, Money value)
        {
            if (value != null)
            {
                var amount = XrmEntity.GetMoneyValue(value);
                return
                    (double)amount >= GetMinMoneyValue(field, entity)
                    && (double)amount <= GetMaxMoneyValue(field, entity);
            }
            return true;
        }

        public bool IntInRange(string field, string entity, int value)
        {
            return
                value >= GetMinIntValue(field, entity)
                && value <= GetMaxIntValue(field, entity);
        }

        public bool DoubleInRange(string field, string entity, double value)
        {
            return
                value >= GetMinDoubleValue(field, entity)
                && value <= GetMaxDoubleValue(field, entity);
        }

        private object CreateOptionSetValue(int value)
        {
            return new OptionSetValue(value);
        }

        public EntityReference CreateLookup(Entity target)
        {
            return new EntityReference(target.LogicalName, target.Id);
        }

        public EntityReference CreateLookup(string targetType, Guid id)
        {
            return new EntityReference(targetType, id);
        }

        public Entity CreateAndRetreive(Entity entity)
        {
            return CreateAndRetreive(entity, null);
        }

        public Entity CreateAndRetreive(Entity entity, IEnumerable<string> fields)
        {
            var id = Create(entity);
            return Retrieve(entity.LogicalName, id, CreateColumnSet(fields));
        }

        public Entity Retrieve(string entityType, Guid id, IEnumerable<string> fields)
        {
            return Retrieve(entityType, id, CreateColumnSet(fields));
        }

        public Entity Retrieve(string entityType, Guid id)
        {
            return Retrieve(entityType, id, CreateColumnSet(null));
        }

        public ColumnSet CreateColumnSet(IEnumerable<string> fields)
        {
            if (fields != null)
                return new ColumnSet(fields.ToArray());
            else
                return new ColumnSet(true);
        }

        public SortedDictionary<string, Guid> IndexFirstGuidByFieldValue(string field, string entity)
        {
            // Retrieve the related opportunity products
            var query = new QueryExpression
            {
                EntityName = entity,
                ColumnSet = new ColumnSet(field),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = field,
                            Operator = ConditionOperator.NotNull
                        }
                    }
                }
            };

            var entities = RetrieveAll(query);

            var thisFieldIndexed = new SortedDictionary<string, Guid>();
            //Query every instance of the entity with the field not null
            foreach (var record in entities)
            {
                var fieldValue = GetFieldAsMatchString(entity, field, XrmEntity.GetField(record, field));
                if (!thisFieldIndexed.ContainsKey(fieldValue))
                {
                    thisFieldIndexed.Add(fieldValue, record.Id);
                }
            }
            return thisFieldIndexed;
        }

        /// <summary>
        ///     Warning! Unlimited results
        /// </summary>
        public SortedDictionary<string, Guid> IndexGuidsByValue(string entityType, string indexByField)
        {
            var query = new QueryExpression
            {
                EntityName = entityType,
                ColumnSet = new ColumnSet(new[] { indexByField }),
            };
            return IndexIdsByField(indexByField, RetrieveAll(query));
        }

        public SortedDictionary<string, Guid> IndexIdsByField(string indexByField, IEnumerable<Entity> items)
        {
            var result = new SortedDictionary<string, Guid>();
            foreach (var record in items)
            {
                var fieldValue = GetFieldAsMatchString(record.LogicalName, indexByField,
                    XrmEntity.GetField(record, indexByField));
                if (!String.IsNullOrWhiteSpace(fieldValue) && !result.ContainsKey(fieldValue))
                    result.Add(fieldValue, record.Id);
            }
            return result;
        }

        /// <summary>
        ///     Warning! Unlimited results
        /// </summary>
        public SortedDictionary<string, Entity> IndexEntitiesByValue(string entityType, string indexByField,
            string[] requiredFields)
        {
            var query = CreateQuery(entityType,
                requiredFields == null ? null : requiredFields.Union(new[] { indexByField }).ToArray());


            return IndexEntitiesByField(indexByField, RetrieveAll(query));
        }

        public SortedDictionary<string, Entity> IndexEntitiesByField(string indexByField, IEnumerable<Entity> items)
        {
            var result = new SortedDictionary<string, Entity>();
            foreach (var record in items)
            {
                var fieldObject = XrmEntity.GetField(record, indexByField);
                var fieldString = fieldObject == null ? null : fieldObject.ToString();
                if (!String.IsNullOrWhiteSpace(fieldString) && !result.ContainsKey(fieldString))
                    result.Add(fieldString, record);
            }
            return result;
        }

        public Entity GetFirst(string entityType, string fieldName, object fieldValue)
        {
            return GetFirst(entityType, fieldName, fieldValue, null);
        }

        public Entity GetFirst(string entityType, string fieldName, object fieldValue, IEnumerable<string> fields)
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

        public IEnumerable<Entity> GetAssociatedEntities(string entityTo, string relationshipName,
            string entityFromRelationShipId, Guid entityFromId,
            string toRelationShipId, IEnumerable<string> fields)
        {
            var query = new QueryExpression(entityTo);
            query.ColumnSet = CreateColumnSet(fields);
            var link = query.AddLink(GetRelationshipEntityName(relationshipName), GetPrimaryKeyName(entityTo),
                toRelationShipId);
            link.LinkCriteria.AddCondition(entityFromRelationShipId, ConditionOperator.Equal, entityFromId);
            return RetrieveAll(query);
        }

        public IEnumerable<Guid> GetAssociatedIds(string thisEntityType, Guid thisId, string relationshipName,
            string otherSideId)
        {
            var relationshipMetadata = GetRelationshipMetadata(relationshipName);
            var isReferencing = relationshipMetadata.Entity2IntersectAttribute == otherSideId;
            var thisSideId = isReferencing
                ? relationshipMetadata.Entity1IntersectAttribute
                : relationshipMetadata.Entity2IntersectAttribute;
            var query = new QueryExpression(GetRelationshipEntityName(relationshipMetadata.IntersectEntityName));
            query.ColumnSet = CreateColumnSet(new[] { otherSideId });
            query.Criteria.AddCondition(thisSideId, ConditionOperator.Equal, thisId);
            return RetrieveAll(query).Select(entity => (Guid)XrmEntity.GetField(entity, otherSideId)).ToArray();
        }

        public IEnumerable<Guid> GetAssociatedIds(string relationshipName, string thisSideId, Guid thisId,
            string otherSideId)
        {
            var query = new QueryExpression(GetRelationshipEntityName(relationshipName));
            query.ColumnSet = CreateColumnSet(new[] { otherSideId });
            query.Criteria.AddCondition(thisSideId, ConditionOperator.Equal, thisId);
            return RetrieveAll(query).Select(entity => (Guid)XrmEntity.GetField(entity, otherSideId));
        }

        /// <summary>
        ///     Warning! Unlimited results
        /// </summary>
        public SortedDictionary<Guid, IEnumerable<Guid>> IndexAssociatedIds(string thisEntityType,
            string relationshipName, string otherSideId)
        {
            var result = new SortedDictionary<Guid, IEnumerable<Guid>>();

            var relationshipMetadata = GetRelationshipMetadata(relationshipName);
            var isReferencing = relationshipMetadata.Entity2IntersectAttribute == otherSideId;
            var thisSideId = isReferencing
                ? relationshipMetadata.Entity1IntersectAttribute
                : relationshipMetadata.Entity2IntersectAttribute;
            var query = new QueryExpression(GetRelationshipEntityName(relationshipName));
            query.ColumnSet = CreateColumnSet(new[] { thisSideId, otherSideId });
            var allThiQuery = new QueryExpression(thisEntityType);
            allThiQuery.ColumnSet = CreateColumnSet(new string[] { });
            var allThisItems = RetrieveAll(new QueryExpression(thisEntityType));
            foreach (var item in allThisItems)
                result.Add(item.Id, new List<Guid>());

            var allAssociations = RetrieveAll(query);
            foreach (var item in allAssociations)
            {
                var thisId = (Guid)XrmEntity.GetField(item, thisSideId);
                if (result.ContainsKey(thisId))
                    ((List<Guid>)result[thisId]).Add((Guid)XrmEntity.GetField(item, otherSideId));
            }
            return result;
        }

        public string GetFieldAsMatchString(string entityType, string fieldName, object fieldValue)
        {
            if (fieldValue == null)
                return "";
            else if (IsRealNumber(fieldName, entityType))
            {
                var format = "#." + (new string('#', GetDecimalPrecision(fieldName, entityType)));
                return (decimal.Parse(fieldValue.ToString())).ToString(format);
            }
            else if (IsLookup(fieldName, entityType))
            {
                return ((EntityReference)fieldValue).Id.ToString();
            }
            else
                return fieldValue.ToString();
        }

        public void SetState(string entityType, Guid id, int state, int status)
        {
            var setStateReq = new SetStateRequest
            {
                EntityMoniker = new EntityReference(entityType, id),
                State = new OptionSetValue(state),
                Status = new OptionSetValue(status)
            };

            Execute(setStateReq);
        }

        public void SetState(string entityType, Guid id, int state)
        {
            var setStateReq = new SetStateRequest
            {
                EntityMoniker = new EntityReference(entityType, id),
                State = new OptionSetValue(state),
                Status = new OptionSetValue(-1)
            };

            Execute(setStateReq);
        }

        public void SetState(Entity entity, int state)
        {
            SetState(entity.LogicalName, entity.Id, state);
        }

        public IEnumerable<Entity> RetrieveAllEntityType(string entityType)
        {
            return RetrieveAll(new QueryExpression(entityType) { ColumnSet = CreateColumnSet(null) });
        }

        public IEnumerable<Entity> RetrieveAllEntityType(string entityType, IEnumerable<string> fields)
        {
            return RetrieveAll(new QueryExpression(entityType) { ColumnSet = CreateColumnSet(fields) });
        }

        public IEnumerable<Entity> GetUserRoles(Guid userId, string[] columns)
        {
            var query = new QueryExpression("role");
            query.ColumnSet = new ColumnSet(columns);
            var userRole = query.AddLink("systemuserroles", "roleid", "roleid");
            userRole.LinkCriteria.AddCondition("systemuserid", ConditionOperator.Equal, userId);
            return RetrieveAll(query);
        }

        public IEnumerable<Entity> GetLinkedRecords(string linkToEntity, string linkFromEntity,
            string linkToEntityLookup, Guid linkFromrecordId, string[] fields,
            IEnumerable<ConditionExpression> filters)
        {
            var query = new QueryExpression(linkToEntity);
            query.Criteria.AddCondition(linkToEntityLookup, ConditionOperator.Equal, linkFromrecordId);
            AddAndConditions(query, filters);
            query.ColumnSet = CreateColumnSet(fields);
            return RetrieveAll(query);
        }

        public IEnumerable<Entity> GetLinkedRecords(string entity, string entityId, Guid entityGuid, string linkedType,
            string lookup)
        {
            var query = new QueryExpression(linkedType);
            var link = query.AddLink(entity, lookup, entityId);
            link.LinkCriteria.AddCondition(new ConditionExpression(entityId, ConditionOperator.Equal, entityGuid));
            query.ColumnSet = CreateColumnSet(null);
            return RetrieveAll(query);
        }

        public IEnumerable<Entity> GetAssociatedEntities(string entity, string entityId, string intersectId1,
            Guid intersectGuid,
            string relationshipName, string relatedEntity, string relatedEntityId, string intersectId2)
        {
            var query = new QueryExpression(relatedEntity);
            var link = query.AddLink(relationshipName, intersectId2, intersectId2);
            var link2 = link.AddLink(entity, intersectId1, entityId);
            link2.LinkCriteria.AddCondition(new ConditionExpression(entityId, ConditionOperator.Equal, intersectGuid));
            query.ColumnSet = CreateColumnSet(null);
            return RetrieveAll(query);
        }

        private static void AddAndConditions(QueryExpression query, IEnumerable<ConditionExpression> filters)
        {
            if (filters != null)
            {
                foreach (var condition in filters)
                    query.Criteria.AddCondition(condition);
            }
        }

        private static void AddOrderExpressions(QueryExpression query, IEnumerable<OrderExpression> orders)
        {
            if (orders != null)
            {
                foreach (var order in orders)
                    query.AddOrder(order.AttributeName, order.OrderType);
            }
        }

        public IEnumerable<Entity> GetLinkedRecords(string linkToEntity, string linkFromEntity,
            string linkToEntityLookup, Guid linkFromrecordId)
        {
            return GetLinkedRecords(linkToEntity, linkFromEntity,
                linkToEntityLookup, linkFromrecordId, null, null);
        }

        public void SetField(string entityType, Guid guid, string fieldName, object value)
        {
            var entity = new Entity(entityType) { Id = guid };
            XrmEntity.SetField(entity, fieldName, value);
            Update(entity);
        }

        public Entity GetLinkedRecord(string linkedRecordType, string linkFromRecordType, string linkFromLookup,
            Guid linkFromId, string[] fields)
        {
            var query = new QueryExpression(linkedRecordType);
            query.ColumnSet = CreateColumnSet(fields);
            var linkFrom = query.AddLink(linkFromRecordType,
                XrmEntity.GetPrimaryKeyName(linkedRecordType), linkFromLookup);
            linkFrom.LinkCriteria.AddCondition(new ConditionExpression(XrmEntity.GetPrimaryKeyName(linkFromRecordType),
                ConditionOperator.Equal, linkFromId));
            return RetrieveFirst(query);
        }

        public Entity GetLinkedRecord(string linkedRecordType, string linkThroughRecordType, string linkFromRecordType,
            string linkThroughLookup, string linkFromLookup, Guid linkFromId, string[] fields)
        {
            var query = new QueryExpression(linkedRecordType);
            query.ColumnSet = CreateColumnSet(fields);
            var linkThrough = query.AddLink(linkThroughRecordType, XrmEntity.GetPrimaryKeyName(linkedRecordType),
                linkThroughLookup);
            var linkFrom = linkThrough.AddLink(linkFromRecordType,
                XrmEntity.GetPrimaryKeyName(linkThroughRecordType), linkFromLookup);
            linkFrom.LinkCriteria.AddCondition(new ConditionExpression(XrmEntity.GetPrimaryKeyName(linkFromRecordType),
                ConditionOperator.Equal, linkFromId));
            return RetrieveFirst(query);
        }

        public Entity Refresh(Entity entity)
        {
            return Retrieve(entity.LogicalName, entity.Id);
        }

        public Entity Refresh(Entity entity, string[] fieldsToGet)
        {
            return Retrieve((string)entity.LogicalName, (Guid)entity.Id, fieldsToGet);
        }

        public IEnumerable<Entity> RetrieveAllFetch(string fetchXmlQuery)
        {
            return RetrieveMultiple(new FetchExpression(fetchXmlQuery)).Entities.ToArray();
        }

        internal void SetFieldIfChanging(string recordType, Guid id, string fieldName, object fieldValue)
        {
            var record = Retrieve(recordType, id, new[] { fieldName });
            var currentValue = XrmEntity.GetField(record, fieldName);
            if (!XrmEntity.FieldsEqual(currentValue, fieldValue))
            {
                XrmEntity.SetField(record, fieldName, fieldValue);
                Update(record);
            }
        }

        internal void SetFieldsIfChanging(string recordType, string fieldName,
            SortedDictionary<Guid, object> idFieldSwitches)
        {
            var existingValues = new SortedDictionary<Guid, object>();
            var ids = idFieldSwitches.Keys;
            var items = Retrieve(recordType, ids, new[] { fieldName });
            foreach (var item in items)
            {
                existingValues.Add(item.Id, XrmEntity.GetField(item, fieldName));
            }

            foreach (var id in idFieldSwitches.Keys)
            {
                if (existingValues.ContainsKey(id) && XrmEntity.FieldsEqual(existingValues[id], idFieldSwitches[id]))
                {
                    //if the value not changing for this record don't bother updating
                }
                else
                    SetField(recordType, id, fieldName, idFieldSwitches[id]);
            }
        }

        public IEnumerable<Entity> Retrieve(string entityType, IEnumerable<Guid> ids, IEnumerable<string> fields)
        {
            if (!ids.Any())
                return new Entity[0];
            var query = new QueryExpression(entityType);
            query.Criteria.AddCondition(XrmEntity.GetPrimaryKeyName(entityType), ConditionOperator.In, ids.Distinct().Cast<object>().ToArray());
            if (fields != null)
                query.ColumnSet = new ColumnSet(fields.ToArray());
            else
                query.ColumnSet = new ColumnSet(true);
            return RetrieveAll(query);
        }

        public IEnumerable<Entity> RetrieveAll(string entityName, IEnumerable<FilterExpression> orFilters)
        {
            return RetrieveAllOrClauses(entityName, orFilters, null);
        }

        public IEnumerable<Entity> RetrieveAllAndClauses(string entityName, IEnumerable<ConditionExpression> conditions)
        {
            return RetrieveAllAndClauses(entityName, conditions, null);
        }

        public IEnumerable<Entity> RetrieveAllAndClauses(string entityName, IEnumerable<ConditionExpression> conditions,
            IEnumerable<string> fields)
        {
            var query = CreateQuery(entityName, fields);
            AddAndConditions(query, conditions);
            return RetrieveAll(query);
        }

        private QueryExpression CreateQuery(string recordType, IEnumerable<string> fields)
        {
            var q = new QueryExpression(recordType);
            q.ColumnSet = CreateColumnSet(fields);
            return q;
        }

        public IEnumerable<Entity> RetrieveAllOrClauses(string entityName, IEnumerable<ConditionExpression> orConditions)
        {
            return RetrieveAllOrClauses(entityName, orConditions, null);
        }

        public IEnumerable<Entity> RetrieveAllOrClauses(string entityName, IEnumerable<ConditionExpression> orConditions,
            IEnumerable<string> fields)
        {
            var filters = orConditions
                .Select(c =>
                {
                    var f = new FilterExpression();
                    f.AddCondition(c);
                    return f;
                }
                );
            return RetrieveAllOrClauses(entityName, filters, fields);
        }

        public IEnumerable<Entity> RetrieveAllOrClauses(string entityName, IEnumerable<FilterExpression> orFilters,
            IEnumerable<string> fields)
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

        public IEnumerable<Entity> RetrieveMultiple(string entityType, string searchString, int maxCount)
        {
            var query = new QueryExpression(entityType);
            query.ColumnSet = CreateColumnSet(null);
            query.Criteria.AddCondition(GetPrimaryNameField(entityType), ConditionOperator.BeginsWith, searchString);
            return RetrieveMultiple(query).Entities.Take(maxCount);
        }

        internal string GetThisSideId(string relationshipName, string entityType, bool isReferencing)
        {
            var rMetadata = GetRelationshipMetadata(relationshipName);
            string thisSideId;
            if ((entityType != rMetadata.Entity2LogicalName))
                thisSideId = rMetadata.Entity1IntersectAttribute;
            else if (entityType == rMetadata.Entity1LogicalName && isReferencing)
                thisSideId = rMetadata.Entity1IntersectAttribute;
            else
                thisSideId = rMetadata.Entity2IntersectAttribute;
            return thisSideId;
        }

        public SortedDictionary<string, Guid?> IndexMatchingGuids(string entityName, string matchField,
            IEnumerable<string> matchValues)
        {
            var result = new SortedDictionary<string, Guid?>();
            if (matchValues != null && matchValues.Any())
            {
                var filterExpressions = new List<FilterExpression>();
                foreach (var matchValue in matchValues)
                {
                    var parseValue = ParseField(matchField, entityName, matchValue);
                    var stringValue = GetFieldAsMatchString(entityName, matchField, parseValue);
                    var filterExpression = new FilterExpression();
                    filterExpression.AddCondition(new ConditionExpression(matchField, ConditionOperator.Equal,
                        stringValue));
                    filterExpressions.Add(filterExpression);
                }
                var entities = RetrieveAllOrClauses(entityName, filterExpressions, new[] { matchField });

                foreach (var entity in entities)
                {
                    var matchValue = XrmEntity.GetStringField(entity,
                        GetFieldAsMatchString(entityName, matchField, matchField));
                    if (!result.ContainsKey(matchValue))
                        result.Add(matchValue, entity.Id);
                }
                foreach (var value in matchValues)
                {
                    if (!result.ContainsKey(value))
                        result.Add(value, null);
                }
            }
            return result;
        }

        public void Assign(Entity entity, Guid userId)
        {
            Assign(entity, userId, "systemuser");
        }

        public void Assign(Entity entity, Guid ownerId, string ownerType)
        {
            var request = new AssignRequest
            {
                Assignee = CreateLookup(ownerType, ownerId),
                Target = CreateLookup(entity)
            };
            Execute(request);
        }

        public string GetEntityDisplayCollectionName(string entityType)
        {
            return GetLabelDisplay(GetEntityMetadata(entityType).DisplayCollectionName);
        }

        public Guid Create(Entity entity, IEnumerable<string> fieldsToSubmit)
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
                throw new NullReferenceException("fieldsToSubmit Passed To Set In New Record");
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

        public void DisAssociate(string relationshipName, Guid idFrom, string typeFrom, string keyAttributeFrom, string typeTo, string keyAttributeTo, IEnumerable<Guid> relatedEntities, bool isReferencing = false)
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

        public void Activate(Entity entity)
        {
            SetState(entity.LogicalName, entity.Id, XrmPicklists.State.Active);
        }

        public void Deactivate(string entityType, Guid id)
        {
            SetState(entityType, id, XrmPicklists.State.Inactive);
        }

        public void Deactivate(Entity entity)
        {
            Deactivate(entity.LogicalName, entity.Id);
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

        public void Update(Entity entity, IEnumerable<string> fieldsToSubmit)
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

        #region Metadata Methods

        public IEnumerable<RelationshipMetadataBase> GetEntityRelationships(string entity)
        {
            lock (LockObject)
            {
                if (!EntityRelationships.ContainsKey(entity))
                {
                    Controller.LogLiteral("Retrieving " + entity + " relationship metadata");
                    var request = new RetrieveEntityRequest
                    {
                        EntityFilters = EntityFilters.Relationships,
                        LogicalName = entity
                    };
                    var response = (RetrieveEntityResponse)Execute(request);
                    Controller.LogLiteral("Retrieved " + entity + " relationship metadata");
                    EntityRelationships.Add(entity,
                        response.EntityMetadata.OneToManyRelationships
                            .Cast<RelationshipMetadataBase>()
                            .Union(response.EntityMetadata.ManyToManyRelationships).ToArray());
                }
            }
            return EntityRelationships[entity];
        }

        public IEnumerable<OneToManyRelationshipMetadata> GetEntityOneToManyRelationships(string entity)
        {
            return
                GetEntityRelationships(entity)
                    .Where(r => r is OneToManyRelationshipMetadata)
                    .Cast<OneToManyRelationshipMetadata>();
        }

        public IEnumerable<ManyToManyRelationshipMetadata> GetEntityManyToManyRelationships(string entity)
        {
            return
                GetEntityRelationships(entity)
                    .Where(r => r is ManyToManyRelationshipMetadata)
                    .Cast<ManyToManyRelationshipMetadata>();
        }

        public List<AttributeMetadata> GetEntityFieldMetadata(string entity)
        {
            lock (LockObject)
            {
                if (!EntityFieldMetadata.ContainsKey(entity))
                {
                    Controller.LogLiteral("Retrieving " + entity + " field metadata");
                    // Create the request
                    var request = new RetrieveEntityRequest
                    {
                        EntityFilters = EntityFilters.Attributes,
                        LogicalName = entity
                    };
                    var response = (RetrieveEntityResponse)Execute(request);
                    Controller.LogLiteral("Retrieved " + entity + " field metadata");
                    EntityFieldMetadata.Add(entity, new List<AttributeMetadata>(response.EntityMetadata.Attributes));
                }
            }
            return EntityFieldMetadata[entity];
        }

        #endregion

        public Entity GetFirst(string recordType)
        {
            return RetrieveFirst(CreateQuery(recordType, null));
        }

        public Entity GetFirst(string recordType, IEnumerable<string> fields)
        {
            return RetrieveFirst(CreateQuery(recordType, fields));
        }

        public IEnumerable<Entity> GetFirstX(string recordType, int x)
        {
            return RetrieveFirstX(CreateQuery(recordType, null), x);
        }

        public IEnumerable<Entity> GetFirstX(string recordType, int x, IEnumerable<ConditionExpression> conditions,
            IEnumerable<OrderExpression> orders)
        {
            return GetFirstX(recordType, x, null, conditions, orders);
        }

        public IEnumerable<Entity> GetFirstX(string recordType, int x, IEnumerable<string> fields,
            IEnumerable<ConditionExpression> conditions,
            IEnumerable<OrderExpression> orders)
        {
            var query = CreateQuery(recordType, fields);
            AddAndConditions(query, conditions);
            AddOrderExpressions(query, orders);
            return RetrieveFirstX(query, x);
        }

        public IEnumerable<Entity> RetrieveFirstX(QueryExpression query, int x)
        {
            query.PageInfo.PageNumber = 1;
            query.PageInfo.Count = x;
            var response = RetrieveMultiple(query);
            var result = response.Entities.ToArray();

            //If there is more than one page of records then keep retrieving until we get them all
            if (response.MoreRecords)
            {
                var tempHolder = new List<Entity>(result);
                while (response.MoreRecords && tempHolder.Count < x)
                {
                    query.PageInfo.PagingCookie = response.PagingCookie;
                    query.PageInfo.PageNumber = query.PageInfo.PageNumber + 1;
                    response = RetrieveMultiple(query);
                    tempHolder.AddRange(response.Entities.ToArray());
                }
                result = tempHolder.ToArray();
            }
            return result.Take(x);
        }

        /// <summary>
        ///     DONT USE USE THE PROPERTY
        /// </summary>
        private List<ManyToManyRelationshipMetadata> _allRelationshipMetadata;

        /// <summary>
        ///     DONT USE USE THE PROPERTY
        /// </summary>
        private List<OptionSetMetadata> _sharedOptionSets;

        public List<EntityMetadata> GetAllEntityMetadata()
        {
            lock (LockObject)
            {
                if (!_loadedAllEntities)
                {
                    var request = new RetrieveAllEntitiesRequest
                    {
                        EntityFilters = EntityFilters.Entity,
                    };
                    var response = (RetrieveAllEntitiesResponse)Execute(request);
                    _entityMetadata.Clear();
                    _entityMetadata.AddRange(response.EntityMetadata);
                    _loadedAllEntities = true;
                }
                return _entityMetadata;
            }
        }

        private List<OptionSetMetadata> SharedOptionSets
        {
            get
            {
                lock (LockObject)
                {
                    if (_sharedOptionSets == null)
                    {
                        var request = new RetrieveAllOptionSetsRequest();
                        var response = (RetrieveAllOptionSetsResponse)Execute(request);
                        _sharedOptionSets =
                            new List<OptionSetMetadata>(
                                response.OptionSetMetadata.Where(
                                    m => m is OptionSetMetadata && m.IsGlobal.HasValue && m.IsGlobal.Value)
                                    .Cast<OptionSetMetadata>()
                                    .ToArray());
                    }
                    return _sharedOptionSets;
                }
            }
        }

        private List<ManyToManyRelationshipMetadata> AllRelationshipMetadata
        {
            get
            {
                lock (LockObject)
                {
                    if (_allRelationshipMetadata == null)
                    {
                        var request = new RetrieveAllEntitiesRequest
                        {
                            EntityFilters = EntityFilters.Relationships,
                        };
                        var response = (RetrieveAllEntitiesResponse)Execute(request);
                        _allRelationshipMetadata = new List<ManyToManyRelationshipMetadata>();
                        foreach (
                            var relationship in
                                response.EntityMetadata.SelectMany(m => m.ManyToManyRelationships))
                        {
                            if (!_allRelationshipMetadata.Any(r => r.SchemaName == relationship.SchemaName))
                                _allRelationshipMetadata.Add(relationship);
                        }
                    }
                    return _allRelationshipMetadata;
                }
            }
        }

        /// <summary>
        ///     DOESN'T UPDATE PRIMARY FIELD - CALL THE CREATEORUPDATESTRING METHOD
        /// </summary>
        public void CreateOrUpdateEntity(string schemaName, string displayName, string displayCollectionName,
            string description, bool audit, string primaryFieldSchemaName,
            string primaryFieldDisplayName, string primaryFieldDescription,
            int primaryFieldMaxLength, bool primaryFieldIsMandatory, bool primaryFieldAudit, bool isActivityType,
            bool notes, bool activities, bool connections, bool mailMerge, bool queues)
        {
            lock (LockObject)
            {
                var metadata = new EntityMetadata();

                var exists = EntityExists(schemaName);
                if (exists)
                    metadata = GetEntityMetadata(schemaName);

                metadata.SchemaName = schemaName;
                metadata.LogicalName = schemaName;
                metadata.DisplayName = new Label(displayName, 1033);
                metadata.DisplayCollectionName = new Label(displayCollectionName, 1033);
                metadata.IsAuditEnabled = new BooleanManagedProperty(audit);
                metadata.IsActivity = isActivityType;
                metadata.IsValidForQueue = new BooleanManagedProperty(queues);
                metadata.IsMailMergeEnabled = new BooleanManagedProperty(mailMerge);
                metadata.IsConnectionsEnabled = new BooleanManagedProperty(connections);
                metadata.IsActivity = isActivityType;
                if (!String.IsNullOrWhiteSpace(description))
                    metadata.Description = new Label(description, 1033);

                if (exists)
                {
                    var request = new UpdateEntityRequest
                    {
                        Entity = metadata,
                        HasActivities = activities,
                        HasNotes = notes
                    };
                    Execute(request);
                }
                else
                {
                    metadata.OwnershipType = OwnershipTypes.UserOwned;

                    var primaryFieldMetadata = new StringAttributeMetadata();
                    SetCommon(primaryFieldMetadata, primaryFieldSchemaName, primaryFieldDisplayName,
                        primaryFieldDescription,
                        primaryFieldIsMandatory, primaryFieldAudit, true);
                    primaryFieldMetadata.MaxLength = primaryFieldMaxLength;

                    var request = new CreateEntityRequest
                    {
                        Entity = metadata,
                        PrimaryAttribute = primaryFieldMetadata,
                        HasActivities = activities,
                        HasNotes = notes
                    };
                    Execute(request);
                    RefreshFieldMetadata(primaryFieldSchemaName, schemaName);
                }
                RefreshEntityMetadata(schemaName);
            }
        }

        private void RefreshEntityMetadata(string schemaName)
        {
            lock (LockObject)
            {
                var request = new RetrieveEntityRequest
                {
                    EntityFilters = EntityFilters.Attributes,
                    LogicalName = schemaName
                };
                var response = (RetrieveEntityResponse)Execute(request);
                if (EntityExists(schemaName))
                    GetAllEntityMetadata().Remove(GetAllEntityMetadata().First(m => m.SchemaName == schemaName));
                GetAllEntityMetadata().Add(response.EntityMetadata);
            }
        }

        public bool EntityExists(string schemaName)
        {
            return GetAllEntityMetadata().Any(m => m.SchemaName == schemaName);
        }

        private AttributeRequiredLevelManagedProperty GetRequiredLevel(bool isRequired)
        {
            return isRequired
                ? new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.ApplicationRequired)
                : new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None);
        }

        public void PublishAll()
        {
            Execute(new PublishAllXmlRequest());
        }

        private void RefreshFieldMetadata(string schemaName, string entityType)
        {
            lock (LockObject)
            {
                var fieldMetadata = GetEntityFieldMetadata(entityType);
                if (fieldMetadata.Any(m => m.SchemaName == schemaName))
                    fieldMetadata.Remove(fieldMetadata.Single(m => m.SchemaName == schemaName));

                var request = new RetrieveAttributeRequest
                {
                    EntityLogicalName = entityType,
                    LogicalName = schemaName,
                    RetrieveAsIfPublished = true
                };
                var response = (RetrieveAttributeResponse)Execute(request);
                fieldMetadata.Add(response.AttributeMetadata);
            }
        }

        public void CreateOrUpdateBooleanAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable, string recordType)
        {
            var optionSet = new BooleanOptionSetMetadata();
            optionSet.FalseOption = new OptionMetadata(new Label("No", 1033), 0);
            optionSet.TrueOption = new OptionMetadata(new Label("Yes", 1033), 1);

            BooleanAttributeMetadata metadata;
            if (FieldExists(schemaName, recordType))
                metadata = (BooleanAttributeMetadata)GetFieldMetadata(schemaName, recordType);
            else
                metadata = new BooleanAttributeMetadata();

            SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);
            metadata.OptionSet = optionSet;

            CreateOrUpdateAttribute(schemaName, recordType, metadata);
        }

        private void SetCommon(AttributeMetadata metadata, string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable)
        {
            metadata.SchemaName = schemaName;
            metadata.DisplayName = new Label(displayName, 1033);
            metadata.LogicalName = schemaName;
            if (!string.IsNullOrWhiteSpace(description))
                metadata.Description = new Label(description, 1033);
            metadata.RequiredLevel = GetRequiredLevel(isRequired);
            metadata.IsAuditEnabled = new BooleanManagedProperty(audit);
            metadata.IsValidForAdvancedFind = new BooleanManagedProperty(searchable);
        }

        private void CreateOrUpdateAttribute(string schemaName, string recordType, AttributeMetadata metadata)
        {
            lock (LockObject)
            {
                if (FieldExists(schemaName, recordType))
                {
                    var request = new UpdateAttributeRequest
                    {
                        EntityName = recordType,
                        Attribute = metadata,
                    };
                    Execute(request);
                    RefreshFieldMetadata(schemaName, recordType);
                }
                else
                {
                    var request = new CreateAttributeRequest
                    {
                        EntityName = recordType,
                        Attribute = metadata
                    };
                    Execute(request);
                    RefreshFieldMetadata(schemaName, recordType);
                }
            }
        }

        public bool FieldExists(string fieldName, string recordType)
        {
            return GetEntityFieldMetadata(recordType).Any(m => m.LogicalName == fieldName);
        }

        public void CreateOrUpdateDateAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable, string recordType,
            bool includeTime)
        {
            DateTimeAttributeMetadata metadata;
            if (FieldExists(schemaName, recordType))
                metadata = (DateTimeAttributeMetadata)GetFieldMetadata(schemaName, recordType);
            else
                metadata = new DateTimeAttributeMetadata();

            SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);

            metadata.Format = includeTime ? DateTimeFormat.DateAndTime : DateTimeFormat.DateOnly;

            CreateOrUpdateAttribute(schemaName, recordType, metadata);
        }

        public void CreateOrUpdateDecimalAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable, string recordType,
            decimal minimum, decimal maximum)
        {
            DecimalAttributeMetadata metadata;
            if (FieldExists(schemaName, recordType))
                metadata = (DecimalAttributeMetadata)GetFieldMetadata(schemaName, recordType);
            else
                metadata = new DecimalAttributeMetadata();

            SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);

            metadata.MinValue = minimum;
            metadata.MaxValue = maximum;

            CreateOrUpdateAttribute(schemaName, recordType, metadata);
        }

        public void CreateOrUpdateMoneyAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable, string recordType,
            double minimum, double maximum)
        {
            MoneyAttributeMetadata metadata;
            if (FieldExists(schemaName, recordType))
                metadata = (MoneyAttributeMetadata)GetFieldMetadata(schemaName, recordType);
            else
                metadata = new MoneyAttributeMetadata();

            SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);

            metadata.MinValue = minimum;
            metadata.MaxValue = maximum;

            CreateOrUpdateAttribute(schemaName, recordType, metadata);
        }

        public void CreateOrUpdateIntegerAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable,
            string recordType, int minimum, int maximum)
        {
            IntegerAttributeMetadata metadata;
            if (FieldExists(schemaName, recordType))
                metadata = (IntegerAttributeMetadata)GetFieldMetadata(schemaName, recordType);
            else
                metadata = new IntegerAttributeMetadata();

            SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);

            metadata.MinValue = minimum;
            metadata.MaxValue = maximum;

            CreateOrUpdateAttribute(schemaName, recordType, metadata);
        }

        public void CreateOrUpdateLookupAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable, string recordType,
            string referencedEntityType, bool displayInRelated)
        {
            lock (LockObject)
            {
                LookupAttributeMetadata metadata;
                if (FieldExists(schemaName, recordType))
                    metadata = (LookupAttributeMetadata)GetFieldMetadata(schemaName, recordType);
                else
                    metadata = new LookupAttributeMetadata();

                SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);

                if (FieldExists(schemaName, recordType))
                {
                    CreateOrUpdateAttribute(schemaName, recordType, metadata);
                    var relationships =
                        GetEntityOneToManyRelationships(referencedEntityType);
                    var relationship = relationships.First(r => r.ReferencingAttribute.ToLower() == schemaName);
                    var newBehvaiour = displayInRelated
                        ? AssociatedMenuBehavior.UseCollectionName
                        : AssociatedMenuBehavior.DoNotDisplay;
                    if (newBehvaiour != relationship.AssociatedMenuConfiguration.Behavior)
                    {
                        relationship.AssociatedMenuConfiguration.Behavior = displayInRelated
                            ? AssociatedMenuBehavior.UseCollectionName
                            : AssociatedMenuBehavior.DoNotDisplay;
                        var request = new UpdateRelationshipRequest()
                        {
                            Relationship = relationship
                        };
                        Execute(request);
                        RefreshEntityMetadata(recordType);
                        RefreshEntityMetadata(referencedEntityType);
                    }
                }
                else
                {
                    var request = new CreateOneToManyRequest
                    {
                        OneToManyRelationship = new OneToManyRelationshipMetadata
                        {
                            SchemaName = string.Format("{0}_{1}_{2}", recordType, referencedEntityType, schemaName),
                            AssociatedMenuConfiguration = new AssociatedMenuConfiguration
                            {
                                Behavior =
                                    displayInRelated
                                        ? AssociatedMenuBehavior.UseCollectionName
                                        : AssociatedMenuBehavior.DoNotDisplay
                            },
                            ReferencingEntity = recordType,
                            ReferencedEntity = referencedEntityType
                        },
                        Lookup = metadata
                    };

                    Execute(request);
                    RefreshFieldMetadata(schemaName, recordType);
                    CreateOrUpdateAttribute(schemaName, recordType, metadata);
                    RefreshEntityMetadata(recordType);
                    RefreshEntityMetadata(referencedEntityType);
                }
            }
        }

        /// <summary>
        ///     DOESN'T UPDATE THE PICKLIST ITSELF - CALL THE UPDATEPICKLISTOPTIONS OR CREATEORUPDATESHAREDOPTIONSET METHOD
        /// </summary>
        public void CreateOrUpdatePicklistAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable,
            string recordType, string sharedOptionSetName)
        {
            lock (LockObject)
            {
                PicklistAttributeMetadata metadata;
                var exists = FieldExists(schemaName, recordType);
                if (exists)
                    metadata = (PicklistAttributeMetadata)GetFieldMetadata(schemaName, recordType);
                else
                    metadata = new PicklistAttributeMetadata();

                SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);

                metadata.OptionSet = new OptionSetMetadata { Name = sharedOptionSetName, IsGlobal = true };

                CreateOrUpdateAttribute(schemaName, recordType, metadata);
            }
        }

        public void CreateOrUpdatePicklistAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable,
            string recordType, IEnumerable<KeyValuePair<int, string>> options)
        {
            lock (LockObject)
            {
                var optionSet = new OptionSetMetadata
                {
                    OptionSetType = OptionSetType.Picklist,
                    IsGlobal = false
                };
                optionSet.Options.AddRange(options.Select(o => new OptionMetadata(new Label(o.Value, 1033), o.Key)));

                PicklistAttributeMetadata metadata;
                var exists = FieldExists(schemaName, recordType);
                if (exists)
                    metadata = (PicklistAttributeMetadata)GetFieldMetadata(schemaName, recordType);
                else
                    metadata = new PicklistAttributeMetadata();

                SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);

                metadata.OptionSet = optionSet;

                CreateOrUpdateAttribute(schemaName, recordType, metadata);
            }
        }

        public void UpdatePicklistOptions(string fieldName, string recordType,
            IEnumerable<KeyValuePair<int, string>> optionSet)
        {
            if (optionSet.Any())
            {
                var existingOptions = GetPicklistKeyValues(recordType, fieldName);
                var itemUpdated = false;
                foreach (var option in existingOptions)
                {
                    if (!optionSet.Any(o => o.Key == option.Key))
                    {
                        var request = new DeleteOptionValueRequest
                        {
                            AttributeLogicalName = fieldName,
                            EntityLogicalName = recordType,
                            Value = option.Key
                        };
                        Execute(request);
                        itemUpdated = true;
                    }
                    else if (optionSet.Any(o => o.Key == option.Key && o.Value != option.Value))
                    {
                        var newValue = optionSet.Single(o => o.Key == option.Key);
                        var request = new UpdateOptionValueRequest
                        {
                            AttributeLogicalName = fieldName,
                            EntityLogicalName = recordType,
                            Value = option.Key,
                            Label = new Label(newValue.Value, 1033)
                        };
                        Execute(request);
                        itemUpdated = true;
                    }
                }
                foreach (var option in optionSet)
                {
                    if (!existingOptions.Any(o => o.Key == option.Key))
                    {
                        var request = new InsertOptionValueRequest
                        {
                            AttributeLogicalName = fieldName,
                            EntityLogicalName = recordType,
                            Value = option.Key,
                            Label = new Label(option.Value, 1033)
                        };
                        Execute(request);
                        itemUpdated = true;
                    }
                }
                if (itemUpdated)
                    RefreshFieldMetadata(fieldName, recordType);
            }
        }

        public void CreateOrUpdateStringAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable,
            string recordType, int? maxLength, StringFormat stringFormat)
        {
            StringAttributeMetadata metadata;
            if (FieldExists(schemaName, recordType))
                metadata = (StringAttributeMetadata)GetFieldMetadata(schemaName, recordType);
            else
                metadata = new StringAttributeMetadata();
            SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);

            metadata.MaxLength = maxLength;
            metadata.Format = stringFormat;

            CreateOrUpdateAttribute(schemaName, recordType, metadata);

            if (GetEntityMetadata(recordType).PrimaryNameAttribute == schemaName)
                RefreshEntityMetadata(recordType);
        }

        public void CreateOrUpdateMemoAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable,
            string recordType, int? maxLength)
        {
            MemoAttributeMetadata metadata;
            if (FieldExists(schemaName, recordType))
                metadata = (MemoAttributeMetadata)GetFieldMetadata(schemaName, recordType);
            else
                metadata = new MemoAttributeMetadata();
            SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);

            metadata.MaxLength = maxLength;

            CreateOrUpdateAttribute(schemaName, recordType, metadata);

            if (GetEntityMetadata(recordType).PrimaryNameAttribute == schemaName)
                RefreshEntityMetadata(recordType);
        }

        public void CreateOrUpdateStatusAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable,
            string recordType)
        {
            StatusAttributeMetadata metadata;
            if (FieldExists(schemaName, recordType))
                metadata = (StatusAttributeMetadata)GetFieldMetadata(schemaName, recordType);
            else
                metadata = new StatusAttributeMetadata();
            SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);

            CreateOrUpdateAttribute(schemaName, recordType, metadata);

            if (GetEntityMetadata(recordType).PrimaryNameAttribute == schemaName)
                RefreshEntityMetadata(recordType);
        }

        /// <summary>
        ///     WARNING!! DELETES THE CUSTOM ENTITY
        /// </summary>
        /// <param name="schemaName"></param>
        public void DeleteEntity(string schemaName)
        {
            lock (LockObject)
            {
                var tempRequest1 = new RetrieveEntityRequest
                {
                    LogicalName = schemaName,
                    EntityFilters = EntityFilters.Relationships
                };
                var response = (RetrieveEntityResponse)Execute(tempRequest1);
                foreach (var r in response.EntityMetadata.OneToManyRelationships)
                {
                    if (r.IsCustomRelationship.HasValue && r.IsCustomRelationship.Value)
                    {
                        var tempRequest2 = new DeleteRelationshipRequest
                        {
                            Name = r.SchemaName
                        };
                        Execute(tempRequest2);
                    }
                }

                var request = new DeleteEntityRequest();
                request.LogicalName = schemaName;
                Execute(request);
                if (GetAllEntityMetadata().Any(m => m.SchemaName == schemaName))
                    GetAllEntityMetadata().Remove(GetAllEntityMetadata().Single(m => m.SchemaName == schemaName));
                if (_entityFieldMetadata.ContainsKey(schemaName))
                    _entityFieldMetadata.Remove(schemaName);
            }
        }

        public void CreateOrUpdateRelationship(string schemaName, string entityType1, string entityType2,
            bool entityType1DisplayRelated, bool entityType2DisplayRelated)
        {
            var metadata = new ManyToManyRelationshipMetadata();
            lock (LockObject)
            {
                var exists = RelationshipExists(schemaName);
                if (exists)
                    metadata = GetRelationshipMetadata(schemaName);
                metadata.SchemaName = schemaName;
                metadata.IntersectEntityName = schemaName;
                metadata.Entity1LogicalName = entityType1;
                metadata.Entity2LogicalName = entityType2;
                metadata.Entity1AssociatedMenuConfiguration = new AssociatedMenuConfiguration()
                {
                    Behavior = entityType1DisplayRelated
                        ? AssociatedMenuBehavior.UseCollectionName
                        : AssociatedMenuBehavior.DoNotDisplay
                };
                metadata.Entity2AssociatedMenuConfiguration = new AssociatedMenuConfiguration()
                {
                    Behavior = entityType2DisplayRelated
                        ? AssociatedMenuBehavior.UseCollectionName
                        : AssociatedMenuBehavior.DoNotDisplay
                };
                if (exists)
                {
                    var request = new UpdateRelationshipRequest
                    {
                        Relationship = metadata
                    };
                    Execute(request);
                }
                else
                {
                    var request = new CreateManyToManyRequest
                    {
                        IntersectEntitySchemaName = schemaName,
                        ManyToManyRelationship = metadata
                    };
                    Execute(request);
                }
                RefreshRelationshipMetadata(schemaName);
            }
        }

        private void RefreshRelationshipMetadata(string schemaName)
        {
            lock (LockObject)
            {
                var metadata = AllRelationshipMetadata;
                if (metadata.Any(m => m.SchemaName == schemaName))
                    metadata.Remove(metadata.Single(m => m.SchemaName == schemaName));

                var request = new RetrieveRelationshipRequest
                {
                    Name = schemaName
                };
                var response = (RetrieveRelationshipResponse)Execute(request);
                metadata.Add((ManyToManyRelationshipMetadata)response.RelationshipMetadata);
            }
        }

        public bool RelationshipExists(string schemaName)
        {
            return AllRelationshipMetadata.Any(m => m.SchemaName == schemaName);
        }

        public void DeleteRelationship(string relationshipName)
        {
            var request = new DeleteRelationshipRequest
            {
                Name = relationshipName
            };
            Execute(request);
            if (AllRelationshipMetadata.Any(r => r.SchemaName == relationshipName))
            {
                var relationship = GetRelationshipMetadata(relationshipName);
                AllRelationshipMetadata.Remove(relationship);
            }
        }

        //public void CreateLookupAttribute(string fieldName, string displayName, string recordType, string targetType)
        //{
        //    if (FieldExists(fieldName, recordType))
        //        throw new ArgumentException("Field Already Exists");
        //    CreateOrUpdateLookupAttribute(fieldName, displayName, null, false, false, false, recordType, targetType);
        //}

        public void CreateDateAttribute(string fieldName, string displayName, string recordType)
        {
            if (FieldExists(fieldName, recordType))
                throw new ArgumentException("Field Already Exists");
            CreateOrUpdateDateAttribute(fieldName, displayName, null, false, false, false, recordType, false);
        }

        public void DeleteField(string fieldName, string entityName)
        {
            if (FieldExists(fieldName, entityName))
            {
                var request = new DeleteAttributeRequest
                {
                    EntityLogicalName = entityName,
                    LogicalName = fieldName
                };
                Execute(request);
            }
        }

        public bool SharedOptionSetExists(string schemaName)
        {
            return SharedOptionSets.Any(s => s.Name == schemaName);
        }

        public void CreateOrUpdateSharedOptionSet(string schemaName, string displayName,
            IEnumerable<KeyValuePair<int, string>> options)
        {
            if (SharedOptionSetExists(schemaName))
            {
                var optionSetMetadata = GetSharedOptionSet(schemaName);
                optionSetMetadata.DisplayName = new Label(displayName, 1033);
                var updateOptionSetRequest = new UpdateOptionSetRequest
                {
                    OptionSet = optionSetMetadata
                };
                Execute(updateOptionSetRequest);
                if (options.Any())
                {
                    var existingOptions = OptionSetToKeyValues(optionSetMetadata.Options);
                    var optionSet = options.ToArray();
                    foreach (var option in existingOptions)
                    {
                        if (!optionSet.Any(o => o.Key == option.Key))
                        {
                            var request = new DeleteOptionValueRequest
                            {
                                OptionSetName = schemaName,
                                Value = option.Key
                            };
                            Execute(request);
                        }
                        else if (optionSet.Any(o => o.Key == option.Key && o.Value != option.Value))
                        {
                            var newValue = optionSet.Single(o => o.Key == option.Key);
                            var request = new UpdateOptionValueRequest
                            {
                                OptionSetName = schemaName,
                                Value = option.Key,
                                Label = new Label(newValue.Value, 1033)
                            };
                            Execute(request);
                        }
                    }
                    foreach (var option in optionSet)
                    {
                        if (!existingOptions.Any(o => o.Key == option.Key))
                        {
                            var request = new InsertOptionValueRequest
                            {
                                OptionSetName = schemaName,
                                Value = option.Key,
                                Label = new Label(option.Value, 1033)
                            };
                            Execute(request);
                        }
                    }
                }
            }
            else
            {
                var optionSetMetadata = new OptionSetMetadata();
                optionSetMetadata.Name = schemaName;
                optionSetMetadata.DisplayName = new Label(displayName, 1033);
                optionSetMetadata.IsGlobal = true;
                optionSetMetadata.Options.AddRange(
                    options.Select(o => new OptionMetadata(new Label(o.Value, 1033), o.Key)).ToList());

                var request = new CreateOptionSetRequest { OptionSet = optionSetMetadata };
                Execute(request);
            }
            RefreshSharedOptionValues(schemaName);
        }

        private void RefreshSharedOptionValues(string schemaName)
        {
            lock (LockObject)
            {
                if (SharedOptionSets.Any(m => m.Name == schemaName))
                    SharedOptionSets.Remove(SharedOptionSets.Single(m => m.Name == schemaName));

                var request = new RetrieveOptionSetRequest
                {
                    Name = schemaName
                };
                var response = (RetrieveOptionSetResponse)Execute(request);
                SharedOptionSets.Add((OptionSetMetadata)response.OptionSetMetadata);
            }
        }

        private OptionSetMetadata GetSharedOptionSet(string schemaName)
        {
            if (!SharedOptionSets.Any(s => s.Name == schemaName))
                throw new ArgumentException("Error getting option set: " + schemaName);
            return SharedOptionSets.Single(s => s.Name == schemaName);
        }

        public IEnumerable<KeyValuePair<int, string>> GetSharedOptionSetKeyValues(string schemaName)
        {
            return OptionSetToKeyValues(GetSharedOptionSet(schemaName).Options);
        }

        public void DeleteSharedOptionSet(string schemaName)
        {
            var request = new DeleteOptionSetRequest
            {
                Name = schemaName
            };
            Execute(request);
            if (SharedOptionSets.Any(o => o.Name == schemaName))
                SharedOptionSets.Remove(SharedOptionSets.Single(o => o.Name == schemaName));
        }

        public IEnumerable<string> GetAllEntityTypes()
        {
            return GetAllEntityMetadata()
                .Where(m => !(m.IsIntersect ?? false))
                .Select(e => e.LogicalName)
               .ToArray();
        }

        public IEnumerable<ExecuteMultipleResponseItem> UpdateMultiple(IEnumerable<Entity> entities,
            IEnumerable<string> fields)
        {
            var responses = ExecuteMultiple(entities
                .Select(e => ReplicateWithFields(e, fields))
                .Select(e => new UpdateRequest() { Target = e }));

            return responses.ToArray();
        }

        public IEnumerable<ExecuteMultipleResponseItem> CreateMultiple(IEnumerable<Entity> entities)
        {
            var response = ExecuteMultiple(entities.Where(e => e != null).Select(e => new CreateRequest() { Target = e }));
            return response.ToArray();
        }

        public IEnumerable<ExecuteMultipleResponseItem> DeleteMultiple(IEnumerable<Entity> entities)
        {
            var response =
                ExecuteMultiple(
                    entities.Where(e => e != null)
                        .Select(e => new DeleteRequest() { Target = new EntityReference(e.LogicalName, e.Id) }));
            return response.ToArray();
        }

        public IEnumerable<ExecuteMultipleResponseItem> ExecuteMultiple(IEnumerable<OrganizationRequest> requests)
        {
            var responses = new List<ExecuteMultipleResponseItem>();

            var requestsArray = requests.ToArray();
            var requestsArrayCount = requestsArray.Count();

            var request = CreateExecuteMultipleRequest();

            var currentSetSize = 0;
            for (var i = 0; i < requestsArrayCount; i++)
            {
                var organizationRequest = requestsArray.ElementAt(i);

                request.Requests.Add(organizationRequest);
                currentSetSize++;
                if (currentSetSize == 50 || i == requestsArrayCount - 1)
                {
                    var response = (ExecuteMultipleResponse)Execute(request);
                    foreach (var r in response.Responses)
                        r.RequestIndex = i - currentSetSize + r.RequestIndex + 1;
                    responses.AddRange(response.Responses);
                    request = CreateExecuteMultipleRequest();
                    currentSetSize = 0;
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

        public string GetFieldAsDisplayString(string recordType, string fieldName, object value, bool isHtml = false, string func = null)
        {
            if (value == null)
                return "";
            else if (value is string)
            {
                if (isHtml)
                    return ((string)value).Replace(Environment.NewLine, "<br />");
                else
                    return ((string)value);
            }
            else if (IsLookup(fieldName, recordType))
            {
                return XrmEntity.GetLookupName(value);
            }
            else if (IsOptionSet(fieldName, recordType))
            {
                if (value is OptionSetValue)
                    return GetOptionLabel(((OptionSetValue)value).Value, fieldName, recordType);
                throw new Exception("Value Type Not Matched For OptionSetValue " + value.GetType().Name);
            }
            else if (IsMoney(fieldName, recordType))
            {
                return XrmEntity.GetMoneyValue(value).ToString(StringFormats.MoneyFormat);
            }
            else if (IsDate(fieldName, recordType))
            {
                if (value is DateTime)
                {
                    if (func == "year")
                        return ((DateTime)value).ToLocalTime().ToString("yyyy");
                    if (GetDateFormat(fieldName, recordType) == DateTimeFormat.DateAndTime)
                        return ((DateTime)value).ToLocalTime().ToString(StringFormats.DateTimeFormat);
                    return ((DateTime)value).ToLocalTime().Date.ToString(StringFormats.DateFormat);
                }
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
                    return string.Join(", ", namesToOutput.Where(f => !f.IsNullOrWhiteSpace()));
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

        private bool IsDate(string fieldName, string recordType)
        {
            var fieldType = GetFieldType(fieldName, recordType);
            return fieldType == AttributeTypeCode.DateTime;
        }

        private DateTimeFormat? GetDateFormat(string fieldName, string recordType)
        {
            var metadata = (DateTimeAttributeMetadata)GetFieldMetadata(fieldName, recordType);
            return metadata.Format;
        }

        private bool IsOptionSet(string fieldName, string recordType)
        {
            var fieldType = GetFieldType(fieldName, recordType);
            return fieldType == AttributeTypeCode.Picklist || fieldType == AttributeTypeCode.Status ||
                   fieldType == AttributeTypeCode.State;
        }

        private bool IsMoney(string fieldName, string recordType)
        {
            var fieldType = GetFieldType(fieldName, recordType);
            return fieldType == AttributeTypeCode.Money;
        }

        public int GetObjectTypeCode(string recordType)
        {
            var entity = GetEntityMetadata(recordType);
            if (!entity.ObjectTypeCode.HasValue)
                throw new NullReferenceException("ObjectTypeCode Is Null");
            return entity.ObjectTypeCode.Value;
        }

        public string GetDatabaseName()
        {
            return XrmConfiguration.OrganizationUniqueName + "_MSCRM";
        }

        public OneToManyRelationshipMetadata GetOneToManyRelationship(string referencedRecordType,
            string relationshipName)
        {
            var relationships = GetEntityOneToManyRelationships(referencedRecordType);
            if (relationships.Any(r => r.SchemaName == relationshipName))
            {
                return relationships.First(r => r.SchemaName == relationshipName);
            }
            throw new ArgumentOutOfRangeException("relationshipName",
                "No Relationship Exists With The name: " + relationshipName);
        }

        public string GetOneToManyRelationshipLabel(string referencedRecordType, string relationshipName)
        {
            var relationship = GetOneToManyRelationship(referencedRecordType, relationshipName);
            var menuConfiguration = relationship.AssociatedMenuConfiguration;
            if (menuConfiguration.Behavior.HasValue && menuConfiguration.Behavior == AssociatedMenuBehavior.UseLabel)
            {
                var labelString = GetLabelDisplay(menuConfiguration.Label);
                if (!labelString.IsNullOrWhiteSpace())
                    return labelString;
            }
            return GetEntityCollectionName(relationship.ReferencingEntity);
        }

        public string GetManyToManyRelationshipLabel(string referencedRecordType, string relationshipName, bool from1)
        {
            var relationships = GetEntityManyToManyRelationships(referencedRecordType);
            if (relationships.Any(r => r.SchemaName == relationshipName))
            {
                var relationship = relationships.First(r => r.SchemaName == relationshipName);
                var menuConfiguration =
                    from1
                        ? relationship.Entity2AssociatedMenuConfiguration
                        : relationship.Entity1AssociatedMenuConfiguration;

                if (menuConfiguration.Behavior.HasValue && menuConfiguration.Behavior == AssociatedMenuBehavior.UseLabel)
                {
                    var labelString = GetLabelDisplay(menuConfiguration.Label);
                    if (!labelString.IsNullOrWhiteSpace())
                        return labelString;
                }
                return GetEntityCollectionName(from1 ? relationship.Entity2LogicalName : relationship.Entity1LogicalName);
            }
            throw new ArgumentOutOfRangeException("relationshipName",
                "No Relationship Exists With The name: " + relationshipName);
        }

        public bool IsActivityParty(string field, string recordType)
        {
            return GetFieldType(field, recordType) == AttributeTypeCode.PartyList;
        }

        public IEnumerable<Entity> GetLinkedRecordsThroughBridge(string linkedRecordType, string recordTypeThrough,
            string recordTypeFrom, string linkedThroughLookupFrom, string linkedThroughLookupTo, Guid recordFromId)
        {
            var query = CreateQuery(linkedRecordType, null);
            var bridgeLink = query.AddLink(recordTypeThrough, GetPrimaryKeyField(linkedRecordType),
                linkedThroughLookupTo);
            var fromLink = bridgeLink.AddLink(recordTypeFrom, linkedThroughLookupFrom,
                GetPrimaryKeyField(recordTypeFrom));
            fromLink.LinkCriteria.AddCondition(GetPrimaryKeyField(recordTypeFrom), ConditionOperator.Equal, recordFromId);
            return RetrieveAll(query);
        }

        public bool IsWritable(string fieldName, string recordType)
        {
            return GetFieldMetadata(fieldName, recordType).IsValidForUpdate ?? false;
        }

        public IEnumerable<string> GetFields(string recordType)
        {
            return GetEntityFieldMetadata(recordType).Select(a => a.LogicalName).ToArray<string>();
        }

        public Entity UpdateAndRetreive(Entity entity)
        {
            Update(entity);
            return Retrieve(entity.LogicalName, entity.Id);
        }

        public Entity UpdateAndRetreive(Entity entity, IEnumerable<string> fieldsToUpdate)
        {
            Update(entity, fieldsToUpdate);
            return Retrieve(entity.LogicalName, entity.Id);
        }

        public static QueryExpression BuildQuery(string entityType, IEnumerable<string> fields,
            IEnumerable<ConditionExpression> filters, IEnumerable<string> sortFields = null)
        {
            var query = new QueryExpression(entityType);
            if (filters != null)
            {
                foreach (var condition in filters)
                    query.Criteria.AddCondition(condition);
            }
            if (sortFields != null)
            {
                foreach (var sortField in sortFields)
                    query.AddOrder(sortField, OrderType.Ascending);
            }
            if (fields != null)
                query.ColumnSet = new ColumnSet(fields.ToArray());
            else
                query.ColumnSet = new ColumnSet(true);

            return query;
        }

        public IEnumerable<TimeInfo> GetSchedule(Guid resourceId, DateTime start, DateTime end)
        {
            var req = new QueryScheduleRequest
            {
                End = end,
                Start = start,
                ResourceId = resourceId,
                TimeCodes = new[] { TimeCode.Available }
            };
            var response = (QueryScheduleResponse)Execute(req);
            return response.TimeInfos;
        }

        public IEnumerable<TimeInfo> GetCalendar(Guid calendarId, DateTime start, DateTime end)
        {
            var req = new ExpandCalendarRequest()
            {
                End = end,
                Start = start,
                CalendarId = calendarId
            };
            var response = (ExpandCalendarResponse)Execute(req);
            return response.result;
        }

        public Entity InitialiseEmailFromTemplate(string regardingType, Guid regardingId, Guid templateId)
        {
            var request = new InstantiateTemplateRequest()
            {
                ObjectId = regardingId,
                ObjectType = regardingType,
                TemplateId = templateId
            };
            var response = (InstantiateTemplateResponse)Execute(request);
            return response.EntityCollection.Entities.First();
        }

        public void SendEmail(Guid emailId)
        {
            var request = new SendEmailRequest()
            {
                EmailId = emailId,
                TrackingToken = "",
                IssueSend = true
            };
            Execute(request);
        }


        public void StartWorkflow(Guid workflowId, Guid targetId)
        {
            var request = new ExecuteWorkflowRequest { EntityId = targetId, WorkflowId = workflowId };
            Execute(request);
        }

        public IEnumerable<Entity> GetEmailAttachments(Guid emailId, IEnumerable<string> fields)
        {
            var conditions = new[] { new ConditionExpression("objectid", ConditionOperator.Equal, emailId) };
            return RetrieveAllAndClauses("activitymimeattachment", conditions);
        }

        public bool IsCalendarAvailable(Guid calendarid)
        {
            var now = DateTime.UtcNow;
            var start = now.ToUniversalTime();
            var end = start.AddDays(1).ToUniversalTime();
            var schedule = GetCalendar(calendarid, start, end);
            var active =
                schedule.Any(s => s.Start.HasValue && s.Start.Value < now && s.End.HasValue && s.End.Value > now);

            var exclusionDayRules = new List<Entity>();
            //okay since the expand calendar not including calendars
            var calendar = Retrieve("calendar", calendarid, new[] { "holidayschedulecalendarid" });
            var holidayCalendarId = calendar.GetLookupGuid("holidayschedulecalendarid");

            if (holidayCalendarId.HasValue)
            {
                var holidayCalendar = Retrieve("calendar", holidayCalendarId.Value);
                exclusionDayRules.AddRange(holidayCalendar.GetEntitiesField("calendarrules"));

                var closureCalendar = Retrieve("calendar", OrganisationSettings.BusinessClosureCalendarId);
                exclusionDayRules.AddRange(closureCalendar.GetEntitiesField("calendarrules"));
            }

            now = DateTime.UtcNow;
            var localNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(now, TimeZoneId);

            foreach (var rule in exclusionDayRules)
            {
                var exclusionStart = rule.GetDateTimeField("effectiveintervalstart");
                var exclusionEnd = rule.GetDateTimeField("effectiveintervalend");
                if (exclusionStart.HasValue && exclusionEnd.HasValue && exclusionStart.Value <= localNow &&
                    exclusionEnd.Value >= localNow)
                {
                    active = false;
                    break;
                }
            }
            return active;
        }

        public bool IsCustomAttribute(string field, string entityName)
        {
            return GetFieldMetadata(field, entityName).IsCustomAttribute ?? false;
        }

        public QueryExpression ConvertFetchToQueryExpression(string fetchXmlQuery)
        {
            var convertRequest = new FetchXmlToQueryExpressionRequest();
            convertRequest.FetchXml = fetchXmlQuery;
            var convertResponse = (FetchXmlToQueryExpressionResponse)Execute(convertRequest);
            var query = convertResponse.Query;
            return query;
        }

        public string ConvertQueryExpressionToFetch(QueryExpression queryExpression)
        {
            var convertRequest = new QueryExpressionToFetchXmlRequest();
            convertRequest.Query = queryExpression;
            var convertResponse = (QueryExpressionToFetchXmlResponse)Execute(convertRequest);
            var query = convertResponse.FetchXml;
            return query;
        }

        public IEnumerable<Guid> GetMarketingListMemberIds(Guid marketingListId)
        {
            return GetMarketingListMembers(marketingListId, new string[] { }).Select(e => e.Id);
        }

        public IEnumerable<Entity> GetMarketingListMembers(Guid marketingListId, IEnumerable<string> fields)
        {
            var members = new List<Entity>();
            var marketingList = Retrieve("list", marketingListId, new[] { "type", "query", "createdfromcode" });
            var type = marketingList.GetBoolean("type");
            switch (type)
            {
                case XrmPicklists.ListType.Dynamic:
                    {
                        var query = ConvertFetchToQueryExpression(marketingList.GetStringField("query"));
                        query.ColumnSet = CreateColumnSet(fields);
                        members.AddRange(RetrieveAll(query));
                        break;
                    }
                case XrmPicklists.ListType.Static:
                    {
                        members.AddRange(GetAssociatedEntities("contact", "listmember", "listid", marketingListId,
                            "entityid", fields));
                        break;
                    }
            }
            return members;
        }

        public void ExecuteAction(string actionName, EntityReference targetId)
        {
            var request = new OrganizationRequest(actionName);
            request.Parameters.Add("Target", targetId);
            //request["BoolInArgument"] = true;
            //request["DateTimeInArgument"] = DateTime.Now;
            //request["DecimalInArgument"] = decimal.Zero;
            var response = Service.Execute(request);
            //bool boolvalue = (bool)response.Results["BoolOutArgument"];
        }

        public void FulfillSalesOrder(Guid orderId, int status)
        {
            var orderClose = new Entity("orderclose");
            orderClose["salesorderid"] = new EntityReference { LogicalName = "salesorder", Id = orderId };
            orderClose["subject"] = "Order Fulfilled";
            orderClose["description"] = "Order Fulfilled";
            var request = new FulfillSalesOrderRequest
            {
                OrderClose = orderClose,
                Status = new OptionSetValue(status)
            };
            Execute(request);
        }

        public IEnumerable<string> GetAllNnRelationshipEntityNames()
        {
            return GetAllEntityMetadata()
                .Where(m => m.IsIntersect ?? false)
                .Select(e => e.LogicalName)
               .ToArray();
        }

        public virtual ManyToManyRelationshipMetadata GetRelationshipMetadataForEntityName(string relationshipEntityName)
        {
            lock (LockObject)
            {
                if (AllRelationshipMetadata.All(r => r.IntersectEntityName != relationshipEntityName))
                    throw new NullReferenceException(string.Format("Couldn't Find Relationship With Entity Name {0}", (relationshipEntityName)));
                return AllRelationshipMetadata.First(r => r.IntersectEntityName == relationshipEntityName);
            }
        }

        public string GetRelationshipDisplayLabel(string relationshipName, string entityType)
        {
            var relationship = GetRelationshipMetadata(relationshipName);
            var isRecordType2 = relationship.Entity2LogicalName == entityType;
            var menuConfiguration = isRecordType2 ? relationship.Entity2AssociatedMenuConfiguration : relationship.Entity1AssociatedMenuConfiguration;
            if(menuConfiguration.Behavior == AssociatedMenuBehavior.UseLabel)
            {
                var label = isRecordType2 ? relationship.Entity2AssociatedMenuConfiguration.Label : relationship.Entity1AssociatedMenuConfiguration.Label;
                return GetLabelDisplay(label);
            }
            else
            {
                return GetEntityCollectionName(entityType);
            }
        }
    }
}