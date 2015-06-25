#region

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.ObjectMapping;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Record.Xrm.Mappers;
using JosephM.Xrm;

#endregion

namespace JosephM.Record.Xrm.XrmRecord
{
    public class XrmRecordService : IRecordService
    {
        private readonly XrmService _xrmService;
        private readonly Object _lockObject = new object();

        private readonly LookupMapper _lookupMapper = new LookupMapper();

        internal XrmRecordService(XrmService xrmService)
        {
            _xrmService = xrmService;
        }

        public XrmRecordService(IXrmRecordConfiguration iXrmRecordConfiguration, LogController controller)
        {
            var xrmRecordConfiguration = new XrmRecordConfigurationInterfaceMapper().Map(iXrmRecordConfiguration);
            var xrmConfiguration = new XrmConfigurationMapper().Map(xrmRecordConfiguration);
            _xrmService = new XrmService(xrmConfiguration, controller);
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

        public IEnumerable<IRecord> RetrieveAll(string recordType, IEnumerable<string> fields)
        {
            return ToIRecords(_xrmService.RetrieveAllEntityType(recordType, fields));
        }

        public string GetFieldLabel(string field, string recordType)
        {
            return _xrmService.GetFieldLabel(field, recordType);
        }

        public int GetMaxLength(string field, string recordType)
        {
            return _xrmService.GetMaxLength(field, recordType);
        }

        public IEnumerable<PicklistOption> GetPicklistKeyValues(string field, string recordType)
        {
            return _xrmService.GetPicklistKeyValues(recordType, field)
                .Select(kv => new PicklistOption(kv.Key.ToString(), kv.Value))
                .ToArray();
        }

        public bool IsMandatory(string field, string recordType)
        {
            return _xrmService.IsMandatory(field, recordType);
        }

        public RecordFieldType GetFieldType(string field, string recordType)
        {
            return new FieldTypeMapper().Map(_xrmService.GetFieldType(field, recordType));
        }

        public int GetMaxIntValue(string field, string recordType)
        {
            return _xrmService.GetMaxIntValue(field, recordType) ?? int.MaxValue;
        }

        public int GetMinIntValue(string field, string recordType)
        {
            return _xrmService.GetMinIntValue(field, recordType) ?? int.MinValue;
        }


        public IRecord GetFirst(string recordType, string fieldName, object fieldValue)
        {
            return ToIRecord(_xrmService.GetFirst(recordType, fieldName, fieldValue));
        }


        public void Update(IRecord record)
        {
            _xrmService.Update(ToEntity(record));
        }


        public IRecord NewRecord(string recordType)
        {
            return new XrmRecord(recordType);
        }


        public string Create(IRecord record)
        {
            return _xrmService.Create(ToEntity(record)).ToString();
        }

        public ParseFieldResponse ParseFieldRequest(ParseFieldRequest parseFieldRequest)
        {
            try
            {
                //PciklistOptions are only used by the generic records so for this case we just pass in the picklist index
                var temp = ToEntityValue(parseFieldRequest.Value);
                var parsedValue = _xrmService.ParseField(parseFieldRequest.FieldName,
                    parseFieldRequest.RecordType,
                    temp);
                var newValue = ToRecordField(parsedValue, parseFieldRequest.FieldName, parseFieldRequest.RecordType);
                return new ParseFieldResponse(newValue);
            }
            catch (Exception ex)
            {
                return new ParseFieldResponse(ex.Message);
            }
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
            _xrmService.Update(ToEntity(record), changedPersistentFields);
        }

        public string GetLookupTargetType(string field, string recordType)
        {
            return _xrmService.GetLookupTargetEntity(field, recordType);
        }

        public IEnumerable<IRecord> RetrieveMultiple(string recordType, string searchString, int maxCount)
        {
            return ToEnumerableIRecord(_xrmService.RetrieveMultiple(recordType, searchString, maxCount));
        }


        public string GetPrimaryField(string recordType)
        {
            return _xrmService.GetPrimaryNameField(recordType);
        }

        public string GetFieldDescription(string fieldName, string recordType)
        {
            return _xrmService.GetFieldDescription(fieldName, recordType);
        }

        public void ClearCache()
        {
            _xrmService.ClearCache();
        }

        public bool IsNotNullable(string fieldName, string recordType)
        {
            return false;
        }

        public bool IsMultilineText(string fieldName, string recordType)
        {
            return _xrmService.IsMultilineText(fieldName, recordType);
        }

        public decimal GetMaxDecimalValue(string fieldName, string recordType)
        {
            var value = _xrmService.GetMaxDecimalValue(fieldName, recordType);
            return value ?? decimal.MaxValue;
        }

        public int GetDecimalPrecision(string fieldName, string recordType)
        {
            return _xrmService.GetPrecision(fieldName, recordType);
        }

        public decimal GetMinDecimalValue(string fieldName, string recordType)
        {
            var value = _xrmService.GetMinDecimalValue(fieldName, recordType);
            return value ?? decimal.MinValue;
        }

        public string GetDisplayName(string recordType)
        {
            return _xrmService.GetEntityDisplayName(recordType);
        }

        public string GetCollectionName(string recordType)
        {
            return _xrmService.GetEntityCollectionName(recordType);
        }

        public IsValidResponse VerifyConnection()
        {
            return _xrmService.VerifyConnection();
        }

        public bool IsAuditOn(string recordType)
        {
            return _xrmService.IsEntityAuditOn(recordType);
        }

        public bool IsFieldAuditOn(string fieldName, string recordType)
        {
            return _xrmService.IsFieldAuditOn(fieldName, recordType);
        }

        public bool IsFieldSearchable(string fieldName, string recordType)
        {
            return _xrmService.IsFieldSearchable(fieldName, recordType);
        }

        public TextFormat GetTextFormat(string fieldName, string recordType)
        {
            var format = _xrmService.GetTextFormat(fieldName, recordType);
            return format == null
                ? TextFormat.Text
                : new StringFormatMapper().Map((StringFormat) format);
        }

        public bool IsDateIncludeTime(string fieldName, string recordType)
        {
            return _xrmService.IsDateIncludeTime(fieldName, recordType);
        }

        public IEnumerable<IRecord> GetWhere(string recordType, string field, string value)
        {
            return ToEnumerableIRecord(
                _xrmService.RetrieveAllActive(recordType, null,
                    new[]
                    {
                        new ConditionExpression(field,
                            ConditionOperator.Equal,
                            value)
                    },
                    null));
        }

        public IEnumerable<string> GetFields(string recordType)
        {
            return _xrmService.GetFields(recordType);
        }

        public IRecord GetFirst(string recordType)
        {
            return ToIRecord(_xrmService.GetFirst(recordType));
        }


        public IntegerType GetIntegerType(string fieldType, string recordType)
        {
            return new IntegerTypeMapper().Map(_xrmService.GetIntegerFormat(fieldType, recordType));
        }


        public string GetPrimaryKey(string recordType)
        {
            return _xrmService.GetPrimaryKeyField(recordType);
        }


        public IRecord Get(string recordType, string id)
        {
            return ToIRecord(_xrmService.Retrieve(recordType, new Guid(id)));
        }


        public string GetFieldAsMatchString(string recordType, string fieldName, object fieldValue)
        {
            return _xrmService.GetFieldAsMatchString(recordType, fieldName, fieldValue);
        }

        public IDictionary<string, IRecord> IndexRecordsByField(string recordType, string matchField)
        {
            return _xrmService.IndexEntitiesByValue(recordType, matchField, null)
                .ToDictionary(kv => kv.Key, kv => ToIRecord(kv.Value));
        }

        public object ParseField(string fieldName, string recordType, object value)
        {
            return _xrmService.ParseField(fieldName, recordType, value);
        }

        public IDictionary<string, IEnumerable<string>> IndexAssociatedIds(string recordType, string relationshipName,
            string otherSideId)
        {
            return _xrmService.IndexAssociatedIds(recordType, relationshipName, otherSideId)
                .ToDictionary(kv => kv.Key.ToString(), kv => kv.Value.Select(g => g.ToString()));
        }

        public IEnumerable<string> GetAssociatedIds(string recordType, string id, string relationshipName,
            string otherSideId)
        {
            return _xrmService.GetAssociatedIds(recordType, new Guid(id), relationshipName, otherSideId)
                .Select(g => g.ToString());
        }


        public IDictionary<string, string> IndexGuidsByValue(string recordType, string fieldName)
        {
            return _xrmService.IndexGuidsByValue(recordType, fieldName)
                .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
        }


        public IDictionary<string, string> IndexMatchingGuids(string recordType, string fieldName,
            IEnumerable<string> unmatchedStrings)
        {
            return _xrmService.IndexMatchingGuids(recordType, fieldName, unmatchedStrings)
                .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
        }


        public bool IsLookup(string field, string recordType)
        {
            return _xrmService.IsLookup(field, recordType);
        }

        public bool IsFieldDisplayRelated(string field, string recordType)
        {
            return _xrmService.IsFieldDisplayRelated(field, recordType);
        }

        public bool IsSharedPicklist(string field, string recordType)
        {
            return _xrmService.IsSharedPicklist(field, recordType);
        }

        public string GetSharedPicklistDisplayName(string field, string recordType)
        {
            return _xrmService.GetSharedPicklistDisplayName(field, recordType);
        }

        public string GetSharedPicklistDisplayName(string optionSetName)
        {
            return _xrmService.GetSharedPicklistDisplayName(optionSetName);
        }

        public bool HasNotes(string recordType)
        {
            return _xrmService.HasNotes(recordType);
        }

        public bool HasActivities(string recordType)
        {
            return _xrmService.HasActivities(recordType);
        }

        public bool HasConnections(string recordType)
        {
            return _xrmService.HasConnections(recordType);
        }

        public bool HasMailMerge(string recordType)
        {
            return _xrmService.HasMailMerge(recordType);
        }

        public bool HasQueues(string recordType)
        {
            return _xrmService.HasQueues(recordType);
        }

        public string GetDescription(string recordType)
        {
            return _xrmService.GetDescription(recordType);
        }

        public IEnumerable<string> GetSharedOptionSetNames()
        {
            return _xrmService.GetSharedOptionSetNames();
        }

        public IEnumerable<KeyValuePair<string, string>> GetSharedOptionSetKeyValues(string optionSetName)
        {
            return _xrmService.GetSharedOptionSetKeyValues(optionSetName)
                .Select(kv => new KeyValuePair<string, string>(kv.Key.ToString(CultureInfo.InvariantCulture), kv.Value)).ToArray();
        }

        public bool IsCustomer(string recordType, string field)
        {
            return _xrmService.GetFieldType(field, recordType) == AttributeTypeCode.Customer;
        }

        public IEnumerable<IRecord> RetrieveAllOrClauses(string recordType, IEnumerable<Condition> orConditions)
        {
            return
                ToEnumerableIRecord(_xrmService.RetrieveAllOrClauses(recordType, ToConditionExpressions(orConditions)));
        }

        private IEnumerable<IRecord> ToEnumerableIRecord(IEnumerable<Entity> entities)
        {
            return entities == null ? null : entities.Select(ToIRecord);
        }

        public IEnumerable<IRecord> RetrieveAllAndClauses(string recordType, IEnumerable<Condition> andConditions)
        {
            return
                ToEnumerableIRecord(_xrmService.RetrieveAllAndClauses(recordType, ToConditionExpressions(andConditions)));
        }

        public IEnumerable<IRecord> RetrieveAllAndClauses(string recordType, IEnumerable<Condition> andConditions,
            IEnumerable<string> fields)
        {
            return
                ToEnumerableIRecord(_xrmService.RetrieveAllAndClauses(recordType, ToConditionExpressions(andConditions),
                    fields));
        }

        public void Create(IRecord iRecord, IEnumerable<string> fieldToSet)
        {
            _xrmService.Create(ToEntity(iRecord), fieldToSet);
        }

        public void Delete(IRecord record)
        {
            _xrmService.Delete(ToEntity(record));
        }

        public void Delete(string recordType, string id)
        {
            _xrmService.Delete(recordType, new Guid(id));
        }

        public IEnumerable<IRecord> GetFirstX(string recordType, int x)
        {
            return ToEnumerableIRecord(_xrmService.GetFirstX(recordType, x));
        }

        public IEnumerable<IRecord> GetFirstX(string recordType, int x, IEnumerable<Condition> conditions,
            IEnumerable<SortExpression> sorts)
        {
            return
                ToEnumerableIRecord(_xrmService.GetFirstX(recordType, x, ToConditionExpressions(conditions),
                    ToOrderExpressions(sorts)));
        }

        private IEnumerable<ConditionExpression> ToConditionExpressions(IEnumerable<Condition> conditions)
        {
            return conditions == null ? null : conditions.Select(ToConditionExpression);
        }

        private ConditionExpression ToConditionExpression(Condition condition)
        {
            if (condition.Value != null)
                return new ConditionExpression(condition.FieldName,
                    new ConditionTypeMapper().Map(condition.ConditionType),
                    condition.Value);
            else
                return new ConditionExpression(condition.FieldName,
                    new ConditionTypeMapper().Map(condition.ConditionType));
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
                newValue = _lookupMapper.Map((EntityReference) newValue);
            }
            else if (newValue is OptionSetValue)
            {
                var key = ((OptionSetValue) newValue).Value;
                try
                {
                    newValue = new PicklistOption(key.ToString(), _xrmService.GetOptionLabel(key, fieldName, recordType));
                }
                catch (Exception)
                {
                    //if no match on the option value just set null
                    newValue = null;
                }
            }
            else if (newValue is EntityCollection)
            {
                var entities = ((EntityCollection) newValue).Entities;
                newValue = ToIRecords(entities);
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

            //map all the fields
            foreach (var field in iRecord.GetFieldsInEntity())
            {
                var originalValue = ToEntityValue(iRecord.GetField(field));
                entity.SetField(field, originalValue);
            }

            return entity;
        }

        private object ToEntityValue(object recordValue)
        {
            var temp = recordValue;
            if (temp is string)
            {
                //Strings no map
            }
            else if (temp is PicklistOption)
                temp = new OptionSetValue(int.Parse(((PicklistOption) temp).Key));
            else if (temp is Lookup)
                temp = _lookupMapper.Map((Lookup) temp);
            else if (temp is Password)
                temp = ((Password) temp).GetRawPassword();
            else if (temp is IEnumerable<IRecord>)
                temp = ToEntities((IEnumerable<IRecord>) temp).ToArray();
            return temp;
        }

        public bool FieldsEqual(object fieldValue1, object fieldValue2)
        {
            return XrmEntity.FieldsEqual(fieldValue1, fieldValue2);
        }

        /// <summary>
        ///     THIS DOESN'T CREATE THE FIELDS OR UPDATE THE PRIMARY FIELD
        /// </summary>
        public void CreateOrUpdate(RecordMetadata recordMetadata)
        {
            if (!_xrmService.EntityExists(recordMetadata.SchemaName))
            {
                _xrmService.CreateOrUpdateEntity(recordMetadata.SchemaName, recordMetadata.DisplayName,
                    recordMetadata.DisplayCollectionName,
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
                    recordMetadata.DisplayCollectionName,
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
                    var typedField = (BooleanFieldMetadata) field;
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
                    var typedField = (DateFieldMetadata) field;
                    _xrmService.CreateOrUpdateDateAttribute(typedField.SchemaName,
                        typedField.DisplayName,
                        typedField.Description,
                        typedField.IsMandatory,
                        typedField.Audit,
                        typedField.Searchable,
                        recordType,
                        typedField.IncludeTime
                        );
                    break;
                }
                case (RecordFieldType.Decimal):
                {
                    var typedField = (DecimalFieldMetadata) field;
                    _xrmService.CreateOrUpdateDecimalAttribute(typedField.SchemaName,
                        typedField.DisplayName,
                        typedField.Description,
                        typedField.IsMandatory,
                        typedField.Audit,
                        typedField.Searchable,
                        recordType,
                        typedField.Minimum, typedField.Maximum, typedField.DecimalPrecision);
                    break;
                }
                case (RecordFieldType.Integer):
                {
                    var typedField = (IntegerFieldMetadata) field;
                    _xrmService.CreateOrUpdateIntegerAttribute(typedField.SchemaName,
                        typedField.DisplayName,
                        typedField.Description,
                        typedField.IsMandatory,
                        typedField.Audit,
                        typedField.Searchable,
                        recordType,
                        typedField.Minimum, typedField.Maximum);
                    break;
                }
                case (RecordFieldType.Lookup):
                {
                    var typedField = (LookupFieldMetadata) field;
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
                    var typedField = (PicklistFieldMetadata) field;
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
                                .SchemaName);
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
                                        int.Parse(p.Key), p.Value)));
                    }
                    break;
                }
                case (RecordFieldType.String):
                {
                    var typedField = (StringFieldMetadata) field;
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
                    var typedField = (MoneyFieldMetadata) field;
                    _xrmService.CreateOrUpdateMoneyAttribute(typedField.SchemaName,
                        typedField.DisplayName,
                        typedField.Description,
                        typedField.IsMandatory,
                        typedField.Audit,
                        typedField.Searchable,
                        recordType,
                        typedField.Minimum, typedField.Maximum);
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
                            recordType, typedField.Minimum, typedField.Maximum, typedField.DecimalPrecision);
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
                    break;
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
                    break;
                }
            }
        }

        public bool RecordTypeExists(string recordType)
        {
            return _xrmService.EntityExists(recordType);
        }


        public void CreateOrUpdate(RelationshipMetadata relationshipMetadata)
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

        public IEnumerable<One2ManyRelationshipMetadata> GetOneToManyRelationships(string recordType)
        {
            var relationships = _xrmService.GetEntityOneToManyRelationships(recordType);
            //var real = relationships.First();

            var mapper = new OneToManyRelationshipTypeMapper();
            return relationships
                .Where(r => r.IsValidForAdvancedFind.HasValue && r.IsValidForAdvancedFind.Value)
                .Select(mapper.Map)
                .ToArray();
        }

        public bool FieldExists(string fieldName, string recordType)
        {
            return _xrmService.FieldExists(fieldName, recordType);
        }

        public bool RelationshipExists(string relationship)
        {
            return _xrmService.RelationshipExists(relationship);
        }

        public void PublishAll()
        {
            _xrmService.PublishAll();
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
                                    if (item != null && item.Attributes != null )
                                    {
                                        if (item.Attributes["name"] == null || item.Attributes["width"] == null)
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
                            var view = new ViewMetadata(viewFields) {ViewType = viewType};
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
            var viewNamesToUpdate = GetViewNamesToUpdate(recordMetadata);

            var savedQueries =
                _xrmService.RetrieveAllActive("savedquery", null,
                    new[]
                    {
                        new ConditionExpression("returnedtypecode",
                            ConditionOperator.Equal,
                            recordMetadata.SchemaName)
                    },
                    null).ToArray();
            foreach (var query in savedQueries.Where(sq => viewNamesToUpdate.Contains(sq.GetStringField("name"))))
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
                _xrmService.Update(query);
            }
        }


        public bool SharedOptionSetExists(string optionSetName)
        {
            return _xrmService.SharedOptionSetExists(optionSetName);
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

        public IEnumerable<string> GetAllRecordTypesForSearch()
        {
            return
                _xrmService.GetAllEntityMetadata()
                    .Where(m => m.IsValidForAdvancedFind ?? false)
                    .Select(m => m.LogicalName);
        }

        public string GetFieldAsDisplayString(IRecord record, string fieldName)
        {
            return _xrmService.GetFieldAsDisplayString(record.Type, fieldName, ToEntityValue(record.GetField(fieldName)));
        }

        public string GetSqlViewName(string recordType)
        {
            return _xrmService.GetFilteredViewName(recordType);
        }

        public string GetDatabaseName()
        {
            return _xrmService.GetDatabaseName();
        }

        public IRecordService LookupService
        {
            get { return this; }
        }

        public string[] GetViewNamesToUpdate(RecordMetadata recordMetadata)
        {
            var displayName = _xrmService.GetEntityDisplayName(recordMetadata.SchemaName);
            var displayCollectionName =
                _xrmService.GetEntityDisplayCollectionName(recordMetadata.SchemaName);
            var viewNamesToUpdate = new[]
            {
                displayName + " Associated View",
                //"Quick Find Active " + displayCollectionName,
                displayName + " Advanced Find View",
                "Active " + displayCollectionName,
                "My " + displayCollectionName,
                "Inactive " + displayCollectionName
            };
            return viewNamesToUpdate;
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
            var mapper = new EnumMapper<FilterOperator, LogicalOperator>();
            var crmFilters = filters
                .Select(f =>
                {
                    var crmFilter = new FilterExpression();

                    crmFilter.FilterOperator = mapper.Map(f.ConditionOperator);
                    foreach (var c in f.Conditions)
                        crmFilter.AddCondition(ToConditionExpression(c));
                    return crmFilter;
                })
                .ToArray();
            return _xrmService.RetrieveAllOrClauses(recordType, crmFilters, null).Select(ToIRecord);
        }

        public int GetObjectTypeCode(string recordType)
        {
            return _xrmService.GetObjectTypeCode(recordType);
        }


        public IEnumerable<IRecord> GetRelatedRecords(IRecord record, One2ManyRelationshipMetadata relationship)
        {
            return
                ToIRecords(_xrmService.GetLinkedRecords(relationship.ReferencedEntity,
                    relationship.ReferencedAttribute, new Guid(record.Id), relationship.ReferencingEntity,
                    relationship.ReferencingAttribute));
        }

        public string GetRelationshipLabel(One2ManyRelationshipMetadata relationship)
        {
            return _xrmService.GetOneToManyRelationshipLabel(relationship.ReferencedEntity, relationship.SchemaName);
        }

        public IEnumerable<Many2ManyRelationshipMetadata> GetManyToManyRelationships(string recordType)
        {
            var relationships = _xrmService.GetEntityManyToManyRelationships(recordType);
            //var real = relationships.First();

            var mapper = new ManyToManyRelationshipTypeMapper();
            return relationships
                .Where(r => r.IsValidForAdvancedFind.HasValue && r.IsValidForAdvancedFind.Value)
                .Select(mapper.Map)
                .ToArray();
        }

        public IEnumerable<IRecord> GetRelatedRecords(IRecord record, Many2ManyRelationshipMetadata relationship,
            bool from1)
        {
            return
                from1
                    ? ToIRecords(_xrmService.GetAssociatedEntities(relationship.Entity1LogicalName,
                        GetPrimaryKey(relationship.Entity1LogicalName),
                        relationship.Entity1IntersectAttribute, new Guid(record.Id), relationship.IntersectEntityName,
                        relationship.Entity2LogicalName, relationship.Entity2IntersectAttribute,
                        GetPrimaryKey(relationship.Entity2LogicalName)))
                    : ToIRecords(_xrmService.GetAssociatedEntities(relationship.Entity2LogicalName,
                        GetPrimaryKey(relationship.Entity2LogicalName),
                        relationship.Entity2IntersectAttribute, new Guid(record.Id), relationship.IntersectEntityName,
                        relationship.Entity1LogicalName, relationship.Entity1IntersectAttribute,
                        GetPrimaryKey(relationship.Entity1LogicalName)));
        }

        public string GetRelationshipLabel(Many2ManyRelationshipMetadata relationship, bool from1)
        {
            return _xrmService.GetManyToManyRelationshipLabel(relationship.Entity1LogicalName, relationship.SchemaName,
                from1);
        }

        public bool IsActivityParty(string fieldName, string recordType)
        {
            return _xrmService.IsActivityParty(fieldName, recordType);
        }

        public void Associate(Many2ManyRelationshipMetadata relationship, IRecord record1, IRecord record2)
        {
            throw new NotImplementedException();
        }

        public string GetOptionLabel(string optionKey, string field, string recordType)
        {
            if (optionKey.IsNullOrWhiteSpace() || optionKey == "-1")
                return null;
            var parsedInteger = 0;
            if (int.TryParse(optionKey, out parsedInteger))
            {
                return _xrmService.GetOptionLabel(parsedInteger, field, recordType);
            }
            throw new ArgumentOutOfRangeException("optionKey", "Expected Integer String Actual Value Was: " + optionKey);
        }


        public bool IsString(string fieldName, string recordType)
        {
            return _xrmService.IsString(fieldName, recordType);
        }

        public IEnumerable<string> GetStringFields(string recordType)
        {
            var fieldMetadata = _xrmService.GetEntityFieldMetadata(recordType)
                .Where(s => _xrmService.IsString(s.LogicalName, recordType));
            return fieldMetadata
                .Where(f =>
                {
                    if (!(f.IsValidForRead ?? false)
                        || !f.AttributeOf.IsNullOrWhiteSpace())
                        return false;
                    var thisMetadata = _xrmService.GetFieldMetadata(f.LogicalName, recordType);
                    return (!(thisMetadata is StringAttributeMetadata) ||
                            ((StringAttributeMetadata) thisMetadata).YomiOf.IsNullOrWhiteSpace());
                })
                .Select(f => f.LogicalName);
        }


        public IEnumerable<IRecord> RetrieveAllOrClauses(string entityType, IEnumerable<Condition> orConditions,
            IEnumerable<string> fields)
        {
            return ToIRecords(_xrmService.RetrieveAllOrClauses(entityType, ToConditionExpressions(orConditions), fields));
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
                ToIRecords(_xrmService.GetFirstX(type, x, fields, ToConditionExpressions(conditions),
                    ToOrderExpressions(sort)));
        }


        public IEnumerable<PicklistOption> GetPicklistKeyValues(string field, string recordType, string dependentValue, IRecord record)
        {
            throw new NotImplementedException();
        }


        public bool IsWritable(string fieldName, string recordType)
        {
            return _xrmService.IsWritable(fieldName, recordType);
        }

        public bool IsCreateable(string fieldName, string recordType)
        {
            return _xrmService.IsCreateable(fieldName, recordType);
        }

        public bool IsReadable(string fieldName, string recordType)
        {
            return _xrmService.IsReadable(fieldName, recordType);
        }

        public bool IsCustomField(string fieldName, string recordType)
        {
            return _xrmService.GetFieldMetadata(fieldName, recordType).IsCustomAttribute ?? false;
        }

        public bool IsCustomType(string recordType)
        {
            return _xrmService.GetEntityMetadata(recordType).IsCustomEntity ?? false;
        }

        public bool IsCustomRelationship(string relationshipName)
        {
            return _xrmService.IsCustomRelationship(relationshipName);
        }

        public string GetRecordTypeCode(string thisType)
        {
            return _xrmService.GetObjectTypeCode(thisType).ToString();
        }

        public bool IsDisplayRelated(Many2ManyRelationshipMetadata relationship, bool from1)
        {
            return XrmService.IsManyToManyDisplayRelated(relationship.Entity1LogicalName, relationship.SchemaName, from1);
        }

        public bool IsCustomLabel(Many2ManyRelationshipMetadata relationship, bool from1)
        {
            return XrmService.IsManyToManyCustomLabel(relationship.Entity1LogicalName, relationship.SchemaName, from1);
        }

        public int DisplayOrder(Many2ManyRelationshipMetadata relationship, bool from1)
        {
            return XrmService.ManyToManyDisplayOrder(relationship.Entity1LogicalName, relationship.SchemaName, from1);
        }

        public int GetDisplayOrder(Many2ManyRelationshipMetadata relationship, bool from1)
        {
            return XrmService.GetManyToManyDisplayOrder(relationship.Entity1LogicalName, relationship.SchemaName, from1);
        }

        public bool IsDisplayRelated(One2ManyRelationshipMetadata relationship)
        {
            return XrmService.IsOneToManyDisplayRelated(relationship.ReferencedEntity, relationship.SchemaName);
        }

        public int GetDisplayOrder(One2ManyRelationshipMetadata relationship)
        {
            return XrmService.GetOneToManyDisplayOrder(relationship.ReferencedEntity, relationship.SchemaName);
        }

        public bool IsCustomLabel(One2ManyRelationshipMetadata relationship)
        {
            return XrmService.GetOneToManyIsCustomLabel(relationship.ReferencedEntity, relationship.SchemaName);
        }

        public double GetMinDoubleValue(string field, string thisType)
        {
            var value = _xrmService.GetMinDoubleValue(field, thisType);
            if (value.HasValue)
                return Convert.ToDouble(value.Value);
            else
                return double.MinValue;
        }

        public double GetMaxDoubleValue(string field, string thisType)
        {
            var value = _xrmService.GetMaxDoubleValue(field, thisType);
            if (value.HasValue)
                return Convert.ToDouble(value.Value);
            else
                return double.MaxValue;
        }

        public decimal GetMinMoneyValue(string field, string thisType)
        {
            var value = _xrmService.GetMinMoneyValue(field, thisType);
            if (value.HasValue)
                return Convert.ToDecimal(value.Value);
            else
                return decimal.MinValue;
        }

        public decimal GetMaxMoneyValue(string field, string thisType)
        {
            var value = _xrmService.GetMaxMoneyValue(field, thisType);
            if (value.HasValue)
                return Convert.ToDecimal(value.Value);
            else
                return decimal.MaxValue;
        }

        public IEnumerable<IRecord> GetLinkedRecordsThroughBridge(string linkedRecordType, string recordTypeThrough, string recordTypeFrom, string linkedThroughLookupFrom, string linkedThroughLookupTo, string recordFromId)
        {
            return ToIRecords(_xrmService.GetLinkedRecordsThroughBridge(linkedRecordType, recordTypeThrough, recordTypeFrom, linkedThroughLookupFrom, linkedThroughLookupTo, new Guid(recordFromId)));
        }

        public bool IsMultiline(string field, string recordType)
        {
            return _xrmService.IsMultilineText(field, recordType);
        }

        public IRecordService GetLookupService(string fieldName, string recordType, string reference, IRecord record)
        {
            return LookupService;
        }
    }
}