using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.ObjectMapping;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Record.Xrm.Mappers;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace JosephM.Record.Xrm.XrmRecord
{
    public class XrmRecordService : IRecordService
    {
        private IFormService _formService;
        private XrmService _xrmService;
        private readonly Object _lockObject = new object();

        private readonly LookupMapper _lookupMapper = new LookupMapper();

        internal XrmRecordService(XrmService xrmService)
        {
            _xrmService = xrmService;
        }

        public XrmRecordService(IXrmRecordConfiguration iXrmRecordConfiguration, LogController controller, IFormService formService = null)
        {
            _formService = formService;
            Controller = controller;
            XrmRecordConfiguration = iXrmRecordConfiguration;
        }

        private IXrmRecordConfiguration _xrmRecordConfiguration;
        public IXrmRecordConfiguration XrmRecordConfiguration
        {
            get
            {
                return _xrmRecordConfiguration;
            }
            set
            {
                _xrmRecordConfiguration = value;
                if (_xrmRecordConfiguration != null)
                {
                    var xrmRecordConfiguration = new XrmRecordConfigurationInterfaceMapper().Map(_xrmRecordConfiguration);
                    var xrmConfiguration = new XrmConfigurationMapper().Map(xrmRecordConfiguration);
                    _xrmService = new XrmService(xrmConfiguration, Controller);
                }
            }
        }

        public string SendEmail(Lookup from, Lookup to, string subject, string body)
        {
            var email = new Entity(Entities.email);
            email.SetField(Fields.email_.subject, subject);
            email.SetField(Fields.email_.description, body);
            email.AddFromParty(from.RecordType, new Guid(from.Id));
            email.AddToParty(to.RecordType, new Guid(to.Id));
            email.Id = XrmService.Create(email);
            XrmService.SendEmail(email.Id);
            return email.Id.ToString();
        }

        public XrmRecordService(IXrmRecordConfiguration iXrmRecordConfiguration, IFormService formService = null)
            : this(iXrmRecordConfiguration, new LogController(), formService)
        {
        }

        //DON'T REMOVE THIS CONSTRUCTOR - REQUIRED BY ServiceConnection Attribute
        public XrmRecordService(IXrmRecordConfiguration iXrmRecordConfiguration)
            : this(iXrmRecordConfiguration, new LogController())
        {
        }


        public XrmService XrmService
        {
            get { return _xrmService; }
        }

        public IEnumerable<IFieldMetadata> GetFieldMetadata(string recordType)
        {
            return _xrmService
                .GetFields(recordType)
                .Select(f => new XrmFieldMetadata(f, recordType, _xrmService))
                .ToArray();
        }

        public IRecordTypeMetadata GetRecordTypeMetadata(string recordType)
        {
            return new XrmRecordTypeMetadata(recordType, _xrmService);
        }

        public IEnumerable<IPicklistSet> GetSharedPicklists()
        {
            return _xrmService.GetAllSharedOptionSets().Select(o => new XrmPicklistSet(o, _xrmService));
        }

        public IEnumerable<PicklistOption> GetPicklistKeyValues(string field, string recordType, string dependentValue, IRecord record)
        {
            var metadata = _xrmService.GetFieldMetadata(field, recordType);
            if (metadata.AttributeType == AttributeTypeCode.EntityName)
                return XrmService.GetAllEntityTypes().Select(s => new PicklistOption(s, XrmService.GetEntityDisplayName(s))).ToArray();

            return metadata.AttributeType == AttributeTypeCode.Picklist
                || metadata.AttributeType == AttributeTypeCode.State
                || metadata.AttributeType == AttributeTypeCode.Status
                || metadata.AttributeType == AttributeTypeCode.Integer
                || metadata is EnumAttributeMetadata
                ? _xrmService.GetPicklistKeyValues(recordType, field)
                .Select(kv => new PicklistOption(kv.Key.ToString(), kv.Value))
                .ToArray()
                : null;
        }

        public IRecord NewRecord(string recordType)
        {
            return new XrmRecord(recordType);
        }

        public IEnumerable<IRecord> GetLinkedRecords(string linkedrecordType, string recordTypeFrom,
            string linkedEntityLookup, string entityFromId)
        {
            return ToEnumerableIRecord(
                _xrmService.GetLinkedRecords(linkedrecordType, recordTypeFrom, linkedEntityLookup,
                    new Guid(entityFromId)));
        }


        public void Update(IRecord record, IEnumerable<string> changedPersistentFields)
        {
            if (changedPersistentFields == null)
                _xrmService.Update(ToEntity(record));
            else
                _xrmService.Update(ToEntity(record), changedPersistentFields);
        }

        public string GetLookupTargetType(string field, string recordType)
        {
            return _xrmService.GetLookupTargetEntity(field, recordType);
        }

        public void ClearCache()
        {
            lock (_lockObject)
            {
                _xrmService.ClearCache();
                _recordViews.Clear();
            }
        }

        public IsValidResponse VerifyConnection()
        {
            return _xrmService.VerifyConnection();
        }

        public IRecord Get(string recordType, string id)
        {
            var record = XrmService.GetFirst(recordType, XrmService.GetPrimaryKeyField(recordType), new Guid(id));
            return record == null ? null : ToIRecord(_xrmService.Retrieve(recordType, new Guid(id)));
        }


        public string GetFieldAsMatchString(string recordType, string fieldName, object fieldValue)
        {
            return _xrmService.GetFieldAsMatchString(recordType, fieldName, ToEntityValue(fieldValue));
        }

        public object ParseField(string fieldName, string recordType, object value)
        {
            var temp = ToEntityValue(value);
            var parsedValue = _xrmService.ParseField(fieldName,
                recordType,
                temp);
            var newValue = ToRecordField(parsedValue, fieldName, recordType);
            return newValue;
        }

        public IEnumerable<IRecord> RetrieveAllOrClauses(string recordType, IEnumerable<Condition> orConditions)
        {
            return
                ToEnumerableIRecord(_xrmService.RetrieveAllOrClauses(recordType, ToConditionExpressions(orConditions, recordType)));
        }

        private IEnumerable<IRecord> ToEnumerableIRecord(IEnumerable<Entity> entities)
        {
            return entities == null ? null : entities.Select(ToIRecord);
        }

        public IEnumerable<IRecord> RetrieveAllAndClauses(string recordType, IEnumerable<Condition> andConditions)
        {
            return
                ToEnumerableIRecord(_xrmService.RetrieveAllAndClauses(recordType, ToConditionExpressions(andConditions, recordType)));
        }

        public IEnumerable<IRecord> RetrieveAllAndClauses(string recordType, IEnumerable<Condition> andConditions,
            IEnumerable<string> fields)
        {
            return
                ToEnumerableIRecord(_xrmService.RetrieveAllAndClauses(recordType, ToConditionExpressions(andConditions, recordType),
                    fields)).ToArray();
        }

        public string Create(IRecord iRecord, IEnumerable<string> fieldToSet)
        {
            return _xrmService.Create(ToEntity(iRecord), fieldToSet).ToString();
        }

        public void Delete(IRecord record)
        {
            _xrmService.Delete(ToEntity(record));
        }

        public void Delete(string recordType, string id)
        {
            _xrmService.Delete(recordType, new Guid(id));
        }

        public IEnumerable<IRecord> GetFirstX(string recordType, int x, IEnumerable<Condition> conditions,
            IEnumerable<SortExpression> sorts)
        {
            return
                ToEnumerableIRecord(_xrmService.GetFirstX(recordType, x, ToConditionExpressions(conditions, recordType),
                    ToOrderExpressions(sorts)));
        }

        private IEnumerable<ConditionExpression> ToConditionExpressions(IEnumerable<Condition> conditions, string recordType)
        {
            return conditions == null ? null : conditions.Select(c => ToConditionExpression(c, recordType));
        }

        private ConditionExpression ToConditionExpression(Condition condition, string recordType)
        {
            if (condition.Value != null)
                return new ConditionExpression(condition.FieldName,
                    new ConditionTypeMapper().Map(condition.ConditionType),
                    ConvertToQueryValue(condition.FieldName, recordType, condition.Value));
            else
                return new ConditionExpression(condition.FieldName,
                    new ConditionTypeMapper().Map(condition.ConditionType));
        }

        private Condition ToCondition(ConditionExpression conditionexpression, string recordType)
        {
            if (conditionexpression.Values != null && conditionexpression.Values.Any())
                return new Condition(conditionexpression.AttributeName,
                    new ConditionTypeMapper().Map(conditionexpression.Operator),
                    conditionexpression.Values);
            else
                return new Condition(conditionexpression.AttributeName,
                    new ConditionTypeMapper().Map(conditionexpression.Operator));
        }

        private IRecord ToIRecord(Entity entity)
        {
            var xrmRecord = new XrmRecord(entity.LogicalName);
            xrmRecord.Id = entity.Id == Guid.Empty ? null : entity.Id.ToString();

            //map all the fields
            foreach (var field in entity.Attributes)
            {
                var newValue = ToRecordField(field.Value, field.Key, entity.LogicalName);
                xrmRecord.SetField(field.Key, newValue);
            }

            return xrmRecord;
        }

        private object ToRecordField(object originalValue, string fieldName, string recordType)
        {
            var newValue = originalValue;
            if (newValue == null || newValue is string)
            {
                //do nothing
            }
            if (newValue is EntityReference)
            {
                newValue = _lookupMapper.Map((EntityReference)newValue);
            }
            if (newValue is BooleanManagedProperty)
            {
                newValue = ((BooleanManagedProperty)newValue).Value;
            }
            else if (newValue is OptionSetValue)
            {
                var optionSetValue = (OptionSetValue)newValue;
                newValue = OptionValueToPicklist(fieldName, recordType, optionSetValue);
            }
            else if (newValue is EntityCollection)
            {
                var entities = ((EntityCollection)newValue).Entities;
                newValue = ToIRecords(entities);
            }
            else if (newValue is Guid)
            {
                newValue = ((Guid)newValue).ToString();
            }
            else if (newValue is Microsoft.Xrm.Sdk.Money)
            {
                newValue = ((Microsoft.Xrm.Sdk.Money)newValue).Value;
            }
            else if (newValue is OptionSetValueCollection)
            {
                newValue = ((OptionSetValueCollection)newValue).Select(o => OptionValueToPicklist(fieldName, recordType, o)).ToArray();
            }
            return newValue;
        }

        private PicklistOption OptionValueToPicklist(string fieldName, string recordType, OptionSetValue optionSetValue)
        {
            PicklistOption newValue;
            var key = optionSetValue.Value;
            try
            {
                newValue = new PicklistOption(key.ToString(), _xrmService.GetOptionLabel(key, fieldName, recordType));
            }
            catch (Exception)
            {
                newValue = new PicklistOption(key.ToString(), key.ToString());
            }
            return newValue;
        }

        private IEnumerable<Entity> ToEntities(IEnumerable<IRecord> iRecords)
        {
            return iRecords != null ? iRecords.Select(ToEntity) : null;
        }

        private Entity ToEntity(IRecord iRecord)
        {
            var entity = new Entity(iRecord.Type);
            if (!iRecord.Id.IsNullOrWhiteSpace())
                entity.Id = new Guid(iRecord.Id);

            //map only the fields with metadata - yomi fields through an error at pone point as there was no metadata returned
            var fieldsWithMetadata = XrmService.GetFields(iRecord.Type);
            foreach (var field in iRecord.GetFieldsInEntity().Where(s => fieldsWithMetadata.Contains(s)))
            {
                var originalValue = ToEntityValue(iRecord.GetField(field));
                if (originalValue is string && !string.IsNullOrWhiteSpace(originalValue.ToString()) && _xrmService.GetFieldType(field, iRecord.Type) == AttributeTypeCode.Uniqueidentifier)
                {
                    originalValue = new Guid(originalValue.ToString());
                }
                if (originalValue is decimal && _xrmService.GetFieldType(field, iRecord.Type) == AttributeTypeCode.Money)
                {
                    originalValue = new Microsoft.Xrm.Sdk.Money((decimal)originalValue);
                }
                entity.SetField(field, originalValue);
            }

            return entity;
        }

        public object ToEntityValue(object recordValue)
        {
            var temp = recordValue;
            if (temp is string)
            {
                //Strings no map
            }
            else if (temp is RecordType)
                temp = ((RecordType)temp).Key;
            else if (temp is PicklistOption)
                temp = new OptionSetValue(int.Parse(((PicklistOption)temp).Key));
            else if (temp is Lookup)
                temp = _lookupMapper.Map((Lookup)temp);
            else if (temp is Password)
                temp = ((Password)temp).GetRawPassword();
            else if (temp is IEnumerable<IRecord>)
                temp = ToEntities((IEnumerable<IRecord>)temp).ToArray();
            else if (temp is IEnumerable<PicklistOption>)
                temp = new OptionSetValueCollection(((IEnumerable<PicklistOption>)temp).Select(p => new OptionSetValue(int.Parse(((PicklistOption)p).Key))).ToList());
            return temp;
        }

        public bool FieldsEqual(object fieldValue1, object fieldValue2)
        {
            return XrmEntity.FieldsEqual(ToEntityValue(fieldValue1), ToEntityValue(fieldValue2));
        }

        /// <summary>
        ///     THIS DOESN'T CREATE THE FIELDS OR UPDATE THE PRIMARY FIELD
        /// </summary>
        public void CreateOrUpdate(RecordMetadata recordMetadata)
        {
            if (!_xrmService.EntityExists(recordMetadata.SchemaName))
            {
                _xrmService.CreateOrUpdateEntity(recordMetadata.SchemaName, recordMetadata.DisplayName,
                    recordMetadata.CollectionName,
                    recordMetadata.Description,
                    recordMetadata.Audit,
                    recordMetadata.PrimaryFieldSchemaName,
                    recordMetadata.PrimaryFieldDisplayName,
                    recordMetadata.PrimaryFieldDescription,
                    recordMetadata.PrimaryFieldMaxLength,
                    recordMetadata.PrimaryFieldIsMandatory,
                    recordMetadata.PrimaryFieldAudit,
                    recordMetadata.IsActivityType,
                    recordMetadata.Notes,
                    recordMetadata.Activities,
                    recordMetadata.Connections,
                    recordMetadata.MailMerge,
                    recordMetadata.Queues);
            }
            else
            {
                _xrmService.CreateOrUpdateEntity(recordMetadata.SchemaName, recordMetadata.DisplayName,
                    recordMetadata.CollectionName,
                    recordMetadata.Description,
                    recordMetadata.Audit,
                    null,
                    null,
                    null,
                    0,
                    false,
                    false,
                    recordMetadata.IsActivityType,
                    recordMetadata.Notes,
                    recordMetadata.Activities,
                    recordMetadata.Connections,
                    recordMetadata.MailMerge,
                    recordMetadata.Queues);
            }
        }

        /// <summary>
        ///     THIS DOESN'T UPDATE PICKLIST OPTIONS
        /// </summary>
        public void CreateOrUpdate(FieldMetadata field, string recordType)
        {
            switch (field.FieldType)
            {
                case (RecordFieldType.Boolean):
                    {
                        var typedField = (BooleanFieldMetadata)field;
                        _xrmService.CreateOrUpdateBooleanAttribute(typedField.SchemaName,
                            typedField.DisplayName,
                            typedField.Description,
                            typedField.IsMandatory,
                            typedField.Audit,
                            typedField.Searchable,
                            recordType);
                        break;
                    }
                case (RecordFieldType.Date):
                    {
                        var typedField = (DateFieldMetadata)field;
                        _xrmService.CreateOrUpdateDateAttribute(typedField.SchemaName,
                            typedField.DisplayName,
                            typedField.Description,
                            typedField.IsMandatory,
                            typedField.Audit,
                            typedField.Searchable,
                            recordType,
                            typedField.DateBehaviour,
                            typedField.IncludeTime
                            );
                        break;
                    }
                case (RecordFieldType.Decimal):
                    {
                        var typedField = (DecimalFieldMetadata)field;
                        _xrmService.CreateOrUpdateDecimalAttribute(typedField.SchemaName,
                            typedField.DisplayName,
                            typedField.Description,
                            typedField.IsMandatory,
                            typedField.Audit,
                            typedField.Searchable,
                            recordType,
                            typedField.MinValue, typedField.MaxValue, typedField.DecimalPrecision);
                        break;
                    }
                case (RecordFieldType.Integer):
                    {
                        var typedField = (IntegerFieldMetadata)field;
                        _xrmService.CreateOrUpdateIntegerAttribute(typedField.SchemaName,
                            typedField.DisplayName,
                            typedField.Description,
                            typedField.IsMandatory,
                            typedField.Audit,
                            typedField.Searchable,
                            recordType,
                            Convert.ToInt32(typedField.MinValue), Convert.ToInt32(typedField.MaxValue),
                            new IntegerTypeMapper().Map(
                                typedField.IntegerFormat));
                        break;
                    }
                case (RecordFieldType.Lookup):
                    {
                        var typedField = (LookupFieldMetadata)field;
                        _xrmService.CreateOrUpdateLookupAttribute(typedField.SchemaName,
                            typedField.DisplayName,
                            typedField.Description,
                            typedField.IsMandatory,
                            typedField.Audit,
                            typedField.Searchable,
                            recordType,
                            typedField.ReferencedRecordType,
                            typedField.DisplayInRelated);
                        break;
                    }
                case (RecordFieldType.Picklist):
                    {
                        var typedField = (PicklistFieldMetadata)field;
                        if (typedField.PicklistOptionSet.IsSharedOptionSet)
                        {
                            _xrmService.CreateOrUpdatePicklistAttribute(typedField.SchemaName,
                                typedField.DisplayName,
                                typedField.Description,
                                typedField.IsMandatory,
                                typedField.Audit,
                                typedField.Searchable,
                                recordType,
                                typedField.PicklistOptionSet
                                    .SchemaName, typedField.IsMultiSelect
                                );
                        }
                        else
                        {
                            _xrmService.CreateOrUpdatePicklistAttribute(typedField.SchemaName,
                                typedField.DisplayName,
                                typedField.Description,
                                typedField.IsMandatory,
                                typedField.Audit,
                                typedField.Searchable,
                                recordType,
                                typedField.PicklistOptions.Select(
                                    p =>
                                        new KeyValuePair<int, string>(
                                            int.Parse(p.Key), p.Value)), typedField.IsMultiSelect);
                        }
                        break;
                    }
                case (RecordFieldType.String):
                    {
                        var typedField = (StringFieldMetadata)field;
                        _xrmService.CreateOrUpdateStringAttribute(typedField.SchemaName,
                            typedField.DisplayName,
                            typedField.Description,
                            typedField.IsMandatory,
                            typedField.Audit,
                            typedField.Searchable,
                            recordType,
                            typedField.MaxLength,
                            new StringFormatMapper().Map(
                                typedField.TextFormat));
                        break;
                    }
                case (RecordFieldType.Money):
                    {
                        var typedField = (MoneyFieldMetadata)field;
                        _xrmService.CreateOrUpdateMoneyAttribute(typedField.SchemaName,
                            typedField.DisplayName,
                            typedField.Description,
                            typedField.IsMandatory,
                            typedField.Audit,
                            typedField.Searchable,
                            recordType,
                            Convert.ToDouble(typedField.MinValue), Convert.ToDouble(typedField.MaxValue));
                        break;
                    }
                case (RecordFieldType.Memo):
                    {
                        var typedField = (MemoFieldMetadata)field;
                        _xrmService.CreateOrUpdateMemoAttribute(typedField.SchemaName,
                            typedField.DisplayName,
                            typedField.Description,
                            typedField.IsMandatory,
                            typedField.Audit,
                            typedField.Searchable,
                            recordType,
                            typedField.MaxLength);
                        break;
                    }
                case (RecordFieldType.Status):
                    {
                        var typedField = (StatusFieldMetadata)field;
                        _xrmService.CreateOrUpdateStatusAttribute(typedField.SchemaName,
                                typedField.DisplayName,
                                typedField.Description,
                                typedField.IsMandatory,
                                typedField.Audit,
                                typedField.Searchable,
                                recordType);
                        break;
                    }
                case (RecordFieldType.Double):
                    {
                        var typedField = (DoubleFieldMetadata)field;
                        _xrmService.CreateOrUpdateDoubleAttribute(typedField.SchemaName,
                                typedField.DisplayName,
                                typedField.Description,
                                typedField.IsMandatory,
                                typedField.Audit,
                                typedField.Searchable,
                                recordType, Convert.ToDouble(typedField.MinValue), Convert.ToDouble(typedField.MaxValue), typedField.DecimalPrecision);
                        break;
                    }
                case (RecordFieldType.State):
                    {
                        var typedField = (StateFieldMetadata)field;
                        _xrmService.CreateOrUpdateStateAttribute(typedField.SchemaName,
                                typedField.DisplayName,
                                typedField.Description,
                                typedField.IsMandatory,
                                typedField.Audit,
                                typedField.Searchable,
                                recordType);
                        break;
                    }
                case (RecordFieldType.Virtual):
                case (RecordFieldType.BigInt):
                    {
                        throw new NotSupportedException(string.Format("Updating {0} Attributes Not Supported. Field = {1}", field.FieldType, field.SchemaName));
                    }
                case (RecordFieldType.Uniqueidentifier):
                    {
                        var typedField = (UniqueidentifierFieldMetadata)field;
                        _xrmService.CreateOrUpdateAttribute(typedField.SchemaName,
                                typedField.DisplayName,
                                typedField.Description,
                                typedField.IsMandatory,
                                typedField.Audit,
                                typedField.Searchable,
                                recordType);
                        break;
                    }
                default:
                    {
                        throw new NotImplementedException(string.Format("CreateOrUpdateField Not Implemented For Field {0} In Object {1} For Field Type {2}", field.SchemaName, field.GetType().Name, field.FieldType));
                    }
            }
        }

        public void CreateOrUpdate(IMany2ManyRelationshipMetadata relationshipMetadata)
        {
            _xrmService.CreateOrUpdateRelationship(relationshipMetadata.SchemaName,
                relationshipMetadata.RecordType1,
                relationshipMetadata.RecordType2,
                relationshipMetadata.RecordType1DisplayRelated,
                relationshipMetadata.RecordType2DisplayRelated,
                relationshipMetadata.RecordType1UseCustomLabel,
                relationshipMetadata.RecordType2UseCustomLabel,
                relationshipMetadata.RecordType1CustomLabel,
                relationshipMetadata.RecordType2CustomLabel,
                relationshipMetadata.RecordType1DisplayOrder,
                relationshipMetadata.RecordType2DisplayOrder);
        }

        public IEnumerable<IOne2ManyRelationshipMetadata> GetOneToManyRelationships(string recordType)
        {
            var relationships = _xrmService.GetEntityOneToManyRelationships(recordType);
            return relationships
                .Where(r => r.IsValidForAdvancedFind.HasValue && r.IsValidForAdvancedFind.Value)
                .Select(r => new XrmOne2ManyRelationship(r.SchemaName, r.ReferencedEntity, _xrmService))
                .ToArray();
        }

        public void Publish(string xml = null)
        {
            _xrmService.Publish(xml);
            _xrmService.ClearCache();
        }

        private readonly SortedDictionary<string, IEnumerable<ViewMetadata>> _recordViews =
            new SortedDictionary<string, IEnumerable<ViewMetadata>>();

        public IEnumerable<ViewMetadata> GetViews(string recordType)
        {
            lock (_lockObject)
            {
                if (!_recordViews.ContainsKey(recordType))
                {
                    var savedQueries =
                        _xrmService.RetrieveAllActive("savedquery", null,
                            new[]
                            {
                                new ConditionExpression("returnedtypecode",
                                    ConditionOperator.Equal,
                                    recordType)
                            },
                            null).ToArray();
                    var viewMetadatas = new List<ViewMetadata>();
                    foreach (var query in savedQueries)
                    {
                        var viewFields = new List<ViewField>();
                        var layoutXmlString = query.GetStringField("layoutxml");
                        if (layoutXmlString != null)
                        {
                            var layoutXml = new XmlDocument();
                            layoutXml.LoadXml(layoutXmlString);
                            var cellNodes = layoutXml.SelectNodes("//cell");
                            var i = 0;
                            if (cellNodes != null)
                            {
                                foreach (XmlNode item in cellNodes)
                                {
                                    if (item != null && item.Attributes != null)
                                    {
                                        //exclude where null or where a linked field
                                        //linked fields in view not implemented in UI
                                        if (item.Attributes["name"] == null 
                                            || item.Attributes["width"] == null
                                            || item.Attributes["name"].Value == null
                                            || item.Attributes["name"].Value.Contains(".")
                                            || !this.FieldExists(item.Attributes["name"].Value, recordType))
                                            continue;
                                        viewFields.Add(new ViewField(item.Attributes["name"].Value, ++i,
                                            Convert.ToInt32(item.Attributes["width"].Value)));
                                    }
                                }
                            }
                            var viewType = ViewType.Unmatched;
                            if (query.GetField("querytype") != null)
                            {
                                Enum.TryParse(query.GetInt("querytype").ToString(), out viewType);
                            }
                            var view = new ViewMetadata(viewFields) { ViewType = viewType, Id = query.Id.ToString() };
                            view.RawQuery = query.GetStringField(Fields.savedquery_.fetchxml);
                            viewMetadatas.Add(view);
                        }
                    }
                    _recordViews.Add(recordType, viewMetadatas);
                }

                return _recordViews[recordType];
            }
        }

        public void UpdateViews(RecordMetadata recordMetadata)
        {
            if (recordMetadata.Views == null || !recordMetadata.Views.Any())
                throw new NullReferenceException(string.Format("{0} Has No Views Loaded To Update",
                    recordMetadata.SchemaName));

            var viewUpdating = recordMetadata.Views.First();
            var savedQueries = GetViewsToUpdate(recordMetadata);
            foreach (var query in savedQueries)
            {
                var layoutXmlString = query.GetStringField("layoutxml");
                if (!string.IsNullOrWhiteSpace(layoutXmlString))
                {
                    var layoutXml = new XmlDocument();
                    layoutXml.LoadXml(layoutXmlString);

                    var cellNodes = layoutXml.SelectNodes("//cell");
                    if (cellNodes != null)
                    {
                        foreach (XmlNode item in cellNodes)
                        {
                            if (item.ParentNode != null)
                                item.ParentNode.RemoveChild(item);
                        }
                    }
                    var rowAttribute = layoutXml.SelectSingleNode("//row");
                    if (rowAttribute != null)
                    {
                        foreach (var viewField in viewUpdating.Fields.OrderByDescending(vf => vf.Order))
                        {
                            rowAttribute.InnerXml = "<cell name='" + viewField.FieldName + "' width='" + viewField.Width +
                                                    "' />" + rowAttribute.InnerXml;
                        }
                    }
                    query.SetField("layoutxml", layoutXml.InnerXml);
                }

                var fetchXmlString = query.GetStringField("fetchxml");
                var fetchXml = new XmlDocument();
                fetchXml.LoadXml(fetchXmlString);

                var attributeNodes = fetchXml.SelectNodes("//attribute");
                if (attributeNodes != null)
                {
                    foreach (XmlNode item in attributeNodes)
                    {
                        if (item != null && item.ParentNode != null)
                        {
                            item.ParentNode.RemoveChild(item);
                        }
                    }
                }
                var entityAttribute = fetchXml.SelectSingleNode("//entity");
                if (entityAttribute != null)
                {
                    foreach (var viewField in viewUpdating.Fields)
                    {
                        entityAttribute.InnerXml = "<attribute name='" + viewField.FieldName + "' />" +
                                                   entityAttribute.InnerXml;
                    }
                    entityAttribute.InnerXml = "<attribute name='" +
                                               _xrmService.GetPrimaryKeyField(recordMetadata.SchemaName) +
                                               "' />" + entityAttribute.InnerXml;
                    query.SetField("fetchxml", fetchXml.InnerXml);
                }
                _xrmService.Update(query, new[] { "layoutxml", "fetchxml" });
            }
        }

        public IEnumerable<Entity> GetViewsToUpdate(RecordMetadata recordMetadata)
        {
            var typesToUpdate = new[] { ViewType.AdvancedSearch, ViewType.AssociatedView, ViewType.MainApplicationView }
            .Cast<int>()
            .ToArray()
            .Cast<object>()
            .ToArray();
            var savedQueries = _xrmService.RetrieveAllActive("savedquery", null,
                    new[]
                    {
                        new ConditionExpression("returnedtypecode",
                            ConditionOperator.Equal,
                            recordMetadata.SchemaName),
                        new ConditionExpression(Fields.savedquery_.querytype,
                        ConditionOperator.In
                        , typesToUpdate)
                    },
                    null).ToArray();
            return savedQueries;
        }

        public void CreateOrUpdateSharedOptionSet(PicklistOptionSet sharedOptionSet)
        {
            _xrmService.CreateOrUpdateSharedOptionSet(
                sharedOptionSet.SchemaName,
                sharedOptionSet.DisplayName,
                sharedOptionSet.PicklistOptions.Select(p =>
                    new KeyValuePair<int, string>(
                        int.Parse(p.Key), p.Value))
                );
        }

        public void UpdateFieldOptionSet(string entityType, string fieldName, PicklistOptionSet optionSet)
        {
            _xrmService.UpdatePicklistOptions(
                fieldName,
                entityType,
                optionSet.PicklistOptions.Select(p => new KeyValuePair<int, string>(int.Parse(p.Key), p.Value))
                );
        }

        public IEnumerable<string> GetAllRecordTypes()
        {
            return _xrmService.GetAllEntityTypes();
        }

        public string GetFieldAsDisplayString(IRecord record, string fieldName)
        {
            return _xrmService.GetFieldAsDisplayString(record.Type, fieldName, ToEntityValue(record.GetField(fieldName)));
        }

        public IRecordService LookupService
        {
            get { return this; }
        }

        public void DeleteRecordType(string recordType)
        {
            _xrmService.DeleteEntity(recordType);
        }

        public void DeleteRelationship(string relationshipName)
        {
            _xrmService.DeleteRelationship(relationshipName);
        }

        public void DeleteSharedOptionSet(string schemaName)
        {
            _xrmService.DeleteSharedOptionSet(schemaName);
        }

        public IEnumerable<PicklistOption> GetSharedPicklistKeyValues(string name)
        {
            return _xrmService.GetSharedOptionSetKeyValues(name)
                .Select(kv => new PicklistOption(kv.Key.ToString(), kv.Value));
        }

        public IEnumerable<IsValidResponse> UpdateMultiple(List<IRecord> recordsToUpdate,
            IEnumerable<string> fieldsToUpdate)
        {
            return _xrmService.UpdateMultiple(recordsToUpdate.Select(ToEntity), fieldsToUpdate)
                .Select(r =>
                {
                    var isValid = new IsValidResponse();
                    if (r.Fault != null)
                        isValid.AddInvalidReason(r.Fault.Message);
                    return isValid;
                });
        }

        private IEnumerable<OrderExpression> ToOrderExpressions(IEnumerable<SortExpression> sortExpressions)
        {
            return sortExpressions == null ? null : sortExpressions.Select(ToOrderExpression);
        }

        private OrderExpression ToOrderExpression(SortExpression sort)
        {
            return new OrderExpression(sort.FieldName, new SortTypeMapper().Map(sort.SortType));
        }

        public IEnumerable<IRecord> RetrieveAllActive(string recordType)
        {
            return ToIRecords(_xrmService.RetrieveAllActive(recordType, null, null, null));
        }

        public IEnumerable<IRecord> ToIRecords(IEnumerable<Entity> entities)
        {
            return entities == null ? null : entities.Select(ToIRecord).ToArray();
        }

        public IEnumerable<IRecord> RetrieveAllFetch(string fetchQuery)
        {
            return ToIRecords(_xrmService.RetrieveAllFetch(fetchQuery));
        }

        public IEnumerable<IRecord> RetrieveAllOrClauses(string recordType, IEnumerable<Filter> filters)
        {
            var crmFilters = filters
                .Select(f => ToFilterExpression(f, recordType))
                .ToArray();
            return _xrmService.RetrieveAllOrClauses(recordType, crmFilters, null).Select(ToIRecord);
        }

        private FilterExpression ToFilterExpression(Filter filter, string recordType)
        {
            var mapper = new EnumMapper<FilterOperator, LogicalOperator>();
            var crmFilter = new FilterExpression();
            crmFilter.FilterOperator = mapper.Map(filter.ConditionOperator);
            if (filter.Conditions != null)
            {
                foreach (var c in filter.Conditions)
                    crmFilter.AddCondition(ToConditionExpression(c, recordType));
            }
            if (filter.SubFilters != null)
            {
                foreach (var f in filter.SubFilters)
                    crmFilter.Filters.Add(ToFilterExpression(f, recordType));
            }
            return crmFilter;
        }

        private Filter ToFilter(FilterExpression filterExpression, string recordType)
        {
            var mapper = new EnumMapper<LogicalOperator, FilterOperator>();
            var filter = new Filter();
            filter.ConditionOperator = mapper.Map(filterExpression.FilterOperator);
            filter.IsQuickFindFilter = filterExpression.IsQuickFindFilter;
            if (filterExpression.Conditions != null)
            {
                foreach (var c in filterExpression.Conditions)
                    filter.Conditions.Add(ToCondition(c, recordType));
            }
            if (filterExpression.Filters != null)
            {
                foreach (var f in filterExpression.Filters)
                    filter.SubFilters.Add(ToFilter(f, recordType));
            }
            return filter;
        }

        public string GetRelationshipLabel(One2ManyRelationshipMetadata relationship)
        {
            return _xrmService.GetOneToManyRelationshipLabel(relationship.ReferencedEntity, relationship.SchemaName);
        }

        public IEnumerable<IMany2ManyRelationshipMetadata> GetManyToManyRelationships(string recordType = null)
        {
            if (recordType == null)
            {
                var nnRelationships = _xrmService.AllRelationshipMetadata;
                return nnRelationships.Select(n => new XrmManyToManyRelationshipMetadata(n, _xrmService)).ToArray();
            }
            else
            {
                var relationships = _xrmService.GetEntityManyToManyRelationships(recordType);
                //var real = relationships.First();
                return relationships
                    //.Where(r => r.IsValidForAdvancedFind.HasValue && r.IsValidForAdvancedFind.Value)
                    .Select(r => new XrmManyToManyRelationshipMetadata(r.SchemaName, XrmService, recordType))
                    .ToArray();
            }
        }

        public IEnumerable<IRecord> GetRelatedRecords(IRecord record, IMany2ManyRelationshipMetadata relationship,
            bool from1)
        {
            return
                from1
                    ? ToIRecords(_xrmService.GetAssociatedEntities(relationship.RecordType1,
                        _xrmService.GetPrimaryKeyField(relationship.RecordType1),
                        relationship.Entity1IntersectAttribute, new Guid(record.Id), relationship.IntersectEntityName,
                        relationship.RecordType2, _xrmService.GetPrimaryKeyField(relationship.RecordType2),
                        relationship.Entity2IntersectAttribute))
                    : ToIRecords(_xrmService.GetAssociatedEntities(relationship.RecordType2,
                        _xrmService.GetPrimaryKeyField(relationship.RecordType2),
                        relationship.Entity2IntersectAttribute, new Guid(record.Id), relationship.IntersectEntityName,
                        relationship.RecordType1, _xrmService.GetPrimaryKeyField(relationship.RecordType1),
                        relationship.Entity1IntersectAttribute));
        }

        public IEnumerable<IRecord> RetrieveAllOrClauses(string entityType, IEnumerable<Condition> orConditions,
            IEnumerable<string> fields)
        {
            return ToIRecords(_xrmService.RetrieveAllOrClauses(entityType, ToConditionExpressions(orConditions, entityType), fields));
        }

        public bool IsActivityType(string recordType)
        {
            return _xrmService.GetEntityMetadata(recordType).IsActivity ?? false;
        }

        public IEnumerable<string> GetActivityParticipantTypes()
        {
            return _xrmService
                .GetAllEntityMetadata()
                .Where(m => m.IsActivityParty ?? false)
                .Select(m => m.LogicalName);
        }

        public bool IsActivityPartyParticipant(string recordType)
        {
            return _xrmService.GetEntityMetadata(recordType).IsActivityParty ?? false;
        }


        public IEnumerable<string> GetActivityTypes()
        {
            return _xrmService.GetAllEntityMetadata().Where(m => m.IsActivity ?? false).Select(m => m.LogicalName);
        }


        public IEnumerable<IRecord> GetFirstX(string type, int x, IEnumerable<string> fields,
            IEnumerable<Condition> conditions, IEnumerable<SortExpression> sort)
        {
            return
                ToIRecords(_xrmService.GetFirstX(type, x, fields, ToConditionExpressions(conditions, type),
                    ToOrderExpressions(sort)));
        }

        public IEnumerable<IRecord> GetLinkedRecordsThroughBridge(string linkedRecordType, string recordTypeThrough, string recordTypeFrom, string linkedThroughLookupFrom, string linkedThroughLookupTo, string recordFromId)
        {
            return ToIRecords(_xrmService.GetLinkedRecordsThroughBridge(linkedRecordType, recordTypeThrough, recordTypeFrom, linkedThroughLookupFrom, linkedThroughLookupTo, new Guid(recordFromId)));
        }

        public IRecordService GetLookupService(string fieldName, string recordType, string reference, IRecord record)
        {
            return LookupService;
        }

        public IsValidResponse IsValidForNewType(string typeName)
        {
            throw new NotImplementedException();
        }

        public object ConvertToQueryValue(string fieldName, string entityType, object value)
        {
            var temp = ToEntityValue(value);
            var parsedValue = temp;
            //if is a string don't parse as already correct type and may throw max length error
            if (!(_xrmService.IsString(fieldName, entityType) && parsedValue is string))
            {
                parsedValue = _xrmService.ParseField(fieldName,
                    entityType,
                    parsedValue);
            }
            if (parsedValue is EntityReference)
                parsedValue = ((EntityReference)parsedValue).Id;
            else if (parsedValue is OptionSetValue)
                parsedValue = ((OptionSetValue)parsedValue).Value;
            else if (parsedValue is Microsoft.Xrm.Sdk.Money)
                parsedValue = ((Microsoft.Xrm.Sdk.Money)parsedValue).Value;
            return parsedValue;
        }

        /// <summary>
        /// Runs a query but rather than collecting and returning all results, processes each paged query result in turn to reduce risk of memory overload
        /// </summary>
        public void ProcessResults(QueryDefinition query, Action<IEnumerable<IRecord>> processEachResultSet)
        {
            XrmService.ProcessQueryResults(ToQueryExpression(query), (entities) =>
            {
                var records = ToIRecords(entities);
                processEachResultSet(records);
            });
        }

        public string GetDisplayNameField(string recordType)
        {
            if (recordType == Entities.solution)
                return Fields.solution_.uniquename;
            return GetRecordTypeMetadata(recordType).PrimaryFieldSchemaName;
        }

        public IEnumerable<IRecord> RetreiveAll(QueryDefinition query)
        {
            var queryExpression = ToQueryExpression(query);
            return ToIRecords(XrmService.RetrieveFirstX(queryExpression, query.Top));
        }

        private QueryExpression ToQueryExpression(QueryDefinition query)
        {
            var queryExpression = new QueryExpression(query.RecordType);
            queryExpression.Distinct = query.Distinct;
            queryExpression.ColumnSet = XrmService.CreateColumnSet(query.Fields);
            queryExpression.Criteria = ToFilterExpression(query.RootFilter, query.RecordType);
            if (query.Joins != null)
            {
                foreach (var join in query.Joins)
                {
                    var link = queryExpression.AddLink(join.TargetType, join.SourceField, join.TargetField);
                    MapIntoLink(link, join);
                }
            }
            //if (query.Top > -1)
            //    queryExpression.TopCount = query.Top;
            queryExpression.Orders.AddRange(ToOrderExpressions(query.Sorts));
            return queryExpression;
        }

        private void MapIntoLink(LinkEntity link, Join join)
        {
            if (join.RootFilter != null)
            {
                link.LinkCriteria = ToFilterExpression(join.RootFilter, join.TargetType);
            }
            if (join.Joins != null)
            {
                foreach(var childJoin in join.Joins)
                {
                    var childLink = link.AddLink(childJoin.TargetType, childJoin.SourceField, childJoin.TargetField);
                    MapIntoLink(childLink, childJoin);
                }
            }
        }

        private Join ToJoin(LinkEntity link)
        {
            var join = new Join(link.LinkFromAttributeName, link.LinkFromEntityName, link.LinkToAttributeName);
            if (link.LinkCriteria != null)
            {
                join.RootFilter = ToFilter(link.LinkCriteria, link.LinkToEntityName);
            }
            if (link.LinkEntities != null)
            {
                foreach (var childLink in link.LinkEntities)
                {
                    join.Joins.Add(ToJoin(childLink));
                }
            }
            return join;
        }

        public string WebUrl
        {
            get
            {
                return XrmService.WebUrl;
            }
        }

        public LogController Controller { get; private set; }

        public string GetWebUrl(string recordType, string id, string additionalparams = null)
        {
            var idGuid = Guid.Empty;
            if (!Guid.TryParse(id, out idGuid))
                return null;
            return XrmService.GetWebUrl(recordType, idGuid, additionalparams);
        }

        public IFormService GetFormService()
        {
            return _formService;
        }

        public void SetFormService(IFormService formService)
        {
            _formService = formService;
        }

        public void AddSolutionComponents(string solutionId, int componentType, IEnumerable<string> itemIds)
        {
            var solution = Get(Entities.solution, solutionId);
            if (solution == null)
                throw new NullReferenceException($"No solution was found with id {solutionId}");

            var currentComponentIds = GetSolutionComponents(solutionId, componentType).ToList();
            foreach (var item in itemIds)
            {

                if (!currentComponentIds.Contains(item))
                {
                    var addRequest = new AddSolutionComponentRequest()
                    {
                        AddRequiredComponents = false,
                        ComponentType = componentType,
                        ComponentId = new Guid(item),
                        SolutionUniqueName = solution.GetStringField(Fields.solution_.uniquename)
                    };
                    XrmService.Execute(addRequest);
                    currentComponentIds.Add(item);
                }
            }
        }

        public IEnumerable<string> GetSolutionComponents(string solutionId, int componentType)
        {
            return RetrieveAllAndClauses(Entities.solutioncomponent, new[]
                {
                        new Condition(Fields.solutioncomponent_.componenttype, ConditionType.Equal, componentType),
                        new Condition(Fields.solutioncomponent_.solutionid, ConditionType.Equal, solutionId)
                    }, null)
                        .Select(r => r.GetIdField(Fields.solutioncomponent_.objectid))
                        .ToList();
        }

        public LoadToCrmResponse LoadIntoCrm(IEnumerable<IRecord> records, string matchField, bool setWorkflowRefreshField = false)
        {
            var response = new LoadToCrmResponse();
            if (records.Any())
            {
                var type = records.First().Type;
                var matchFields =
                    records.Select(r => r.GetField(matchField))
                            .Where(f => f != null)
                            .Select(f => ConvertToQueryValue(matchField, type, f))
                            .ToArray();

                var matchingRecords = !matchFields.Any()
                     ? new IRecord[0]
                     : RetrieveAllOrClauses(type,
                     matchFields.Select(s => new Condition(matchField, ConditionType.Equal, s)))
                     .ToArray();

                foreach (var record in records)
                {

                    try
                    {
                        var matchingItems =
                            matchingRecords.Where(r => FieldsEqual(r.GetField(matchField), record.GetField(matchField)))
                        .ToArray();
                        if (matchingItems.Any())
                        {
                            var matchingItem = matchingItems.First();
                            record.Id = matchingItem.Id;
                            var changedFields = record
                                .GetFieldsInEntity()
                                .Where(f => !FieldsEqual(record.GetField(f), matchingItem.GetField(f)))
                                .ToList();

                            //added this for plugin types where workflow activity
                            //do not update the in/out arguments
                            //explicitly setting the pluginassemblyid seems to refresh them
                            if (setWorkflowRefreshField
                                && record.Type == Entities.plugintype
                                && record.GetBoolField(Fields.plugintype_.isworkflowactivity)
                                && record.ContainsField(Fields.plugintype_.pluginassemblyid)
                                && !changedFields.Contains(Fields.plugintype_.pluginassemblyid))
                            {
                                changedFields.Add(Fields.plugintype_.pluginassemblyid);
                            }
                            //for some obscure reason the matching entity loads the lookup as a different type despite the id being the same
                            //so lets just remove if the id matches the existing record
                            if (changedFields.Contains(Fields.sdkmessageprocessingstep_.plugintypeid)
                                && record.GetLookupId(Fields.sdkmessageprocessingstep_.plugintypeid) == matchingItem.GetLookupId(Fields.sdkmessageprocessingstep_.plugintypeid))
                                changedFields.Remove(Fields.sdkmessageprocessingstep_.plugintypeid);

                            if (changedFields.Any())
                            {
                                if (record.Type == Entities.sdkmessageprocessingstepimage)
                                {
                                    foreach (var changedField in changedFields)
                                        matchingItem.SetField(changedField, record.GetField(changedField), this);
                                    Update(matchingItem, null);
                                }
                                else
                                {
                                    Update(record, changedFields);
                                }
                                response.AddUpdated(record);
                            }
                        }
                        else
                        {
                            record.Id = Create(record, null);
                            response.AddCreated(record);
                        }
                    }
                    catch (Exception ex)
                    {
                        response.AddError(record, ex);
                    }
                }
            }

            return response;
        }

        public class LoadToCrmResponse
        {
            private List<IRecord> _updated = new List<IRecord>();
            private List<IRecord> _created = new List<IRecord>();
            private Dictionary<IRecord, Exception> _errors = new Dictionary<IRecord, Exception>();

            public void AddCreated(IRecord record)
            {
                _created.Add(record);
            }
            public void AddUpdated(IRecord record)
            {
                _updated.Add(record);
            }

            public void AddError(IRecord record, Exception ex)
            {
                _errors[record] = ex;
            }

            public IEnumerable<IRecord> Updated { get { return _updated; } }

            public IEnumerable<IRecord> Created { get { return _created; } }

            public Dictionary<IRecord, Exception> Errors { get { return _errors; } }
        }

        public DeleteInCrmResponse DeleteInCrm(IEnumerable<IRecord> records)
        {
            var response = new DeleteInCrmResponse();
            if (records.Any())
            {
                foreach (var record in records)
                {

                    try
                    {
                        Delete(record);
                        response.AddDeleted(record);
                    }
                    catch (Exception ex)
                    {
                        response.AddError(record, ex);
                    }
                }
            }

            return response;
        }

        private TypeConfigs _typeConfigs = new TypeConfigs(new[]
        {
            new TypeConfigs.Config()
                {
                    Type = Entities.adx_webpage,
                    ParentLookupField = Fields.adx_webpage_.adx_rootwebpageid,
                    ParentLookupType = Entities.adx_webpage,
                    UniqueChildFields = new [] { Fields.adx_webpage_.adx_name, Fields.adx_webpage_.adx_websiteid, Fields.adx_webpage_.adx_webpagelanguageid },
                    BlockCreateChild = true
                },
            new TypeConfigs.Config()
                {
                    Type = Entities.adx_entityformmetadata,
                    UniqueChildFields = new [] { Fields.adx_entityformmetadata_.adx_entityform, Fields.adx_entityformmetadata_.adx_type, Fields.adx_entityformmetadata_.adx_sectionname, Fields.adx_entityformmetadata_.adx_attributelogicalname, Fields.adx_entityformmetadata_.adx_tabname, Fields.adx_entityformmetadata_.adx_subgrid_name }
                },
           new TypeConfigs.Config()
                {
                    Type = Entities.adx_entityform,
                    UniqueChildFields = new [] { Fields.adx_entityform_.adx_websiteid, Fields.adx_entityform_.adx_name }
                },
            new TypeConfigs.Config()
                {
                    Type = Entities.adx_webpageaccesscontrolrule,
                    UniqueChildFields = new [] { Fields.adx_webpageaccesscontrolrule_.adx_webpageid, Fields.adx_webpageaccesscontrolrule_.adx_name }
                },
            new TypeConfigs.Config()
                {
                    Type = Entities.productpricelevel,
                    UniqueChildFields = new [] { Fields.productpricelevel_.pricelevelid, Fields.productpricelevel_.productid, Fields.productpricelevel_.uomid }
                },
            new TypeConfigs.Config()
                {
                    Type = Entities.uom,
                    UniqueChildFields = new [] { Fields.uom_.uomscheduleid, Fields.uom_.baseuom, Fields.uom_.name }
                },
             new TypeConfigs.Config()
                {
                    Type = Entities.salesorderdetail,
                    UniqueChildFields = new [] { Fields.salesorderdetail_.salesorderid, Fields.salesorderdetail_.salesorderdetailid, Fields.salesorderdetail_.productid }
                },
             new TypeConfigs.Config()
                {
                    Type = Entities.adx_webformmetadata,
                    UniqueChildFields = new [] { Fields.adx_webformmetadata_.adx_webformstep, Fields.adx_webformmetadata_.adx_type, Fields.adx_webformmetadata_.adx_sectionname, Fields.adx_webformmetadata_.adx_attributelogicalname, Fields.adx_webformmetadata_.adx_tabname, Fields.adx_webformmetadata_.adx_subgrid_name }
                },
            new TypeConfigs.Config()
                {
                    Type = Entities.adx_webform,
                    UniqueChildFields = new [] { Fields.adx_webform_.adx_websiteid, Fields.adx_webform_.adx_name }
                },
             new TypeConfigs.Config()
                {
                    Type = Entities.adx_webformstep,
                    UniqueChildFields = new [] { Fields.adx_webformstep_.adx_webform, Fields.adx_webform_.adx_name }
                },
             new TypeConfigs.Config()
                {
                    Type = Entities.adx_weblinkset,
                    UniqueChildFields = new [] { Fields.adx_weblinkset_.adx_websiteid, Fields.adx_weblinkset_.adx_name }
                },
             new TypeConfigs.Config()
                {
                    Type = Entities.adx_weblink,
                    UniqueChildFields = new [] { Fields.adx_weblink_.adx_weblinksetid, Fields.adx_weblink_.adx_parentweblinkid, Fields.adx_weblink_.adx_name }
                },
             new TypeConfigs.Config()
                {
                    Type = Entities.adx_webtemplate,
                    UniqueChildFields = new [] { Fields.adx_webtemplate_.adx_websiteid, Fields.adx_webtemplate_.adx_name }
                },
             new TypeConfigs.Config()
                {
                    Type = Entities.adx_webfile,
                    UniqueChildFields = new [] { Fields.adx_webfile_.adx_websiteid, Fields.adx_webfile_.adx_name }
                },
             new TypeConfigs.Config()
                {
                    Type = Entities.adx_webrole,
                    UniqueChildFields = new [] { Fields.adx_webrole_.adx_websiteid, Fields.adx_website_.adx_name }
                },
             new TypeConfigs.Config()
                {
                    Type = Entities.adx_sitesetting,
                    UniqueChildFields = new [] { Fields.adx_sitesetting_.adx_websiteid, Fields.adx_sitesetting_.adx_name }
                },
             new TypeConfigs.Config()
                {
                    Type = Entities.adx_sitemarker,
                    UniqueChildFields = new [] { Fields.adx_sitemarker_.adx_websiteid, Fields.adx_sitemarker_.adx_name }
                },
             new TypeConfigs.Config()
                {
                    Type = Entities.adx_publishingstate,
                    UniqueChildFields = new [] { Fields.adx_publishingstate_.adx_websiteid, Fields.adx_publishingstate_.adx_name }
                },
             new TypeConfigs.Config()
                {
                    Type = Entities.adx_pagetemplate,
                    UniqueChildFields = new [] { Fields.adx_pagetemplate_.adx_websiteid, Fields.adx_pagetemplate_.adx_name }
                },
             new TypeConfigs.Config()
                {
                    Type = Entities.adx_entitypermission,
                    UniqueChildFields = new [] { Fields.adx_entitypermission_.adx_websiteid, Fields.adx_entitypermission_.adx_name }
                },
             new TypeConfigs.Config()
                {
                    Type = Entities.adx_entitylist,
                    UniqueChildFields = new [] { Fields.adx_entitylist_.adx_websiteid, Fields.adx_entitylist_.adx_name }
                },
             new TypeConfigs.Config()
                {
                    Type = Entities.adx_contentsnippet,
                    UniqueChildFields = new [] { Fields.adx_contentsnippet_.adx_websiteid, Fields.adx_contentsnippet_.adx_name }
                },
             new TypeConfigs.Config()
                {
                    Type = Entities.annotation,
                    UniqueChildFields = new [] { Fields.annotation_.objectid, Fields.annotation_.subject, Fields.annotation_.filename }
                },
        });
        public TypeConfigs GetTypeConfigs()
        {
            return _typeConfigs;
        }

        public IEnumerable<string> GetQuickfindFields(string recordType)
        {
            var results = new List<String>();
            var savedViews = GetViews(recordType);
            if (savedViews != null)
            {
                var matchingViews = savedViews.Where(v => v.ViewType == ViewType.QuickFindSearch);
                if (matchingViews.Any())
                {
                    var quickfindView = matchingViews.First();
                    if (quickfindView.RawQuery != null)
                    {
                        //okay think need to parse out the attributes
                        var startQuickFindFilter = quickfindView.RawQuery.IndexOf("isquickfindfields");
                        if (startQuickFindFilter != -1)
                        {
                            var endQuickFindFilter = quickfindView.RawQuery.IndexOf("</fil", startQuickFindFilter);
                            if (endQuickFindFilter != -1)
                            {
                                var currentIndex = startQuickFindFilter;
                                while (true)
                                {
                                    var nextAttribute = quickfindView.RawQuery.IndexOf("attribute=\"", currentIndex);
                                    if (nextAttribute == -1 || nextAttribute > endQuickFindFilter)
                                        break;
                                    var startAttribute = nextAttribute + 11;
                                    var endAttribute = quickfindView.RawQuery.IndexOf("\"", startAttribute);
                                    if (endAttribute == -1)
                                        break;
                                    var attributeName = quickfindView.RawQuery.Substring(startAttribute, endAttribute - startAttribute);
                                    results.Add(attributeName);
                                    currentIndex = endAttribute;
                                }
                            }
                        }
                    }
                }
            }
            if (!results.Any())
                results.Add(XrmService.GetPrimaryNameField(recordType));
            return results;
        }

        public class DeleteInCrmResponse
        {
            private List<IRecord> _deleted = new List<IRecord>();
            private Dictionary<IRecord, Exception> _errors = new Dictionary<IRecord, Exception>();

            public void AddDeleted(IRecord record)
            {
                _deleted.Add(record);
            }

            public void AddError(IRecord record, Exception ex)
            {
                _errors[record] = ex;
            }

            public IEnumerable<IRecord> Deleted { get { return _deleted; } }

            public Dictionary<IRecord, Exception> Errors { get { return _errors; } }
        }
    }
}
