﻿using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Xrm.Schema;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace JosephM.Xrm
{
    public class XrmService : IOrganizationService
    {
        private static DateTime MinimumDateTime = new DateTime(1753, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        private int _timeoutSeconds = 600;

        public int TimeoutSeconds
        {
            get { return _timeoutSeconds; }
            set
            {
                _timeoutSeconds = value;
                SetServiceTimeout();
            }
        }

        private int _languageCode;
        private int LanguageCode
        {
            get
            {
                if(_languageCode == 0)
                {
                    var userSettings = RetrieveAllAndConditions(Entities.usersettings, new[]
                    {
                        new ConditionExpression(Fields.usersettings_.systemuserid, ConditionOperator.EqualUserId)
                    }, new[] { Fields.usersettings_.uilanguageid });
                    if(userSettings.Any())
                    {
                        _languageCode = userSettings.First().GetInt(Fields.usersettings_.uilanguageid);
                    }
                    if (_languageCode == 0)
                        _languageCode = 1033;
                }
                return _languageCode;
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

        private void SetServiceTimeout()
        {
            if (_service is OrganizationServiceProxy proxy)
            {
                proxy.Timeout = new TimeSpan(0, 0, TimeoutSeconds);
            }
            else if (_service is CrmServiceClient client)
            {
                CrmServiceClient.MaxConnectionTimeout = new TimeSpan(0, 0, TimeoutSeconds);
                if (client.OrganizationServiceProxy != null)
                {
                    client.OrganizationServiceProxy.Timeout = new TimeSpan(0, 0, TimeoutSeconds);
                }
                if (client.OrganizationWebProxyClient != null
                    && client.OrganizationWebProxyClient.InnerChannel != null)
                {
                    client.OrganizationWebProxyClient.InnerChannel.OperationTimeout = new TimeSpan(0, 0, TimeoutSeconds);
                }
            }
        }

        private readonly SortedDictionary<string, SortedDictionary<string, AttributeMetadata>>
            _entityFieldMetadata = new SortedDictionary<string, SortedDictionary<string, AttributeMetadata>>();

        private bool _loadedAllEntities;
        private readonly SortedDictionary<string, EntityMetadata> _entityMetadata = new SortedDictionary<string, EntityMetadata>();

        private readonly SortedDictionary<string, RelationshipMetadataBase[]> _entityRelationships
            = new SortedDictionary<string, RelationshipMetadataBase[]>();

        private readonly Object _lockObject = new Object();

        private readonly List<ManyToManyRelationshipMetadata> _relationshipMetadata = new
            List<ManyToManyRelationshipMetadata>();


        private Dictionary<IntegerFormat, Dictionary<int, string>> _intPicklistCache = new Dictionary<IntegerFormat, Dictionary<int, string>>();

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


        /// <summary>
        ///     DONT USE CALL THE EXECUTE METHOD OR THE PROPERTY
        /// </summary>
        private IOrganizationService _service;

        public XrmService(IXrmConfiguration crmConfig, IOrganizationConnectionFactory serviceFactory)
        {
            XrmConfiguration = crmConfig;
            ServiceFactory = serviceFactory ?? new XrmOrganizationConnectionFactory();
        }

        protected object LockObject
        {
            get { return _lockObject; }
        }

        private SortedDictionary<string, SortedDictionary<string, AttributeMetadata>> EntityFieldMetadata
        {
            get { return _entityFieldMetadata; }
        }

        private SortedDictionary<string, EntityMetadata> EntityMetadata
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

        public IXrmConfiguration XrmConfiguration { get; set; }
        public IOrganizationConnectionFactory ServiceFactory { get; }

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
                        var getConnection = ServiceFactory.GetOrganisationConnection(XrmConfiguration);
                        _service = getConnection.Service;
                        _organisation = getConnection.Organisation;
                        SetServiceTimeout();
                    }
                }
                return _service;
            }
            set
            {
                _service = value;
                _connectionVerified = false;
            }
        }

        private Organisation _organisation;
        private Organisation GetOrganisation()
        {
            lock (_lockObject)
            {
                if (_organisation == null)
                {
                    if (XrmConfiguration == null)
                    {
                        throw new NullReferenceException("Cannot get organisation as the connection is null");
                    }
                    var verifyConnection = VerifyConnection();
                    if (!verifyConnection.IsValid)
                    {
                        throw new Exception(verifyConnection.GetErrorString());
                    }
                    if (_organisation == null)
                    {
                        throw new NullReferenceException("Error loading organisation details. A connection was successfully made but the organisation details were not populated");
                    }
                }
                return _organisation;
            }
        }

        public string OrganisationVersion
        {
            get
            {
                return GetOrganisation().Version;
            }
        }

        public Guid OrganisationId
        {
            get
            {
                return OrganisationEntity.Id;
            }
        }

        public string EnvironmentId
        {
            get
            {
                return GetOrganisation().EnvironmentId;
            }
        }

        public string WebUrl
        {
            get
            {
                return GetOrganisation().WebUrl;
            }
        }

        private Guid? _defaultSolutionId;
        public Guid DefaultSolutionId
        {
            get
            {
                if (!_defaultSolutionId.HasValue)
                {
                    _defaultSolutionId = GetFirst(Entities.solution, Fields.solution_.uniquename, "default")?.Id ?? Guid.Empty;
                }
                return _defaultSolutionId.Value;
            }
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
                    throw new NullReferenceException($"Error getting the {GetFieldLabel(Fields.organization_.basecurrencyid, Entities.organization)} from the {GetEntityLabel(Entities.organization)} record");
                }
                return value.Value;
            }
        }

        public virtual OrganizationResponse Execute(OrganizationRequest request)
        {
            return Execute(request, true);
        }

        public virtual OrganizationResponse Execute(OrganizationRequest request, bool retry)
        {
            OrganizationResponse result;
            try
            {
                result = Service.Execute(request);
            }
            catch (FaultException<OrganizationServiceFault>)
            {
                if (!retry)
                    throw;
                if (request is ImportSolutionRequest)
                    throw;
                lock (_lockObject)
                {
                    //I have seen this error thrown when the sand box server is busy, and subsequent calls are successful. Going to add a retry
                    Thread.Sleep(50);
                    result = Service.Execute(request);
                }
            }
            catch (CommunicationException)
            {
                if (!retry)
                    throw;
                lock (_lockObject)
                {
                    //Error was being thrown after service running overnight with no activity
                    //adding logic to reconnect when this error thrown
                    if (XrmConfiguration != null)
                    {
                        _service = null;
                        result = Service.Execute(request);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                if(_service is CrmServiceClient crmServiceClient)
                {
                    throw new Exception($"Error executing request: {crmServiceClient.LastCrmError}", ex);
                }
                else
                {
                    throw;
                }
            }
            return result;
        }

        public virtual ManyToManyRelationshipMetadata GetRelationshipMetadata(string relationshipName)
        {
            lock (LockObject)
            {
                if (RelationshipMetadata.All(rm => rm.SchemaName != relationshipName))
                {
                    var request = new RetrieveRelationshipRequest
                    {
                        Name = relationshipName
                    };
                    var response = (RetrieveRelationshipResponse)Execute(request);
                    RelationshipMetadata.Add((ManyToManyRelationshipMetadata)response.RelationshipMetadata);
                }
                return RelationshipMetadata.First(rm => rm.SchemaName == relationshipName);
            }
        }

        public virtual ManyToManyRelationshipMetadata GetRelationshipMetadataForEntityName(string relationshipEntityName)
        {
            lock (LockObject)
            {
                if (AllRelationshipMetadata.All(r => r.IntersectEntityName != relationshipEntityName))
                    throw new NullReferenceException(string.Format("Couldn;t Find Relationship With Entity Name {0}", (relationshipEntityName)));
                return AllRelationshipMetadata.First(r => r.IntersectEntityName == relationshipEntityName);
            }
        }

        public virtual EntityMetadata GetEntityMetadata(string entity)
        {
            lock (LockObject)
            {
                if (!EntityMetadata.ContainsKey(entity))
                {
                    var request = new RetrieveEntityRequest
                    {
                        EntityFilters = EntityFilters.Default,
                        LogicalName = entity
                    };
                    var response = (RetrieveEntityResponse)Execute(request);
                    EntityMetadata.Add(entity, response.EntityMetadata);
                }
            }
            return EntityMetadata[entity];
        }

        public virtual AttributeMetadata GetFieldMetadata(string field, string entity)
        {
            var entityFieldMetadata = GetEntityFieldMetadata(entity);

            if (!entityFieldMetadata.ContainsKey(field))
            {
                throw new Exception("Error Getting field metadata\nEntity: " + entity + "\nField: " + field);
            }
            return entityFieldMetadata[field];
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
            var label = GetLabelDisplay(GetFieldMetadata(field, entity).DisplayName);
            return string.IsNullOrWhiteSpace(label) ? field : label;
        }

        public string GetFieldDescription(string field, string entity)
        {
            return GetLabelDisplay(GetFieldMetadata(field, entity).Description);
        }

        public string GetLabelDisplay(Label label)
        {
            return label.UserLocalizedLabel?.Label;
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
            if (options != null)
            {
                foreach (var item in options)
                {
                    var option = item.Value;
                    if (option != null)
                        result.Add(new KeyValuePair<int, string>(option.Value, GetOptionLabel(item)));
                }
            }
            return result;
        }

        private IDictionary<int,string> GetIntPicklistCache(IntegerFormat integerFormat)
        {
            lock(_lockObject)
            {
                if (!_intPicklistCache.ContainsKey(integerFormat))
                {
                    if (integerFormat == IntegerFormat.TimeZone)
                    {
                        var dictionary = new Dictionary<int, string>();
                        foreach (var tz in RetrieveAllEntityType(Entities.timezonedefinition))
                        {
                            if (!dictionary.ContainsKey(tz.GetInt(Fields.timezonedefinition_.timezonecode)))
                                dictionary.Add(tz.GetInt(Fields.timezonedefinition_.timezonecode), tz.GetStringField(Fields.timezonedefinition_.userinterfacename));
                        }
                        dictionary = dictionary
                            .OrderBy(kv => kv.Value == null ? 0 : kv.Value.Contains("GMT-") ? 1 : kv.Value.Contains("GMT+") ? 3 : 2)
                            .ThenByDescending(kv => kv.Value == null ? "" : (kv.Value.Contains("GMT-") && kv.Value.IndexOf(")") > 0) ? kv.Value.Substring(0, kv.Value.IndexOf(")")) :"")
                            .ThenBy(kv => kv.Value == null ? "" : (kv.Value.Contains("GMT+") && kv.Value.IndexOf(")") > 0) ? kv.Value.Substring(0, kv.Value.IndexOf(")")) : "")
                            .ThenBy(kv => kv.Value)

                            .ToDictionary(kv => kv.Key, kv => kv.Value);
                        _intPicklistCache.Add(integerFormat, dictionary);
                    }
                    else if (integerFormat == IntegerFormat.Language)
                    {
                        var dictionary = new Dictionary<int, string>();

                        //early versions dont have this type and were throwing error
                        //so for them lets just return empty list and use as standard integer
                        if (EntityExists(Entities.languagelocale))
                        {
                            var req = new RetrieveAvailableLanguagesRequest();
                            var res = (RetrieveAvailableLanguagesResponse)Execute(req);

                            foreach (var tz in RetrieveAllEntityType(Entities.languagelocale))
                            {
                                var localeId = tz.GetInt(Fields.languagelocale_.localeid);
                                if (res.LocaleIds.Contains(localeId) && !dictionary.ContainsKey(localeId))
                                    dictionary.Add(localeId, tz.GetStringField(Fields.languagelocale_.language));
                            }
                        }

                        dictionary = dictionary
                            .OrderBy(kv => kv.Value)
                            .ToDictionary(kv => kv.Key, kv => kv.Value);
                        _intPicklistCache.Add(integerFormat, dictionary);
                    }
                    else
                        _intPicklistCache.Add(integerFormat, new Dictionary<int, string>());
                }
                return _intPicklistCache[integerFormat];
            }
        }


        public IEnumerable<KeyValuePair<int, string>> GetPicklistKeyValues(string entityType, string fieldName)
        {
            var fieldMetadata = GetFieldMetadata(fieldName, entityType);
            if (fieldMetadata is EnumAttributeMetadata enumFm && enumFm.OptionSet != null)
            {
                return OptionSetToKeyValues(enumFm.OptionSet.Options);
            }
            if (fieldMetadata is IntegerAttributeMetadata intMt)
            {
                return GetIntPicklistCache(intMt.Format ?? IntegerFormat.None);
            }
            if (fieldMetadata is BooleanAttributeMetadata bFm
                && bFm.OptionSet != null
                && bFm.OptionSet.FalseOption != null
                && bFm.OptionSet.TrueOption != null)
            {
                return OptionSetToKeyValues(new[] { bFm.OptionSet.FalseOption, bFm.OptionSet.TrueOption });
            }
            return new KeyValuePair<int, string>[0];
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

        public decimal GetMaxDecimalValue(string field, string entity)
        {
            return ((DecimalAttributeMetadata)GetFieldMetadata(field, entity)).MaxValue ?? decimal.MaxValue;
        }

        public decimal GetMinDecimalValue(string field, string entity)
        {
            return ((DecimalAttributeMetadata)GetFieldMetadata(field, entity)).MinValue ?? decimal.MinValue;
        }

        public double GetMaxMoneyValue(string field, string entity)
        {
            return ((MoneyAttributeMetadata)GetFieldMetadata(field, entity)).MaxValue ?? double.MaxValue;
        }

        public double GetMinMoneyValue(string field, string entity)
        {
            return ((MoneyAttributeMetadata)GetFieldMetadata(field, entity)).MinValue ?? double.MinValue;
        }

        public int GetMaxIntValue(string fieldName, string entityType)
        {
            return ((IntegerAttributeMetadata)GetFieldMetadata(fieldName, entityType)).MaxValue ?? int.MaxValue;
        }

        public int GetMinIntValue(string fieldName, string entityType)
        {
            return ((IntegerAttributeMetadata)GetFieldMetadata(fieldName, entityType)).MinValue ?? int.MinValue;
        }

        public double GetMaxDoubleValue(string field, string entity)
        {
            return ((DoubleAttributeMetadata)GetFieldMetadata(field, entity)).MaxValue ?? double.MaxValue;
        }

        public double GetMinDoubleValue(string field, string entity)
        {
            return ((DoubleAttributeMetadata)GetFieldMetadata(field, entity)).MinValue ?? double.MinValue;
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
                        return option.Value ?? -1;
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(value))
            {
                throw new Exception($"Field {field} in entity {entity} has no matching picklist option for label " + value);
            }
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
            return optionValue.ToString();
        }

        public SortedDictionary<int, string> GetOptions(string field, string entity)
        {
            var result = new SortedDictionary<int, string>();

            var metadata = (PicklistAttributeMetadata)GetFieldMetadata(field, entity);
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
            if (metadata.AttributeType == AttributeTypeCode.Lookup
                || metadata.AttributeType == AttributeTypeCode.Owner
                || metadata.AttributeType == AttributeTypeCode.Customer)
            {
                var targets = ((LookupAttributeMetadata)metadata).Targets;
                result = targets.Any() ? string.Join(",", targets) : null;
            }
            else if (metadata.AttributeType == AttributeTypeCode.PartyList)
            {
                return $"{Entities.contact},{Entities.account},{Entities.queue},{Entities.systemuser}";
            }
            else if (metadata.AttributeType == AttributeTypeCode.Uniqueidentifier)
            {
                var typeMetadata = GetEntityMetadata(entity);
                if(typeMetadata.IsIntersect ?? false)
                {
                    var relationshipMetadata = GetRelationshipMetadataForEntityName(entity);
                    if(relationshipMetadata != null)
                    {
                        if(relationshipMetadata.Entity1IntersectAttribute == field)
                        {
                            result = relationshipMetadata.Entity1LogicalName;
                        }
                        else if (relationshipMetadata.Entity2IntersectAttribute == field)
                        {
                            result = relationshipMetadata.Entity2LogicalName;
                        }
                    }
                }
            }
            return result;
        }

        private static string GetOptionLabel(OptionMetadata option)
        {
            if (option.Label != null && option.Label.UserLocalizedLabel != null && option.Label.UserLocalizedLabel.Label != null)
            {
                return option.Label.UserLocalizedLabel.Label;
            }
            else if (option.Label != null && option.Label.LocalizedLabels != null && option.Label.LocalizedLabels.Any())
            {
                return option.Label.LocalizedLabels[0].Label;
            }
            else
            {
                return option.Value.ToString();
            }
        }

        public string GetRelationshipEntityName(string relationshipName)
        {
            return GetRelationshipMetadata(relationshipName).IntersectEntityName;
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
                case AttributeTypeCode.Money:
                    {
                        return 2;
                    }
            }
            throw new NotImplementedException($"Get precision not implemented for field of type {fieldType}");
        }

        public string GetPrimaryNameField(string targetType)
        {
            return GetEntityMetadata(targetType).PrimaryNameAttribute;
        }

        public string GetPrimaryKeyField(string targetType)
        {
            return GetEntityMetadata(targetType).PrimaryIdAttribute;
        }

        private bool _connectionVerified;
        public IsValidResponse VerifyConnection()
        {
            var response = new IsValidResponse();
            if (!_connectionVerified)
            {
                try
                {
                    Execute(new WhoAmIRequest());
                    _connectionVerified = true;
                }
                catch (Exception ex)
                {
                    response.AddInvalidReason(ex.DisplayString());
                }
            }
            return response;
        }

        public string GetEntityDisplayName(string recordType)
        {
            return GetLabelDisplay(GetEntityMetadata(recordType).DisplayName) ?? recordType;
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

        public static QueryExpression BuildQueryActive(string entityType, string[] fields,
            IEnumerable<ConditionExpression> filters, string[] sortFields)
        {
            var query = new QueryExpression(entityType);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, XrmPicklists.State.Active);
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
                query.ColumnSet = new ColumnSet(fields);
            else
                query.ColumnSet = new ColumnSet(true);

            return query;
        }

        public static QueryExpression BuildQuery(string entityType, IEnumerable<string> fields,
IEnumerable<ConditionExpression> filters, IEnumerable<string> sortFields)
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

        public void ProcessQueryResults(QueryExpression query, Func<IEnumerable<Entity>, bool> processEachResultSet)
        {
            query.PageInfo.PageNumber = 1;
            var response = RetrieveMultiple(query);
            var shallContinue = processEachResultSet(response.Entities);

            //If there is more than one page of records then keep retrieving until we get them all
            if (shallContinue && response.MoreRecords)
            {
                while (response.MoreRecords)
                {
                    query.PageInfo.PagingCookie = response.PagingCookie;
                    query.PageInfo.PageNumber = query.PageInfo.PageNumber + 1;
                    response = RetrieveMultiple(query);
                    if (!processEachResultSet(response.Entities))
                        return;
                }
            }
        }

        public void ClearCache()
        {
            lock (LockObject)
            {
                EntityFieldMetadata.Clear();
                EntityMetadata.Clear();
                RelationshipMetadata.Clear();
                EntityRelationships.Clear();
                AllRelationshipMetadata = null;
                SharedOptionSets = null;
                _loadedAllEntities = false;
                _allFieldsLoaded = false;
                _allRelationshipsLoaded = false;
                _intPicklistCache.Clear();
            }
        }

        public object ParseField(string fieldName, string entityType, object value)
        {
            return ParseField(fieldName, entityType, value, false);
        }

        public object ParseField(string fieldName, string entityType, object value, bool datesAmericanFormat)
        {
            var fieldType = GetFieldType(fieldName, entityType);
            if (!value.IsEmpty())
            {
                switch (fieldType)
                {
                    case AttributeTypeCode.String:
                    case AttributeTypeCode.Memo:
                        {
                            var maxLength = GetMaxLength(fieldName, entityType);
                            var temp = value.ToString();
                            if (temp.Length > maxLength)
                            {
                                throw new ArgumentOutOfRangeException(nameof(value), $"Field {fieldName} exceeds maximum length of {maxLength}. Length is {temp.Length}");
                            }
                            return temp;
                        }
                    case AttributeTypeCode.Integer:
                        {
                            int temp;
                            if (value is int i)
                            {
                                temp = i;
                            }
                            else if (value is string s && string.IsNullOrWhiteSpace(s))
                            {
                                return null;
                            }
                            else
                            {
                                var intAsString = value.ToString();
                                var picklist = GetPicklistKeyValues(entityType, fieldName);
                                if (picklist != null && picklist.Any(kv => kv.Value == intAsString))
                                {
                                    temp = picklist.First(kv => kv.Value == intAsString).Key;
                                }
                                else
                                {
                                    double output = 0;
                                    if (!double.TryParse(intAsString, out output))
                                    {
                                        throw new Exception($"Could not parse int for value {intAsString} in field {fieldName}");
                                    }
                                    temp = Convert.ToInt32(output);
                                }
                            }
                            var min = GetMinIntValue(fieldName, entityType);
                            var max = GetMaxIntValue(fieldName, entityType);
                            if(temp < min || temp > max)
                            {
                                throw new ArgumentOutOfRangeException(nameof(value), $"Field {fieldName} outside permitted range of {min} to {max}. Value is {temp}");
                            }
                            return temp;
                        }
                    case AttributeTypeCode.DateTime:
                        {
                            DateTime? temp = null;
                            if (value is DateTime dt)
                            {
                                temp = dt;
                            }
                            else if (!string.IsNullOrWhiteSpace(value.ToString()))
                            {
                                if (value.ToString().All(c => char.IsDigit(c) || c == '.')
                                    && value.ToString().Count(c => c == '.') <= 1)
                                {
                                    temp = DateTime.FromOADate(double.Parse(value.ToString()));
                                }
                                else
                                {
                                    try
                                    {
                                        temp = DateTime.Parse(value.ToString(), new CultureInfo(datesAmericanFormat ? "en-US" : "en-GB"));
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new ArgumentException(nameof(value), $"Error parsing value {value} to date for field {fieldName}", ex);
                                    }
                                }
                            }
                            //remove the second fractions as crm strips them out
                            if (temp.HasValue)
                            {
                                temp = temp.Value.AddMilliseconds(-1 * temp.Value.Millisecond);
                                if(temp.Value.Kind == DateTimeKind.Local)
                                {
                                    temp = temp.Value.ToUniversalTime();
                                }
                            }
                            if(temp.HasValue && temp.Value < MinimumDateTime)
                            {
                                throw new ArgumentException(nameof(value), $"Date is less than the minumum value supported by CrmDateTime. Actual value: {temp.Value.ToString("d")}, Minimum value supported: {MinimumDateTime.ToString("d")}");
                            }
                            
                            return temp;
                        }
                    case AttributeTypeCode.Lookup:
                    case AttributeTypeCode.Customer:
                    case AttributeTypeCode.Owner:
                        {
                            if (value is EntityReference)
                            {
                                return value;
                            }
                            if (value is Guid g && fieldType == AttributeTypeCode.Lookup)
                            {
                                return new EntityReference(GetLookupTargetEntity(fieldName, entityType), g);
                            }
                            else if (value is string s)
                            {
                                var types = new List<string>();
                                if (fieldType == AttributeTypeCode.Lookup)
                                {
                                    types.Add(GetLookupTargetEntity(fieldName, entityType));
                                }
                                else if (fieldType == AttributeTypeCode.Owner)
                                {
                                    types.AddRange(new[] { "team", "systemuser" });
                                }
                                else if (fieldType == AttributeTypeCode.Customer)
                                {
                                    types.AddRange(new[] { "account", "contact" });
                                }
                                var matchingRecords = new List<EntityReference>();
                                foreach (var type in types)
                                {
                                    Guid tryGetGuid = Guid.Empty;
                                    if (Guid.TryParse(value.ToString(), out tryGetGuid))
                                    {
                                        if (types.Count() == 1)
                                        {
                                            matchingRecords.Add(CreateLookup(type, tryGetGuid));
                                        }
                                        else
                                        {
                                            var match = GetFirst(type, GetPrimaryKeyField(type), tryGetGuid);
                                            if (match != null)
                                            {
                                                matchingRecords.Add(match.ToEntityReference());
                                            }
                                        }
                                    }
                                    else
                                    {
                                        matchingRecords.AddRange(RetrieveAllAndConditions(type,
                                            new[]
                                        {
                                            new ConditionExpression(GetPrimaryNameField(type), ConditionOperator.Equal,
                                                value.ToString())
                                        }, new string[0]).Select(e => e.ToEntityReference()).ToArray());

                                    }
                                }
                                if (matchingRecords.Count() == 1)
                                {
                                    return matchingRecords.First();
                                }
                                throw new ArgumentOutOfRangeException(nameof(value), $"Error parsing field {fieldName}. The value {value} does not match to a unique {types.JoinGrammarOr()} records name");
                            }
                            else
                            {
                                throw new ArgumentOutOfRangeException(nameof(value), $"Error parsing field {fieldName}. Parsing {fieldType} is not implemented for argument type of {value.GetType().Name}");
                            }
                        }
                    case AttributeTypeCode.Picklist:
                    case AttributeTypeCode.Status:
                    case AttributeTypeCode.State:
                        {
                            if (value is OptionSetValue)
                            {
                                return value;
                            }
                            if (value is string s)
                            {
                                return CreateOptionSetValue(GetMatchingOptionValue(s, fieldName, entityType));
                            }
                            else if (value is int i)
                            {
                                return CreateOptionSetValue(i);
                            }
                            else
                            {
                                throw new ArgumentOutOfRangeException(nameof(value), $"Error parsing field {fieldName}. Parsing {fieldType} is not implemented for argument type of {value.GetType().Name}");
                            }
                        }
                    case AttributeTypeCode.Decimal:
                        {
                            decimal temp = 0;
                            if (value is decimal d)
                            {
                                temp = d;
                            }
                            else
                            {
                                temp = decimal.Parse(value.ToString().Replace(",", ""));
                            }
                            temp = decimal.Round(temp, GetPrecision(fieldName, entityType));
                            var min = GetMinDecimalValue(fieldName, entityType);
                            var max = GetMaxDecimalValue(fieldName, entityType);
                            if (temp < min || temp > max)
                            {
                                throw new ArgumentOutOfRangeException(nameof(value), $"Field {fieldName} outside permitted range of {min} to {max}. Value is {temp}");
                            }
                            return temp;
                        }
                    case AttributeTypeCode.Double:
                        {
                            double temp;
                            if (value is double d)
                                temp = d;
                            else
                            {
                                temp = double.Parse(value.ToString().Replace(",", ""));
                            }
                            var min = GetMinDoubleValue(fieldName, entityType);
                            var max = GetMaxDoubleValue(fieldName, entityType);
                            if (temp < min || temp > max)
                            {
                                throw new ArgumentOutOfRangeException(nameof(value), $"Field {fieldName} outside permitted range of {min} to {max}. Value is {temp}");
                            }
                            return temp;
                        }
                    case AttributeTypeCode.Money:
                        {
                            Money temp;
                            if (value is Money m)
                            {
                                temp = m;
                            }
                            else if (value is decimal d)
                            {
                                temp = new Money(d);
                            }
                            else
                            {
                                var valueString = value.ToString().Replace(",", "").Trim(new[] { '$', '£', '€' });
                                temp = new Money(decimal.Parse(valueString));
                            }
                            var min = GetMinMoneyValue(fieldName, entityType);
                            var max = GetMaxMoneyValue(fieldName, entityType);
                            var tempAmount = temp.Value;
                            if ((double)tempAmount < min || (double)tempAmount > max)
                            {
                                throw new ArgumentOutOfRangeException(nameof(value), $"Field {fieldName} outside permitted range of {min} to {max}. Value is {temp}");
                            }
                            return temp;
                        }
                    case AttributeTypeCode.Boolean:
                        {
                            if (value is bool)
                                return value;
                            else if (value is string s)
                            {
                                var picklist = GetPicklistKeyValues(entityType, fieldName);
                                var trueLabel = picklist.Any(p => p.Key == 1)
                                    ? picklist.First(p => p.Key == 1).Value
                                    : null;
                                var falseLabel = picklist.Any(p => p.Key == 0)
                                    ? picklist.First(p => p.Key == 0).Value
                                    : null;
                                var valueToLower = value.ToString().ToLower();
                                if (new string[] { trueLabel?.ToLower(), "1", true.ToString().ToLower() }.Contains(valueToLower))
                                {
                                    return true;
                                }
                                if (new string[] { falseLabel?.ToLower(), "0", false.ToString().ToLower() }.Contains(valueToLower))
                                {
                                    return false;
                                }
                                throw new ArgumentOutOfRangeException(nameof(value), $"Could not parse matching boolean for string value of '{value}'");
                            }
                            else
                            {
                                throw new ArgumentOutOfRangeException(nameof(value), $"Error parsing field {fieldName}. Parsing {fieldType} is not implemented for argument type of {value.GetType().Name}");
                            }
                        }
                    case AttributeTypeCode.Uniqueidentifier:
                        {
                            if (value is Guid)
                            {
                                return value;
                            }
                            if (value is string s)
                            {
                                try
                                {
                                    return Guid.Parse(value.ToString());
                                }
                                catch(Exception ex)
                                {
                                    throw new ArgumentOutOfRangeException($"Error parsing value {value} to Guid for field {fieldName}", ex);
                                }
                            }
                            else
                            {
                                throw new ArgumentOutOfRangeException(nameof(value), $"Error parsing field {fieldName}. Parsing {fieldType} is not implemented for argument type of {value.GetType().Name}");
                            }
                        }
                    case AttributeTypeCode.PartyList:
                        {
                            if (value is string)
                            {
                                return value.ToString().Split(';')
                                    .Select(s => new EntityReference() { Name = s.Trim() })
                                    .Select(XrmEntity.CreatePartyEntity)
                                    .ToArray();
                            }
                            if (value is IEnumerable<EntityReference> ienumEr)
                            {
                                return ienumEr.Select(XrmEntity.CreatePartyEntity).ToArray();
                            }
                            if (value is IEnumerable<Entity> || value is EntityCollection)
                            {
                                return value;
                            }
                            else
                            {
                                throw new ArgumentOutOfRangeException(nameof(value), $"Error parsing field {fieldName}. Parsing {fieldType} is not implemented for argument type of {value.GetType().Name}");
                            }
                        }
                    case AttributeTypeCode.EntityName:
                        {
                            if (value is Core.FieldType.RecordType rt)
                            {
                                value = rt.Key;
                            }
                            if (value is string)
                            {
                                return value;
                            }
                            else
                            {
                                throw new ArgumentOutOfRangeException(nameof(value), $"Error parsing field {fieldName}. Parsing {fieldType} is not implemented for argument type of {value.GetType().Name}");
                            }
                        }
                    case AttributeTypeCode.Virtual:
                        {
                            var fieldMetadata = GetFieldMetadata(fieldName, entityType);
                            if(value is OptionSetValueCollection)
                            {
                                return value;
                            }
                            if (fieldMetadata is MultiSelectPicklistAttributeMetadata ms)
                            {
                                if (value is string stringOptions && !string.IsNullOrWhiteSpace(stringOptions))
                                {
                                    var splitStringOptions = stringOptions.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                    var optionValueCollection = new OptionSetValueCollection();
                                    foreach(var item in splitStringOptions)
                                    {
                                        optionValueCollection.Add(new OptionSetValue(GetMatchingOptionValue(item.Trim(), fieldName, entityType)));
                                    }
                                    return optionValueCollection;
                                }
                            }
                            break;
                        }
                }
                return value;
            }
            else
                return null;
        }

        public string GetWebUrl(string recordType, Guid id, string additionalparams = null, Entity entity = null)
        {
            if (recordType == null)
                return null;
            string result = null;
            switch(recordType)
            {
                case Entities.pluginassembly:
                case Entities.plugintype:
                case Entities.sdkmessageprocessingstep:
                case Entities.sdkmessageprocessingstepimage:
                case Entities.organization:
                    {
                        return null;
                    }
                case Entities.solution:
                    {
                        result = $"{WebUrl}/tools/solution/edit.aspx?id={id}";
                        break;
                    }
                case Entities.fieldsecurityprofile:
                    {
                        result = $"{WebUrl}/biz/fieldsecurityprofiles/edit.aspx?id={id}";
                        break;
                    }
                case Entities.workflow:
                    {
                        var workflow = entity ?? Retrieve(Entities.workflow, id, new[] { Fields.workflow_.category });
                        var workflowCategory = workflow.GetOptionSetValue(Fields.workflow_.category);
                        switch (workflowCategory)
                        {
                            case OptionSets.Process.Category.BusinessProcessFlow:
                                {
                                    result = $"{WebUrl}/Tools/ProcessControl/bpfConfigurator.aspx?id={id}";
                                    break;
                                }
                            case OptionSets.Process.Category.BusinessRule:
                                {
                                    result = $"{WebUrl}/tools/systemcustomization/businessrules/businessRulesDesigner.aspx?BRlaunchpoint=BRGrid&id={id}";
                                    break;
                                }
                            default:
                                {
                                    result = $"{WebUrl}/sfa/workflow/edit.aspx?id={id}";
                                    break;
                                }
                        }
                        break;
                    }
                case Entities.webresource:
                    {
                        result = $"{WebUrl}/main.aspx?etn={recordType}&id={id}&pagetype=webresourceedit";
                        break;
                    }
                case Entities.systemform:
                    {
                        var systemForm = entity ?? Retrieve(Entities.systemform, id, new[] { Fields.systemform_.type });
                        if (systemForm.GetOptionSetValue(Fields.systemform_.type) == OptionSets.SystemForm.FormType.Dashboard)
                        {
                            result = $"{WebUrl}/main.aspx?extraqs=%26formId%3d%7b{id}%7d%26dashboardType%3d1030&pagetype=dashboardeditor";
                        }
                        else
                        {
                            result = $"{WebUrl}/main.aspx?etn={recordType}&extraqs=formtype%3dmain%26formId%3d{id}%26action%3d-1&pagetype=formeditor";
                        }
                        break;
                    }
                case Entities.savedquery:
                    {
                        result = $"{WebUrl}/tools/vieweditor/viewManager.aspx?id={id}";
                        break;
                    }
                case Entities.role:
                    {
                        result = $"{WebUrl}/biz/roles/edit.aspx?id={id}";
                        break;
                    }
                case Entities.report:
                    {
                        result = $"{WebUrl}/CRMReports/reportproperty.aspx?id=%7b{id}%7d"; 
                        break;
                    }
                case Entities.appmodule:
                    {
                        result = $"{WebUrl}/designer/app/fd140aaf-4df4-11dd-bd17-0019b9312238/{id}#/AppDesignerCanvas/{id}";
                        break;
                    }
                case "entity":
                    {
                        result = $"{WebUrl}/tools/solution/edit.aspx?id={DefaultSolutionId}";
                        break;
                    }
                case "field":
                    {
                        result = $"{WebUrl}/tools/systemcustomization/attributes/manageAttribute.aspx?attributeId={id}";
                        break;
                    }
                case "manytomanyrelationship":
                    {
                        result = $"{WebUrl}/tools/systemcustomization/relationships/manageRelationship.aspx?entityRelationshipId={id}&entityRole=many";
                        break;
                    }
                case "sharedoptionset":
                    {
                        result = $"{WebUrl}/tools/systemcustomization/optionset/optionset.aspx?id={id}";
                        break;
                    }
            }
            if (result == null)
            {
                result = $"{WebUrl}/main.aspx?etn={recordType}&id={id}&pagetype=entityrecord";
            }
            if (result != null && additionalparams != null)
            {
                result = result + "&" + additionalparams;
            }
            return result;
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

        public Entity CreateAndRetreive(Entity entity, string[] fields)
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

        public Entity GetFirst(string entityType, string fieldName, object fieldValue)
        {
            return GetFirst(entityType, fieldName, fieldValue, null);
        }

        public Entity GetFirst(string entityType, string fieldName, object fieldValue, string[] fields)
        {
            var query = new QueryExpression(entityType);
            var condition = fieldValue == null
                ? new ConditionExpression(fieldName, ConditionOperator.Null)
                : new ConditionExpression(fieldName, ConditionOperator.Equal, fieldValue);
            query.Criteria.AddCondition(condition);
            if (fields != null)
                query.ColumnSet = new ColumnSet(fields);
            else
                query.ColumnSet = new ColumnSet(true);
            return RetrieveFirst(query);
        }

        public Entity RetrieveFirst(QueryExpression query)
        {
            var r = RetrieveFirstX(query, 1);
            return !r.Any() ? null : r.ElementAt(0);
        }

        public IEnumerable<Entity> GetAssociatedEntities(string entityTo, string relationshipName,
            string entityFromRelationShipId, Guid entityFromId,
            string toRelationShipId, string[] fields)
        {
            var query = new QueryExpression(entityTo);
            query.ColumnSet = CreateColumnSet(fields);
            var link = query.AddLink(GetRelationshipEntityName(relationshipName), GetPrimaryKeyField(entityTo),
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
            var query = new QueryExpression(GetRelationshipEntityName(relationshipName));
            query.ColumnSet = CreateColumnSet(new[] { otherSideId });
            query.Criteria.AddCondition(thisSideId, ConditionOperator.Equal, thisId);
            return RetrieveAll(query).Select(entity => (Guid)entity.GetField(otherSideId));
        }

        public IEnumerable<Guid> GetAssociatedIds(string relationshipName, string thisSideId, Guid thisId,
            string otherSideId)
        {
            var query = new QueryExpression(GetRelationshipEntityName(relationshipName));
            query.ColumnSet = CreateColumnSet(new[] { otherSideId });
            query.Criteria.AddCondition(thisSideId, ConditionOperator.Equal, thisId);
            return RetrieveAll(query).Select(entity => (Guid)entity.GetField(otherSideId));
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
                var thisId = (Guid)item.GetField(thisSideId);
                if (result.ContainsKey(thisId))
                    ((List<Guid>)result[thisId]).Add((Guid)item.GetField(otherSideId));
            }
            return result;
        }

        /// <summary>
        ///     Warning! Unlimited results
        /// </summary>
        public IDictionary<Guid, List<Entity>> IndexAssociatedEntities(string relationshipEntityName, string thisTypeId, string otherSideId, string otherType)
        {
            var result = new SortedDictionary<Guid, List<Entity>>();

            var query = BuildQuery(otherType, null, null, null);
            var relationshipLink = query.AddLink(relationshipEntityName, otherSideId, otherSideId);
            relationshipLink.Columns = new ColumnSet(thisTypeId);
            relationshipLink.EntityAlias = "R";
            var allAssociated = RetrieveAll(query);

            foreach (var item in allAssociated)
            {
                var associatedFromId = (Guid)item.GetFieldValue("R." + thisTypeId);
                if (!result.ContainsKey(associatedFromId))
                {
                    result.Add(associatedFromId, new List<Entity>());
                }
                result[associatedFromId].Add(item);
            }
            return result;
        }

        public string GetFieldAsMatchString(string entityType, string fieldName, object fieldValue)
        {
            if (fieldValue == null)
                return "";
            else if (IsRealNumber(fieldName, entityType))
            {
                var format = "#." + (new string('#', GetPrecision(fieldName, entityType)));
                return (decimal.Parse(fieldValue.ToString())).ToString(format);
            }
            else if (IsLookup(fieldName, entityType))
            {
                return ((EntityReference)fieldValue).Id.ToString();
            }
            else if (IsMoney(fieldName, entityType))
            {
                return ((Money)fieldValue).Value.ToString("###################0.00");
            }
            else if (IsOptionSet(fieldName, entityType))
            {
                if (fieldValue is OptionSetValue osv)
                {
                    return osv.Value.ToString();
                }
                else
                {
                    return GetMatchingOptionValue(fieldValue.ToString(), fieldName, entityType).ToString();
                }
            }
            else
                return fieldValue.ToString()?.ToLower();
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

        public void SetState(Entity entity, int state, int status)
        {
            SetState(entity.LogicalName, entity.Id, state, status);
        }

        public void SetState(string type, Guid id, int state, int status)
        {
            var setStateReq = new SetStateRequest
            {
                EntityMoniker = new EntityReference(type, id),
                State = new OptionSetValue(state),
                Status = new OptionSetValue(status)
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
            var link = query.AddLink(relationshipName, relatedEntityId, intersectId2);
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
            entity.SetField(fieldName, value);
            Update(entity);
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
            return Retrieve(entity.LogicalName, entity.Id, fieldsToGet);
        }

        public IEnumerable<Entity> RetrieveAllFetch(string fetchXmlQuery)
        {
            return RetrieveMultiple(new FetchExpression(fetchXmlQuery)).Entities.ToArray();
        }

        internal void SetFieldIfChanging(string recordType, Guid id, string fieldName, object fieldValue)
        {
            var record = Retrieve(recordType, id, new[] { fieldName });
            var currentValue = record.GetField(fieldName);
            if (!XrmEntity.FieldsEqual(currentValue, fieldValue))
            {
                record.SetField(fieldName, fieldValue);
                Update(record);
            }
        }

        public IEnumerable<Entity> Retrieve(string entityType, IEnumerable<Guid> ids, IEnumerable<string> fields)
        {
            var query = new QueryExpression(entityType);
            query.Criteria.AddCondition(XrmEntity.GetPrimaryKeyName(entityType), ConditionOperator.In, ids.Cast<object>().ToArray());
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

        public IEnumerable<Entity> RetrieveAllAndConditions(string entityName, IEnumerable<ConditionExpression> conditions)
        {
            return RetrieveAllAndConditions(entityName, conditions, null);
        }

        public IEnumerable<Entity> RetrieveAllAndConditions(string entityName, IEnumerable<ConditionExpression> conditions,
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
                //there was a bug in SDK when querying on queues
                //which required adding our or filter as a child filter
                //rather than adding in the root filter
                var whatTheFilter = new FilterExpression(LogicalOperator.Or);
                while (tempFilters.Any() && i < 200)
                {
                    var filter = tempFilters.ElementAt(0);
                    tempFilters.RemoveAt(0);
                    whatTheFilter.AddFilter(filter);
                    i++;
                }
                query.Criteria.AddFilter(whatTheFilter);
                foreach (var entity in RetrieveAll(query))
                {
                    if (!results.ContainsKey(entity.Id))
                        results.Add(entity.Id, entity);
                }
            }
            return results.Values;
        }

        public int GetCurrencyPrecision(Guid? currencyGuid)
        {
            return GetCurrency(currencyGuid ?? BaseCurrencyId).GetInt(Fields.transactioncurrency_.currencyprecision);
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

        public void Assign(Entity entity, Guid userId)
        {
            var request = new AssignRequest
            {
                Assignee = CreateLookup("systemuser", userId),
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
            Delete(entityName, id, false);
        }

        public void Delete(string entityName, Guid id, bool bypassWorkflowsAndPlugins)
        {
            var request = new DeleteRequest
            {
                Target = CreateLookup(entityName, id)
            };
            if (bypassWorkflowsAndPlugins)
            {
                request.Parameters.Add("SuppressCallbackRegistrationExpanderJob", true);
                request.Parameters.Add("BypassBusinessLogicExecution", "CustomSync,CustomAsync");
            }
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

        public IEnumerable<Entity> RetrieveMultiple(string recordType, IEnumerable<Guid> ids, IEnumerable<string> fields)
        {
            var responses = ExecuteMultiple(ids
                .Select(id => new RetrieveRequest()
                    {
                        Target = new EntityReference(recordType, id),
                        ColumnSet = CreateColumnSet(fields)
                    }).ToArray());

            foreach(var item in responses.Cast<ExecuteMultipleResponseItem>())
            {
                if (item.Fault != null)
                    throw new FaultException<OrganizationServiceFault>(item.Fault, item.Fault.Message);

                yield return ((RetrieveResponse)item.Response).Entity;
            }

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

        public void Update(Entity entity, bool bypassWorkflowsAndPlugins)
        {
            var request = new UpdateRequest
            {
                Target = entity
            };
            if(bypassWorkflowsAndPlugins)
            {
                request.Parameters.Add("SuppressCallbackRegistrationExpanderJob", true);
                request.Parameters.Add("BypassBusinessLogicExecution", "CustomSync,CustomAsync");
            }
            Execute(request);
        }

        public void Associate(string relationshipName, string keyAttributeFrom, Guid entityFrom, string keyAttributeTo,
            IEnumerable<Guid> relatedEntities)
        {
            var metadata = GetRelationshipMetadata(relationshipName);
            var isReferencing = metadata.Entity1IntersectAttribute == keyAttributeFrom;
            var relatedType = isReferencing ? metadata.Entity2LogicalName : metadata.Entity1LogicalName;
            var targetType = isReferencing ? metadata.Entity1LogicalName : metadata.Entity2LogicalName;

            var relationship = new Relationship(relationshipName)
            {
                PrimaryEntityRole =
                    isReferencing ? EntityRole.Referencing : EntityRole.Referenced
            };

            var entityReferenceCollection = new EntityReferenceCollection();
            foreach (var id in relatedEntities)
                entityReferenceCollection.Add(CreateLookup(relatedType, id));

            Associate(targetType, entityFrom, relationship, entityReferenceCollection);
        }

        public void Associate(string relationshipName, string entityType1, Guid id1, bool Is1Referencing,
            string entityType2, Guid id2)
        {
            var rMetadata = GetRelationshipMetadata(relationshipName);
            string thisSideId;
            if ((entityType1 != rMetadata.Entity2LogicalName) || Is1Referencing)
                thisSideId = rMetadata.Entity1IntersectAttribute;
            else
                thisSideId = rMetadata.Entity2IntersectAttribute;
            var otherSideId = thisSideId == rMetadata.Entity1IntersectAttribute
                ? rMetadata.Entity2IntersectAttribute
                : rMetadata.Entity1IntersectAttribute;

            Associate(relationshipName, thisSideId, id1, otherSideId, new[] { id2 });
        }

        public void Associate(string relationshipName, string keyAttributeFrom, Guid entityFrom, string keyAttributeTo,
            Guid relatedEntity)
        {
            Associate(relationshipName, keyAttributeFrom, entityFrom, keyAttributeTo, new[] { relatedEntity });
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
            return Retrieve(entityType, id, new[] { fieldName }).GetField(fieldName);
        }

        /// <summary>
        ///     !! Doesn;t work same entity type!!
        /// </summary>
        public void Disassociate(string relationshipName, string keyAttributeFrom, Guid entityFrom,
            string keyAttributeTo, IEnumerable<Guid> relatedEntities)
        {
            var metadata = GetRelationshipMetadata(relationshipName);
            var isReferencing = metadata.Entity1IntersectAttribute == keyAttributeFrom;
            var relatedType = isReferencing ? metadata.Entity2LogicalName : metadata.Entity1LogicalName;
            var targetType = isReferencing ? metadata.Entity1LogicalName : metadata.Entity2LogicalName;

            var relationship = new Relationship(relationshipName)
            {
                PrimaryEntityRole =
                    isReferencing ? EntityRole.Referencing : EntityRole.Referenced
            };

            var entityReferenceCollection = new EntityReferenceCollection();
            foreach (var id in relatedEntities)
                entityReferenceCollection.Add(CreateLookup(relatedType, id));

            Disassociate(targetType, entityFrom, relationship, entityReferenceCollection);
        }

        public void Disassociate(string relationshipName, string entityType1, Guid id1, bool Is1Referencing,
            string entityType2, Guid id2)
        {
            var rMetadata = GetRelationshipMetadata(relationshipName);
            string thisSideId;
            if ((entityType1 != rMetadata.Entity2LogicalName) || Is1Referencing)
                thisSideId = rMetadata.Entity1IntersectAttribute;
            else
                thisSideId = rMetadata.Entity2IntersectAttribute;
            var otherSideId = thisSideId == rMetadata.Entity1IntersectAttribute
                ? rMetadata.Entity2IntersectAttribute
                : rMetadata.Entity1IntersectAttribute;

            Disassociate(relationshipName, thisSideId, id1, otherSideId, new[] { id2 });
        }

        /// <summary>
        ///     !! Doesn;t work same entity type!!
        /// </summary>
        public void Disassociate(string relationshipName, string keyAttributeFrom, Guid entityFrom,
            string keyAttributeTo, Guid relatedEntity)
        {
            Disassociate(relationshipName, keyAttributeFrom, entityFrom, keyAttributeTo, new[] { relatedEntity });
        }

        public void Update(Entity entity, IEnumerable<string> fieldsToSubmit, bool bypassWorkflowsAndPlugins = false)
        {
            if (fieldsToSubmit != null && fieldsToSubmit.Any())
            {
                var submissionEntity = ReplicateWithFields(entity, fieldsToSubmit);
                Update(submissionEntity, bypassWorkflowsAndPlugins: bypassWorkflowsAndPlugins);
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
                        submissionEntity.SetField(field, entity.GetField(field));
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
                    var request = new RetrieveEntityRequest
                    {
                        EntityFilters = EntityFilters.Relationships,
                        LogicalName = entity
                    };
                    var response = (RetrieveEntityResponse)Execute(request);
                    EntityRelationships.Add(entity,
                        response.EntityMetadata.OneToManyRelationships
                            .Cast<RelationshipMetadataBase>()
                            .Union(response.EntityMetadata.ManyToManyRelationships)
                            .Union(response.EntityMetadata.ManyToOneRelationships)
                            .ToArray());
                }
            }
            return EntityRelationships[entity];
        }

        public IEnumerable<OneToManyRelationshipMetadata> GetEntityManyToOneRelationships(string entity)
        {
            return
                GetEntityRelationships(entity)
                    .Where(r => r is OneToManyRelationshipMetadata)
                    .Cast<OneToManyRelationshipMetadata>()
                    .Where(r => r.ReferencingEntity == entity)
                    .ToArray();
        }

        public IEnumerable<OneToManyRelationshipMetadata> GetEntityOneToManyRelationships(string entity)
        {
            return
                GetEntityRelationships(entity)
                    .Where(r => r is OneToManyRelationshipMetadata)
                    .Cast<OneToManyRelationshipMetadata>()
                    .Where(r => r.ReferencedEntity == entity)
                    .ToArray();
        }

        public IEnumerable<ManyToManyRelationshipMetadata> GetEntityManyToManyRelationships(string entity)
        {
            return
                GetEntityRelationships(entity)
                    .Where(r => r is ManyToManyRelationshipMetadata)
                    .Cast<ManyToManyRelationshipMetadata>()
                    .ToArray();
        }

        public void SetFieldMetadataCache(string entity, string field, AttributeMetadata attributeMetadata)
        {
            lock (LockObject)
            {
                var entityFieldMetadata = GetEntityFieldMetadata(entity);
                if(entityFieldMetadata.ContainsKey(field))
                {
                    entityFieldMetadata.Remove(field);
                }
                entityFieldMetadata.Add(field, attributeMetadata);
            }
        }

        public SortedDictionary<string, AttributeMetadata> GetEntityFieldMetadata(string entity)
        {
            lock (LockObject)
            {
                if (!EntityFieldMetadata.ContainsKey(entity))
                {
                    // Create the request
                    var request = new RetrieveEntityRequest
                    {
                        EntityFilters = EntityFilters.Attributes,
                        LogicalName = entity
                    };
                    var response = (RetrieveEntityResponse)Execute(request);
                    var attributeMetadata = response.EntityMetadata.Attributes;
                    AttributeMetadata[] fieldMetadata = FilterAttributeMetadata(attributeMetadata);
                    var dictionary = new SortedDictionary<string, AttributeMetadata>();
                    if (fieldMetadata != null)
                    {
                        foreach (var field in fieldMetadata)
                        {
                            if (!dictionary.ContainsKey(field.LogicalName))
                            {
                                dictionary.Add(field.LogicalName, field);
                            }
                        }
                    }
                    EntityFieldMetadata.Add(entity, dictionary);
                }
            }
            return EntityFieldMetadata[entity];
        }

        private static AttributeMetadata[] FilterAttributeMetadata(AttributeMetadata[] attributeMetadata)
        {
            var results = new List<AttributeMetadata>();
            foreach(var attribute in attributeMetadata)
            {
                if (attribute is ImageAttributeMetadata || attribute is FileAttributeMetadata)
                {
                    results.Add(attribute);
                }
                else if (!(attribute.IsValidForRead ?? true))
                {
                    continue;
                }
                else if (attribute is StringAttributeMetadata smt && !string.IsNullOrWhiteSpace(smt.YomiOf))
                {
                    continue;
                }
                else if (!string.IsNullOrWhiteSpace(attribute.AttributeOf) && !(attribute is ImageAttributeMetadata || attribute is FileAttributeMetadata))
                {
                    continue;
                }
                else
                {
                    results.Add(attribute);
                }
            }
            return results.ToArray();
        }

        #endregion

        public Entity GetFirst(string recordType)
        {
            return RetrieveFirst(CreateQuery(recordType, null));
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
            return x > 0
                ? RetrieveFirstX(query, x)
                : RetrieveAll(query);
        }

        public IEnumerable<Entity> RetrieveFirstX(QueryExpression query, int x)
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

        /// <summary>
        ///     DONT USE USE THE PROPERTY
        /// </summary>
        private List<ManyToManyRelationshipMetadata> _allRelationshipMetadata;

        /// <summary>
        ///     DONT USE USE THE PROPERTY
        /// </summary>
        private List<OptionSetMetadata> _sharedOptionSets;

        public IDictionary<string, EntityMetadata> GetAllEntityMetadata()
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
                    foreach(var item in response.EntityMetadata)
                    {
                        if(item.LogicalName != null && !_entityMetadata.ContainsKey(item.LogicalName))
                        {
                            _entityMetadata.Add(item.LogicalName, item);
                        }
                    }
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
            set
            {
                lock (LockObject)
                {
                    _sharedOptionSets = value;
                }
            }
        }

        public List<ManyToManyRelationshipMetadata> AllRelationshipMetadata
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
            set
            {
                lock (LockObject)
                {
                    _allRelationshipMetadata = value;
                }
            }
        }
        //7.0.1.144

        public bool SupportsSetStateUpdate
        {
            get
            {
                return VersionHelper.IsNewerVersion(OrganisationVersion, "7.0.1.145");
            }
        }

        public bool SupportsExecuteMultiple
        {
            get
            {
                return VersionHelper.IsNewerVersion(OrganisationVersion, "5.0.9690.3235");
            }
        }

        public bool SupportsExecuteAsynch
        {
            get
            {
                return VersionHelper.IsNewerVersion(OrganisationVersion, "8.9.9.9");
            }
        }

        /// <summary>
        ///     DOESN'T UPDATE PRIMARY FIELD - CALL THE CREATEORUPDATESTRING METHOD
        /// </summary>
        public void CreateOrUpdateEntity(string schemaName, string displayName, string displayCollectionName,
            string description, bool audit, string primaryFieldSchemaName,
            string primaryFieldDisplayName, string primaryFieldDescription,
            int primaryFieldMaxLength, bool primaryFieldIsMandatory, bool primaryFieldAudit, bool isActivityType, bool notes, bool activities, bool connections, bool mailMerge, bool queues)
        {
            lock (LockObject)
            {
                var metadata = new EntityMetadata();

                var exists = EntityExists(schemaName);
                if (exists)
                    metadata = GetEntityMetadata(schemaName);
                metadata.SchemaName = schemaName;
                metadata.LogicalName = schemaName;
                metadata.DisplayName = new Label(displayName, LanguageCode);
                metadata.DisplayCollectionName = new Label(displayCollectionName, LanguageCode);
                metadata.IsAuditEnabled = new BooleanManagedProperty(audit);
                metadata.IsActivity = isActivityType;
                metadata.IsValidForQueue = new BooleanManagedProperty(queues);
                metadata.IsMailMergeEnabled = new BooleanManagedProperty(mailMerge);
                metadata.IsConnectionsEnabled = new BooleanManagedProperty(connections);
                metadata.IsActivity = isActivityType;
                if (!String.IsNullOrWhiteSpace(description))
                    metadata.Description = new Label(description, LanguageCode);
                else
                    metadata.Description = new Label(displayCollectionName, LanguageCode);

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
                }
            }
        }

        public bool EntityExists(string schemaName)
        {
            return GetAllEntityMetadata().ContainsKey(schemaName);
        }

        private AttributeRequiredLevelManagedProperty GetRequiredLevel(bool isRequired)
        {
            return isRequired
                ? new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.ApplicationRequired)
                : new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None);
        }

        public void Publish(string xml = null)
        {
            if (string.IsNullOrWhiteSpace(xml))
                Execute(new PublishAllXmlRequest());
            else
            {
                var req = new PublishXmlRequest();
                req.ParameterXml = xml;
                Execute(req);
            }
        }

        public void CreateOrUpdateBooleanAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable, string recordType)
        {
            var optionSet = new BooleanOptionSetMetadata();
            optionSet.FalseOption = new OptionMetadata(new Label("No", LanguageCode), 0);
            optionSet.TrueOption = new OptionMetadata(new Label("Yes", LanguageCode), 1);

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
            if (metadata.SchemaName.IsNullOrWhiteSpace())
                metadata.SchemaName = schemaName;
            metadata.DisplayName = new Label(displayName, LanguageCode);
            metadata.LogicalName = schemaName;
            if (!string.IsNullOrWhiteSpace(description))
                metadata.Description = new Label(description, LanguageCode);
            if (metadata.RequiredLevel == null
                || (metadata.RequiredLevel.CanBeChanged
                    && (isRequired && metadata.RequiredLevel.Value != AttributeRequiredLevel.ApplicationRequired
                        || !isRequired && metadata.RequiredLevel.Value == AttributeRequiredLevel.ApplicationRequired
                )))
                metadata.RequiredLevel = GetRequiredLevel(isRequired);
            if (metadata.IsAuditEnabled == null || metadata.IsAuditEnabled.CanBeChanged)
                metadata.IsAuditEnabled = new BooleanManagedProperty(audit);
            metadata.IsValidForAdvancedFind = new BooleanManagedProperty(searchable);
        }

        public void CreateOrUpdateAttribute(string schemaName, string recordType, AttributeMetadata metadata)
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
                }
                else
                {
                    var request = new CreateAttributeRequest
                    {
                        EntityName = recordType,
                        Attribute = metadata
                    };
                    Execute(request);
                }
            }
        }

        public bool FieldExists(string fieldName, string recordType)
        {
            return GetEntityFieldMetadata(recordType).Keys.Any(k => k == fieldName);
        }

        public void CreateOrUpdateDateAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable, string recordType,
            string dateTimeBehaviour, bool includeTime)
        {
            DateTimeAttributeMetadata metadata;
            if (FieldExists(schemaName, recordType))
                metadata = (DateTimeAttributeMetadata)GetFieldMetadata(schemaName, recordType);
            else
                metadata = new DateTimeAttributeMetadata();

            SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);

            metadata.DateTimeBehavior = dateTimeBehaviour;
            metadata.Format = includeTime ? DateTimeFormat.DateAndTime : DateTimeFormat.DateOnly;

            CreateOrUpdateAttribute(schemaName, recordType, metadata);
        }

        public void CreateOrUpdateDecimalAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable, string recordType,
            decimal minimum, decimal maximum, int decimalPrecision)
        {
            DecimalAttributeMetadata metadata;
            if (FieldExists(schemaName, recordType))
                metadata = (DecimalAttributeMetadata)GetFieldMetadata(schemaName, recordType);
            else
                metadata = new DecimalAttributeMetadata();

            SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);

            metadata.MinValue = minimum;
            metadata.MaxValue = maximum;
            if (decimalPrecision >= 0)
                metadata.Precision = decimalPrecision;

            CreateOrUpdateAttribute(schemaName, recordType, metadata);
        }

        public void CreateOrUpdateDoubleAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable, string recordType,
            double minimum, double maximum, int decimalPrecision)
        {
            DoubleAttributeMetadata metadata;
            if (FieldExists(schemaName, recordType))
                metadata = (DoubleAttributeMetadata)GetFieldMetadata(schemaName, recordType);
            else
                metadata = new DoubleAttributeMetadata();

            SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);

            metadata.MinValue = minimum;
            metadata.MaxValue = maximum;
            if (decimalPrecision >= 0)
                metadata.Precision = decimalPrecision;

            CreateOrUpdateAttribute(schemaName, recordType, metadata);
        }

        public void CreateOrUpdateStateAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable, string recordType)
        {
            StateAttributeMetadata metadata;
            if (FieldExists(schemaName, recordType))
                metadata = (StateAttributeMetadata)GetFieldMetadata(schemaName, recordType);
            else
                metadata = new StateAttributeMetadata();

            SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);

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
            string recordType, int minimum, int maximum, IntegerFormat integerFormat)
        {
            IntegerAttributeMetadata metadata;
            if (FieldExists(schemaName, recordType))
                metadata = (IntegerAttributeMetadata)GetFieldMetadata(schemaName, recordType);
            else
                metadata = new IntegerAttributeMetadata();

            SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);

            metadata.MinValue = minimum;
            metadata.MaxValue = maximum;

            metadata.Format = integerFormat;

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

                var referencedRecordTypes = referencedEntityType
                    .Split(',')
                    .Select(s => s?.Trim()?.ToLower())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray();

                var indexOf_ = schemaName.IndexOf("_");
                if (indexOf_ == -1)
                    throw new Exception("Could not determine prefix of field for new relationship name");
                var prefix = indexOf_ == -1
                    ? "new"
                    : schemaName.Substring(0, indexOf_ + 1);
                var usePrefix = !recordType.StartsWith(prefix);
                Func<string, OneToManyRelationshipMetadata> createRelationship = (reffedType) =>
                {
                    return new OneToManyRelationshipMetadata
                    {
                        SchemaName = string.Format("{0}{1}_{2}_{3}", usePrefix ? prefix : "", recordType, reffedType, schemaName),
                        AssociatedMenuConfiguration = new AssociatedMenuConfiguration
                        {
                            Behavior = displayInRelated ? AssociatedMenuBehavior.UseCollectionName : AssociatedMenuBehavior.DoNotDisplay
                        },
                        ReferencingEntity = recordType,
                        ReferencedEntity = reffedType
                    };
                };

                if (FieldExists(schemaName, recordType))
                {
                    CreateOrUpdateAttribute(schemaName, recordType, metadata);
                    var relationships = GetRelationshipsFor(schemaName, recordType);
                    foreach (var relationship in relationships)
                    {
                        if (relationship.IsCustomizable != null && relationship.IsCustomizable.Value)
                        {
                            var newBehaviour = displayInRelated
                                ? AssociatedMenuBehavior.UseCollectionName
                                : AssociatedMenuBehavior.DoNotDisplay;
                            if (newBehaviour != relationship.AssociatedMenuConfiguration.Behavior)
                            {
                                relationship.AssociatedMenuConfiguration.Behavior = displayInRelated
                                    ? AssociatedMenuBehavior.UseCollectionName
                                    : AssociatedMenuBehavior.DoNotDisplay;
                                var request = new UpdateRelationshipRequest()
                                {
                                    Relationship = relationship
                                };
                                Execute(request);
                            }
                        }
                    }
                    foreach(var referencedRecordType in referencedRecordTypes)
                    {
                        if(!relationships.Any(r => r.ReferencedEntity == referencedRecordType))
                        {
                            var newPolymorphicRelationship = createRelationship(referencedRecordType);
                            var request = new CreateOneToManyRequest
                            {
                                 OneToManyRelationship = newPolymorphicRelationship,
                                 Lookup = metadata
                            };
                            Execute(request);
                        }
                    }
                }
                else
                {
                    if (referencedRecordTypes.Count() == 1)
                    {
                        var request = new CreateOneToManyRequest
                        {
                            OneToManyRelationship = createRelationship(referencedEntityType),
                            Lookup = metadata
                        };
                        Execute(request);
                    }
                    else
                    {
                        var createPolymorphicRequest = new OrganizationRequest("CreatePolymorphicLookupAttribute");
                        createPolymorphicRequest["Lookup"] = metadata;
                        createPolymorphicRequest["OneToManyRelationships"] = referencedRecordTypes.Select(s => createRelationship(s)).ToArray();
                        Execute(createPolymorphicRequest);
                    }
                }
            }
        }

        private IEnumerable<OneToManyRelationshipMetadata> GetRelationshipsFor(string fieldName, string entityType)
        {
            return GetEntityManyToOneRelationships(entityType)
                .Where(r => r.ReferencingAttribute.ToLower() == fieldName)
                .ToArray();
        }

        /// <summary>
        ///     DOESN'T UPDATE THE PICKLIST ITSELF - CALL THE UPDATEPICKLISTOPTIONS OR CREATEORUPDATESHAREDOPTIONSET METHOD
        /// </summary>
        public void CreateOrUpdatePicklistAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable,
            string recordType, string sharedOptionSetName, bool isMultiSelect)
        {
            lock (LockObject)
            {
                EnumAttributeMetadata metadata;
                var exists = FieldExists(schemaName, recordType);
                if (exists)
                    metadata = (EnumAttributeMetadata)GetFieldMetadata(schemaName, recordType);
                else
                {
                    if (isMultiSelect)
                        metadata = new MultiSelectPicklistAttributeMetadata();
                    else
                        metadata = new PicklistAttributeMetadata();
                }

                SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);

                metadata.OptionSet = new OptionSetMetadata { Name = sharedOptionSetName, IsGlobal = true };

                CreateOrUpdateAttribute(schemaName, recordType, metadata);
            }
        }

        public void CreateOrUpdatePicklistAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable,
            string recordType, IEnumerable<KeyValuePair<int, string>> options, bool isMultiSelect)
        {
            lock (LockObject)
            {
                var optionSet = new OptionSetMetadata
                {
                    OptionSetType = OptionSetType.Picklist,
                    IsGlobal = false
                };
                optionSet.Options.AddRange(options.Select(o => new OptionMetadata(new Label(o.Value, LanguageCode), o.Key)));

                EnumAttributeMetadata metadata;
                var exists = FieldExists(schemaName, recordType);
                if (exists)
                    metadata = (EnumAttributeMetadata)GetFieldMetadata(schemaName, recordType);
                else
                {
                    if (isMultiSelect)
                        metadata = new MultiSelectPicklistAttributeMetadata();
                    else
                        metadata = new PicklistAttributeMetadata();
                }

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
                    }
                    else if (optionSet.Any(o => o.Key == option.Key && o.Value != option.Value))
                    {
                        var newValue = optionSet.Single(o => o.Key == option.Key);
                        var request = new UpdateOptionValueRequest
                        {
                            AttributeLogicalName = fieldName,
                            EntityLogicalName = recordType,
                            Value = option.Key,
                            Label = new Label(newValue.Value, LanguageCode)
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
                            AttributeLogicalName = fieldName,
                            EntityLogicalName = recordType,
                            Value = option.Key,
                            Label = new Label(option.Value, LanguageCode)
                        };
                        Execute(request);
                    }
                }
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
        }

        public void CreateOrUpdateCustomerAttribute(string schemaName, string displayName, string description,
            bool isRequired, bool audit, bool searchable, string recordType)
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
            }
            else
            {
                throw new NotSupportedException("Creation of Customer type fields is not supported by this application. You will need to create it manually in the web ui");
                //this code for creating a Customer type field requires SDK V ~8.2
                //however breaks this app for older CRM versions by introducing 
                //the HasFeedback property for Entity creation/update requests

                //var request = new CreateCustomerRelationshipsRequest()
                //{
                //    Lookup = metadata,
                //    OneToManyRelationships = new []
                //    {
                //        new OneToManyRelationshipMetadata
                //        {
                //            SchemaName = string.Format("{0}_{1}_{2}", recordType, "account", schemaName),
                //            ReferencingEntity = recordType,
                //            ReferencedEntity = "account"
                //        },
                //        new OneToManyRelationshipMetadata
                //        {
                //            SchemaName = string.Format("{0}_{1}_{2}", recordType, "contact", schemaName),
                //            ReferencingEntity = recordType,
                //            ReferencedEntity = "contact"
                //        },
                //    }
                //};

                //Execute(request);
                //RefreshFieldMetadata(schemaName, recordType);
                //metadata = (LookupAttributeMetadata)GetFieldMetadata(schemaName, recordType);
                //SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);
                //CreateOrUpdateAttribute(schemaName, recordType, metadata);
                //RefreshFieldMetadata(schemaName, recordType);
                //RefreshEntityMetadata(recordType);
                //RefreshEntityMetadata("account");
                //RefreshEntityMetadata("contact");
            }
        }

        public void CreateOrUpdateAttribute(string schemaName, string displayName, string description,
    bool isRequired, bool audit, bool searchable,
    string recordType)
        {
            AttributeMetadata metadata;
            if (FieldExists(schemaName, recordType))
                metadata = (AttributeMetadata)GetFieldMetadata(schemaName, recordType);
            else
                metadata = new AttributeMetadata();
            SetCommon(metadata, schemaName, displayName, description, isRequired, audit, searchable);
            CreateOrUpdateAttribute(schemaName, recordType, metadata);
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
                if (GetAllEntityMetadata().ContainsKey(schemaName))
                    GetAllEntityMetadata().Remove(schemaName);
                if (_entityFieldMetadata.ContainsKey(schemaName))
                    _entityFieldMetadata.Remove(schemaName);
            }
        }

        public void CreateOrUpdateRelationship(string schemaName, string entityType1, string entityType2, bool entityType1DisplayRelated, bool entityType2DisplayRelated, bool entityType1UseCustomLabel, bool entityType2UseCustomLabel, string entityType1CustomLabel, string entityType2CustomLabel, int entityType1DisplayOrder, int entityType2DisplayOrder)
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
                metadata.Entity1AssociatedMenuConfiguration = SetMenuConfiguration(metadata.Entity1AssociatedMenuConfiguration, entityType1DisplayRelated, entityType1UseCustomLabel, entityType1CustomLabel, entityType1DisplayOrder);
                metadata.Entity2AssociatedMenuConfiguration = SetMenuConfiguration(metadata.Entity2AssociatedMenuConfiguration, entityType2DisplayRelated, entityType2UseCustomLabel, entityType2CustomLabel, entityType2DisplayOrder);
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
            }
        }

        private AssociatedMenuConfiguration SetMenuConfiguration(AssociatedMenuConfiguration associatedMenuConfiguration, bool displayRelated, bool useCustomLabel, string customLabel, int displayOrder)
        {
            if (associatedMenuConfiguration == null)
                associatedMenuConfiguration = new AssociatedMenuConfiguration();
            if (!displayRelated)
                associatedMenuConfiguration.Behavior = AssociatedMenuBehavior.DoNotDisplay;
            else if (useCustomLabel)
                associatedMenuConfiguration.Behavior = AssociatedMenuBehavior.UseLabel;
            else
                associatedMenuConfiguration.Behavior = AssociatedMenuBehavior.UseCollectionName;
            if (associatedMenuConfiguration.Behavior == AssociatedMenuBehavior.UseLabel)
                associatedMenuConfiguration.Label = new Label(customLabel, LanguageCode);
            if (associatedMenuConfiguration.Behavior != AssociatedMenuBehavior.DoNotDisplay)
                associatedMenuConfiguration.Order = displayOrder;
            return associatedMenuConfiguration;
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
            var matches = AllRelationshipMetadata.Where(r => r.SchemaName == relationshipName).ToArray();
            foreach (var match in matches)
            {
                AllRelationshipMetadata.Remove(match);
            }

            var remove = RelationshipMetadata.Where(r => r.SchemaName == relationshipName).ToArray();
            foreach (var item in remove)
                RelationshipMetadata.Remove(item);

            foreach (var item in EntityRelationships.ToArray())
            {
                if (item.Value.Any(r => r.SchemaName == relationshipName))
                    EntityRelationships.Remove(item.Key);
            }
        }

        public void DeleteField(string fieldName, string entityName)
        {
            var request = new DeleteAttributeRequest
            {
                EntityLogicalName = entityName,
                LogicalName = fieldName
            };
            Execute(request);
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
                optionSetMetadata.DisplayName = new Label(displayName, LanguageCode);
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
                                Label = new Label(newValue.Value, LanguageCode)
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
                                Label = new Label(option.Value, LanguageCode)
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
                optionSetMetadata.DisplayName = new Label(displayName, LanguageCode);
                optionSetMetadata.IsGlobal = true;
                optionSetMetadata.Options.AddRange(
                    options.Select(o => new OptionMetadata(new Label(o.Value, LanguageCode), o.Key)).ToList());

                var request = new CreateOptionSetRequest { OptionSet = optionSetMetadata };
                Execute(request);
            }
        }

        private OptionSetMetadata GetSharedOptionSet(string schemaName)
        {
            if (SharedOptionSets.All(s => s.Name != schemaName))
                throw new ArgumentException("Error getting option set: " + schemaName);
            return SharedOptionSets.First(s => s.Name == schemaName);
        }

        public IEnumerable<string> GetSharedOptionSetNames()
        {
            return SharedOptionSets.Select(o => o.Name).ToArray();
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
                .Values
                .Where(m => !(m.IsIntersect ?? false))
                .Select(e => e.LogicalName)
               .ToArray();
        }

        public IEnumerable<string> GetAllNnRelationshipEntityNames()
        {
            return GetAllEntityMetadata()
                .Values
                .Where(m => m.IsIntersect ?? false)
                .Select(e => e.LogicalName)
               .ToArray();
        }

        public IEnumerable<string> GetAllSharedOptionSets()
        {
            return SharedOptionSets.Select(e => e.Name).ToArray();
        }

        public IEnumerable<ExecuteMultipleResponseItem> UpdateMultiple(IEnumerable<Entity> entities,
            IEnumerable<string> fields, bool bypassWorkflowsAndPlugins = false)
        {
            var responses = ExecuteMultiple(entities
                .Select(e => fields == null ? e : ReplicateWithFields(e, fields))
                .Select(e =>
                {
                    var request = new UpdateRequest() { Target = e };
                    if (bypassWorkflowsAndPlugins)
                    {
                        request.Parameters.Add("SuppressCallbackRegistrationExpanderJob", true);
                        request.Parameters.Add("BypassBusinessLogicExecution", "CustomSync,CustomAsync");
                    }
                    return request;
                }));

            return responses.ToArray();
        }

        public IEnumerable<ExecuteMultipleResponseItem> CreateMultiple(IEnumerable<Entity> entities, bool bypassWorkflowsAndPlugins = false)
        {
            var response = ExecuteMultiple(entities
                .Where(e => e != null)
                .Select(e =>
                {
                    var request = new CreateRequest() { Target = e };
                    if (bypassWorkflowsAndPlugins)
                    {
                        request.Parameters.Add("SuppressCallbackRegistrationExpanderJob", true);
                        request.Parameters.Add("BypassBusinessLogicExecution", "CustomSync,CustomAsync");
                    }
                    return request;
                }));

            return response.ToArray();
        }

        public IEnumerable<ExecuteMultipleResponseItem> DeleteMultiple(IEnumerable<Entity> entities, bool bypassWorkflowsAndPlugins = false)
        {
            var response =
                ExecuteMultiple(
                    entities
                    .Where(e => e != null)
                    .Select(e =>
                    {
                        var request = new DeleteRequest() { Target = new EntityReference(e.LogicalName, e.Id) };
                        if (bypassWorkflowsAndPlugins)
                        {
                            request.Parameters.Add("SuppressCallbackRegistrationExpanderJob", true);
                            request.Parameters.Add("BypassBusinessLogicExecution", "CustomSync,CustomAsync");
                        }
                        return request;
                    }));
            return response.ToArray();
        }

        public IEnumerable<ExecuteMultipleResponseItem> ExecuteMultiple(IEnumerable<OrganizationRequest> requests)
        {
            var responses = new List<ExecuteMultipleResponseItem>();
            if (requests.Any())
            {
                if (SupportsExecuteMultiple)
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
                else
                {
                    foreach(var request in requests)
                    {
                        try
                        {
                            responses.Add(new ExecuteMultipleResponseItem() { Response = Execute(request) });
                        }
                        catch(FaultException<OrganizationServiceFault> ex)
                        {
                            responses.Add(new ExecuteMultipleResponseItem() { Fault = ex.Detail });
                        }
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

        public string GetFieldAsDisplayString(string recordType, string fieldName, object value, XrmLocalisationService localisationService, Guid? currencyId = null)
        {
            if (value == null)
            {
                return string.Empty;
            }
            else if (value is string stringValue)
            {
                return stringValue;
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
                if (GetDateFormat(fieldName, recordType) == DateTimeFormat.DateAndTime)
                {
                    return localisationService.ToDateTimeDisplayString(dt);
                }
                return localisationService.ToDateDisplayString(dt);
            }
            else if (value is int integerValue)
            {
                return integerValue.ToString("n0", localisationService.NumberFormatInfo);
            }
            else if (value is Money moneyValue)
            {
                var moneyNumberInfo = (NumberFormatInfo)localisationService.NumberFormatInfo.Clone();
                var currencyEntity = GetCurrency(currencyId ?? BaseCurrencyId);
                moneyNumberInfo.CurrencySymbol = GetCurrency(currencyId ?? BaseCurrencyId).GetStringField(Fields.transactioncurrency_.currencysymbol);
                moneyNumberInfo.CurrencyDecimalDigits = GetCurrencyPrecision(currencyEntity.Id);
                return moneyValue.Value.ToString("c", moneyNumberInfo);
            }
            else if (value is decimal decimalValue)
            {
                if (IsMoney(fieldName, recordType))
                {
                    var moneyNumberInfo = (NumberFormatInfo)localisationService.NumberFormatInfo.Clone();
                    var currencyEntity = GetCurrency(currencyId ?? BaseCurrencyId);
                    moneyNumberInfo.CurrencySymbol = GetCurrency(currencyId ?? BaseCurrencyId).GetStringField(Fields.transactioncurrency_.currencysymbol);
                    moneyNumberInfo.CurrencyDecimalDigits = GetCurrencyPrecision(currencyEntity.Id);
                    return decimalValue.ToString("c", moneyNumberInfo);
                }
                else
                {
                    return decimalValue.ToString($"n{GetPrecision(fieldName, recordType)}", localisationService.NumberFormatInfo);
                }
            }
            else if (value is double doubleValue)
            {
                return doubleValue.ToString($"n{GetPrecision(fieldName, recordType)}", localisationService.NumberFormatInfo);
            }
            else if (IsActivityParty(fieldName, recordType))
            {
                if (value is IEnumerable<Entity> entityEnumerableValue)
                {
                    var namesToOutput = new List<string>();
                    foreach (var party in entityEnumerableValue)
                    {
                        var displayIt = party.GetLookupName(Fields.activityparty_.partyid);
                        displayIt = displayIt ?? party.GetStringField(Fields.activityparty_.addressused);
                        namesToOutput.Add(displayIt);
                    }
                    return string.Join("; ", namesToOutput.Where(f => !f.IsNullOrWhiteSpace()));
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
            else if (value is OptionSetValueCollection osvCollection)
            {
                return string.Join(", ", osvCollection.Select(osv => GetOptionLabel(osv.Value, fieldName, recordType)));

            }
            else if (value is byte[] byteValue)
            {
                return "File";
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

        public bool AssociateSafe(string relationshipName, string entityFrom, string keyAttributeFrom, Guid entityFromId,
            string entityTo, string keyAttributeTo,
            IEnumerable<Guid> entitiesTo)
        {
            var associatedItems = GetAssociatedIds(relationshipName, keyAttributeFrom, entityFromId, keyAttributeTo);
            var newItems = entitiesTo.Where(id => !associatedItems.Contains(id));
            if (newItems.Any())
            {
                Associate(relationshipName, keyAttributeFrom, entityFromId, keyAttributeTo, newItems);
                return true;
            }
            else
                return false;
        }

        public int GetObjectTypeCode(string recordType)
        {
            var entity = GetEntityMetadata(recordType);
            if (!entity.ObjectTypeCode.HasValue)
                throw new NullReferenceException("ObjectTypeCode Is Null");
            return entity.ObjectTypeCode.Value;
        }

        public OneToManyRelationshipMetadata GetManyToOneRelationship(string referencingRecordType, string relationshipName)
        {
            var relationships = GetEntityManyToOneRelationships(referencingRecordType);
            if (relationships.Any(r => r.SchemaName == relationshipName))
            {
                return relationships.First(r => r.SchemaName == relationshipName);
            }
            throw new ArgumentOutOfRangeException("relationshipName",
                "No Relationship Exists With The name: " + relationshipName);
        }

        public OneToManyRelationshipMetadata GetOneToManyRelationship(string referencedRecordType, string relationshipName)
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

        public ManyToManyRelationshipMetadata GetManyToManyRelationship(string referencedRecordType, string relationshipName)
        {
            var relationships = GetEntityManyToManyRelationships(referencedRecordType);
            if (!relationships.Any(r => r.SchemaName == relationshipName))
                throw new ArgumentOutOfRangeException("relationshipName",
                    "No Relationship Exists With The name: " + relationshipName);


            var relationship = relationships.First(r => r.SchemaName == relationshipName);
            return relationship;
        }

        public bool IsActivityParty(string field, string recordType)
        {
            return GetFieldType(field, recordType) == AttributeTypeCode.PartyList;
        }

        public IEnumerable<Entity> GetLinkedRecordsThroughBridge(string linkedRecordType, string recordTypeThrough, string recordTypeFrom, string linkedThroughLookupFrom, string linkedThroughLookupTo, Guid recordFromId)
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

        public bool IsCreateable(string fieldName, string recordType)
        {
            return GetFieldMetadata(fieldName, recordType).IsValidForCreate ?? false;
        }

        public IEnumerable<string> GetFields(string recordType)
        {
            return GetEntityFieldMetadata(recordType).Keys.ToArray();
        }

        public bool IsReadable(string fieldName, string recordType)
        {
            return GetFieldMetadata(fieldName, recordType).IsValidForRead ?? false;
        }

        public string GetSharedPicklistDisplayName(string field, string recordType)
        {
            var name = ((PicklistAttributeMetadata)GetFieldMetadata(field, recordType)).OptionSet.Name;
            return GetLabelDisplay(GetSharedOptionSet(name).DisplayName);
        }

        public string GetSharedPicklistDisplayName(string optionSetName)
        {
            var set = GetSharedOptionSet(optionSetName);
            return set.DisplayName == null ? null : GetLabelDisplay(set.DisplayName);
        }

        public OptionSetMetadata GetSharedPicklist(string optionSetName)
        {
            return GetSharedOptionSet(optionSetName);
        }

        public bool HasNotes(string recordType)
        {
            return GetEntityOneToManyRelationships(recordType).Any(
                r => r.ReferencingEntity == "annotation" && r.ReferencingAttribute == "objectid" && r.ReferencedEntity == recordType);
        }

        public bool HasActivities(string recordType)
        {
            return GetEntityOneToManyRelationships(recordType).Any(
                r => r.ReferencingEntity == "activitypointer" && r.ReferencingAttribute == "regardingobjectid" && r.ReferencedEntity == recordType);
        }

        public bool HasConnections(string recordType)
        {
            var mt = GetEntityMetadata(recordType);
            return mt.IsConnectionsEnabled != null && mt.IsConnectionsEnabled.Value;
        }

        public bool HasMailMerge(string recordType)
        {
            var mt = GetEntityMetadata(recordType);
            return mt.IsMailMergeEnabled != null && mt.IsMailMergeEnabled.Value;
        }

        public bool HasQueues(string recordType)
        {
            var mt = GetEntityMetadata(recordType);
            return mt.IsValidForQueue != null && mt.IsValidForQueue.Value;
        }

        public bool AutoAddToQueue(string recordType)
        {
            var mt = GetEntityMetadata(recordType);
            return mt.AutoRouteToOwnerQueue != null && mt.AutoRouteToOwnerQueue.Value;
        }

        public string GetDescription(string recordType)
        {

            var mt = GetEntityMetadata(recordType);
            return mt.Description != null ? GetLabelDisplay(mt.Description) : null;
        }

        public QueryExpression ConvertFetchToQueryExpression(string fetchXml)
        {
            var req = new FetchXmlToQueryExpressionRequest()
            {
                FetchXml = fetchXml
            };
            var response = (FetchXmlToQueryExpressionResponse)Execute(req);
            return response.Query;
        }

        public string ConvertQueryExpressionToFetch(QueryExpression query)
        {
            var req = new QueryExpressionToFetchXmlRequest()
            {
                Query = query
            };
            var response = (QueryExpressionToFetchXmlResponse)Execute(req);
            return response.FetchXml;
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

        public void PopulateReferenceNames(IEnumerable<EntityReference> references)
        {
            if (references == null)
                return;
            var toDictionary = references
                .Where(r => string.IsNullOrWhiteSpace(r.Name))
                .Where(r => r != null && string.IsNullOrWhiteSpace(r.Name))
                .GroupBy(r => r.LogicalName, r => r)
                .ToDictionary(g => g.Key, g => g.ToArray());

            foreach (var type in toDictionary.Keys)
            {
                try
                {
                    var typePrimaryKey = GetPrimaryKeyField(type);
                    var typePrimaryField = GetPrimaryNameField(type);
                    if (!typePrimaryField.IsNullOrWhiteSpace() && !typePrimaryKey.IsNullOrWhiteSpace())
                    {
                        var distinctIds =
                            toDictionary[type].Select(l => l.Id).Distinct();
                        var conditions =
                            distinctIds.Select(id => new ConditionExpression(typePrimaryKey, ConditionOperator.Equal, id));
                        if (conditions.Any())
                        {
                            var theseRecords = RetrieveAllOrClauses(type, conditions, new[] { typePrimaryField }).ToArray();
                            foreach (var lookup in toDictionary[type])
                            {
                                if (theseRecords.Any(r => r.Id == lookup.Id))
                                    lookup.Name = theseRecords.First(r => r.Id == lookup.Id)
                                        .GetStringField(typePrimaryField);
                            }
                        }
                    }
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {
                }
            }
        }

        public void LoadFieldsForEntities(IEnumerable<string> entityType, LogController logController)
        {
            logController = logController ?? new LogController();
            lock (LockObject)
            {
                var allEntityTypes = entityType
                    .Where(e => !EntityFieldMetadata.ContainsKey(e))
                    .ToList();
                if (allEntityTypes.Any())
                {
                    var totalToDo = allEntityTypes.Count();
                    while (true)
                    {
                        logController.UpdateProgress(totalToDo - allEntityTypes.Count, totalToDo, $"Loading field metadata. Please wait this may take a while\n\nEntities completed: {totalToDo - allEntityTypes.Count}/{totalToDo}");
                        if (!allEntityTypes.Any())
                        {
                            break;
                        }
                        var topX = allEntityTypes.Take(50).ToArray();
                        allEntityTypes.RemoveRange(0, topX.Count());
                        var requests = topX
                            .Select(e => new RetrieveEntityRequest
                            {
                                EntityFilters = EntityFilters.Attributes,
                                LogicalName = e
                            })
                            .ToArray();
                        var responses = ExecuteMultiple(requests);
                        foreach (var response in responses)
                        {
                            if (response.Fault == null && response.Response is RetrieveEntityResponse entityMetadataResponse)
                            {
                                if (entityMetadataResponse.EntityMetadata != null
                                    && entityMetadataResponse.EntityMetadata.LogicalName != null
                                    && !EntityFieldMetadata.ContainsKey(entityMetadataResponse.EntityMetadata.LogicalName)
                                    && entityMetadataResponse.EntityMetadata.Attributes != null)
                                {
                                    var dictionary = new SortedDictionary<string, AttributeMetadata>();
                                    var filteredFields = FilterAttributeMetadata(entityMetadataResponse.EntityMetadata.Attributes);
                                    foreach (var field in filteredFields)
                                    {
                                        if (!dictionary.ContainsKey(field.LogicalName))
                                        {
                                            dictionary.Add(field.LogicalName, field);
                                        }
                                    }
                                    EntityFieldMetadata.Add(entityMetadataResponse.EntityMetadata.LogicalName, dictionary);
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool _allFieldsLoaded;
        public void LoadFieldsForAllEntities(LogController logController)
        {
            logController = logController ?? new LogController();
            lock (LockObject)
            {
                if (!_allFieldsLoaded)
                {
                    var allEntityTypes = GetAllEntityMetadata()
                        .Select(e => e.Value.LogicalName)
                        .Where(e => !EntityFieldMetadata.ContainsKey(e))
                        .ToList();
                    if (allEntityTypes.Any())
                    {
                        var totalToDo = allEntityTypes.Count();
                        while (true)
                        {
                            logController.UpdateProgress(totalToDo - allEntityTypes.Count, totalToDo, $"Loading field metadata. Please wait this may take a while\n\nEntities completed: {totalToDo - allEntityTypes.Count}/{totalToDo}");
                            if (!allEntityTypes.Any())
                            {
                                break;
                            }
                            var topX = allEntityTypes.Take(200).ToArray();
                            allEntityTypes.RemoveRange(0, topX.Count());
                            var requests = topX
                                .Select(e => new RetrieveEntityRequest
                                {
                                    EntityFilters = EntityFilters.Attributes,
                                    LogicalName = e
                                })
                                .ToArray();
                            var responses = ExecuteMultiple(requests);
                            foreach (var response in responses)
                            {
                                if (response.Fault == null && response.Response is RetrieveEntityResponse entityMetadataResponse)
                                {
                                    if (entityMetadataResponse.EntityMetadata != null
                                        && entityMetadataResponse.EntityMetadata.LogicalName != null
                                        && !EntityFieldMetadata.ContainsKey(entityMetadataResponse.EntityMetadata.LogicalName)
                                        && entityMetadataResponse.EntityMetadata.Attributes != null)
                                    {
                                        var dictionary = new SortedDictionary<string, AttributeMetadata>();
                                        var filteredFields = FilterAttributeMetadata(entityMetadataResponse.EntityMetadata.Attributes);
                                        foreach (var field in filteredFields)
                                        {
                                            if (!dictionary.ContainsKey(field.LogicalName))
                                            {
                                                dictionary.Add(field.LogicalName, field);
                                            }
                                        }
                                        EntityFieldMetadata.Add(entityMetadataResponse.EntityMetadata.LogicalName, dictionary);
                                    }
                                }
                            }
                        }
                        _allFieldsLoaded = true;
                    }
                }
            }
        }

        private bool _allRelationshipsLoaded;
        public void LoadRelationshipsForAllEntities(LogController logController)
        {
            logController = logController ?? new LogController();
            lock (LockObject)
            {
                if (!_allRelationshipsLoaded)
                {
                    EntityRelationships.Clear();
                    var allEntityTypes = GetAllEntityMetadata()
                        .Select(e => e.Value.LogicalName)
                        .ToList();
                    var totalToDo = allEntityTypes.Count();
                    while (true)
                    {
                        logController.UpdateProgress(totalToDo - allEntityTypes.Count, totalToDo, $"Loading relationship metadata. Please wait this may take a while\n\nEntities completed: {totalToDo - allEntityTypes.Count}/{totalToDo}");
                        if (!allEntityTypes.Any())
                        {
                            break;
                        }
                        var topX = allEntityTypes.Take(200).ToArray();
                        allEntityTypes.RemoveRange(0, topX.Count());
                        var requests = topX
                            .Select(e => new RetrieveEntityRequest
                            {
                                EntityFilters = EntityFilters.Relationships,
                                LogicalName = e
                            })
                            .ToArray();
                        var responses = ExecuteMultiple(requests);
                        foreach (var response in responses)
                        {
                            if (response.Fault == null && response.Response is RetrieveEntityResponse entityMetadataResponse)
                            {
                                var item = entityMetadataResponse.EntityMetadata;
                                if (item != null
                                    && item.LogicalName != null
                                    && !EntityRelationships.ContainsKey(item.LogicalName))
                                {
                                    var relationships = new List<RelationshipMetadataBase>();
                                    if (item.OneToManyRelationships != null)
                                    {
                                        relationships.AddRange(item.OneToManyRelationships);
                                    }
                                    if (item.ManyToManyRelationships != null)
                                    {
                                        relationships.AddRange(item.ManyToManyRelationships);
                                    }
                                    if (item.ManyToOneRelationships != null)
                                    {
                                        relationships.AddRange(item.ManyToOneRelationships);
                                    }
                                    if (!EntityRelationships.ContainsKey(item.LogicalName))
                                    {
                                        EntityRelationships.Add(item.LogicalName, relationships.ToArray());
                                    }
                                }
                            }
                        }
                    }
                    _allRelationshipsLoaded = true;
                }
            }
        }

        private int fileChunkSize = 4095000;
        public string GetFileFieldBase64(string entityType, Guid id, string fieldName)
        {
            string fileName;
            return GetFileFieldBase64(entityType, id, fieldName, out fileName);
        }

        public string GetFileFieldBase64(string entityType, Guid id, string fieldName, out string fileName)
        {
            string fileString = null;
            fileName = null;
            var byteArray = (byte[])LookupField(entityType, id, fieldName);
            if (byteArray != null && byteArray.Any())
            {
                var fieldMetadata = GetFieldMetadata(fieldName, entityType);
                if (fieldMetadata is ImageAttributeMetadata imageMetadata && !(imageMetadata.CanStoreFullImage ?? false))
                {
                    fileString = Convert.ToBase64String(byteArray);
                }
                else
                {
                    var initialiseRequest = new InitializeFileBlocksDownloadRequest()
                    {
                        FileAttributeName = fieldName,
                        Target = new EntityReference(entityType, id)
                    };
                    var initialiseResponse = (InitializeFileBlocksDownloadResponse)Execute(initialiseRequest);
                    fileName = initialiseResponse.FileName;
                    if (initialiseResponse.FileSizeInBytes > 0)
                    {
                        fileString = string.Empty;
                        for (var offset = 0; offset < initialiseResponse.FileSizeInBytes; offset += fileChunkSize)
                        {
                            var downloadBlockRequest = new DownloadBlockRequest()
                            {
                                BlockLength = (offset + fileChunkSize) > initialiseResponse.FileSizeInBytes ? initialiseResponse.FileSizeInBytes % fileChunkSize : fileChunkSize,
                                FileContinuationToken = initialiseResponse.FileContinuationToken,
                                Offset = offset
                            };
                            var downloadBlockResponse = (DownloadBlockResponse)Execute(downloadBlockRequest);
                            fileString += Convert.ToBase64String(downloadBlockResponse.Data);
                        }
                    }
                }
            }
            return fileString;
        }

        public void SetFileFieldBase64(string entityType, Guid id, string fieldName, string fileName, string base64String)
        {
            var fieldMetadata = GetFieldMetadata(fieldName, entityType);
            if (fieldMetadata is ImageAttributeMetadata imageMetadata && !(imageMetadata.CanStoreFullImage ?? false))
            {
                SetField(entityType, id, fieldName, Convert.FromBase64String(base64String));
            }
            else
            {
                var initialiseRequest = new InitializeFileBlocksUploadRequest()
                {
                    FileAttributeName = fieldName,
                    Target = new EntityReference(entityType, id),
                    FileName = fileName
                };
                var initialiseResponse = (InitializeFileBlocksUploadResponse)Execute(initialiseRequest);
                var base64ChunkSize = fileChunkSize / 8;
                var fileSizeInBytes = base64String.Length * 8;

                var fileBlockIds = new List<string>();
                for (var offset = 0; offset < fileSizeInBytes; offset += base64ChunkSize)
                {
                    var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
                    fileBlockIds.Add(blockId);
                    var thisChunkSize = base64ChunkSize > base64String.Substring(offset).Length
                        ? base64String.Substring(offset).Length
                        : base64ChunkSize;
                    var thisPart = base64String.Substring(offset, thisChunkSize);
                    var uploadBlockRequest = new UploadBlockRequest()
                    {
                        BlockId = blockId,
                        BlockData = Convert.FromBase64String(thisPart),
                        FileContinuationToken = initialiseResponse.FileContinuationToken,
                    };
                    Execute(uploadBlockRequest);
                }

                var commitRequest = new CommitFileBlocksUploadRequest
                {
                    BlockList = fileBlockIds.ToArray(),
                    FileName = fileName,
                    FileContinuationToken = initialiseResponse.FileContinuationToken,
                    MimeType = "application/octet-stream"
                };
                Execute(commitRequest);
            }
        }

        public FilterExpression GetFetchFilterAsFilterExpression(string recordType, string fetchXmlFilter)
        {
            var fetchXml = "<fetch><entity name=\"" + recordType + "\">" + fetchXmlFilter + "</entity></fetch>";
            var response = (FetchXmlToQueryExpressionResponse)Execute(new FetchXmlToQueryExpressionRequest
            {
                FetchXml = fetchXml
            });
            return response.Query.Criteria;
        }
    }
}