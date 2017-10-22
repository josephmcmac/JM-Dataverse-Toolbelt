#region

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
using JosephM.Xrm.Schema;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

#endregion

namespace JosephM.Record.Xrm.XrmRecord
{
    public class XrmRecordService : IRecordService
    {
        private IFormService _formService;
        private readonly XrmService _xrmService;
        private readonly Object _lockObject = new object();

        private readonly LookupMapper _lookupMapper = new LookupMapper();

        internal XrmRecordService(XrmService xrmService)
        {
            _xrmService = xrmService;
        }

        public XrmRecordService(IXrmRecordConfiguration iXrmRecordConfiguration, LogController controller, IFormService formService = null)
        {
            _formService = formService;
            XrmRecordConfiguration = iXrmRecordConfiguration;
            var xrmRecordConfiguration = new XrmRecordConfigurationInterfaceMapper().Map(iXrmRecordConfiguration);
            var xrmConfiguration = new XrmConfigurationMapper().Map(xrmRecordConfiguration);
            _xrmService = new XrmService(xrmConfiguration, controller);
        }

        public IXrmRecordConfiguration XrmRecordConfiguration { get; set; }

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
            var type = _xrmService.GetFieldType(field, recordType);
            return type == AttributeTypeCode.Picklist
                || type == AttributeTypeCode.State
                || type == AttributeTypeCode.Status
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
            return ToIRecord(_xrmService.Retrieve(recordType, new Guid(id)));
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
                    fields));
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
            else if (newValue is OptionSetValue)
            {
                var key = ((OptionSetValue)newValue).Value;
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
                var entities = ((EntityCollection)newValue).Entities;
                newValue = ToIRecords(entities);
            }
            else if (newValue is Guid)
            {
                newValue = ((Guid)newValue).ToString();
            }
            else if (newValue is Money)
            {
                newValue = ((Money)newValue).Value;
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
                temp = new OptionSetValue(int.Parse(((PicklistOption)temp).Key));
            else if (temp is Lookup)
                temp = _lookupMapper.Map((Lookup)temp);
            else if (temp is Password)
                temp = ((Password)temp).GetRawPassword();
            else if (temp is IEnumerable<IRecord>)
                temp = ToEntities((IEnumerable<IRecord>)temp).ToArray();
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
                            Convert.ToInt32(typedField.MinValue), Convert.ToInt32(typedField.MaxValue));
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
                                        if (item.Attributes["name"] == null || item.Attributes["width"] == null || item.Attributes["name"].Value == null || item.Attributes["name"].Value.Contains("."))
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
                            var view = new ViewMetadata(viewFields) { ViewType = viewType };
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
            var typesToUpdate = new[] { ViewType.AdvancedSearch, ViewType.AssociatedView, ViewType.MainApplicationView, ViewType.QuickFindSearch }
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

        public string GetRelationshipLabel(One2ManyRelationshipMetadata relationship)
        {
            return _xrmService.GetOneToManyRelationshipLabel(relationship.ReferencedEntity, relationship.SchemaName);
        }

        public IEnumerable<IMany2ManyRelationshipMetadata> GetManyToManyRelationships(string recordType)
        {
            var relationships = _xrmService.GetEntityManyToManyRelationships(recordType);
            //var real = relationships.First();
            return relationships
                //.Where(r => r.IsValidForAdvancedFind.HasValue && r.IsValidForAdvancedFind.Value)
                .Select(r => new XrmManyToManyRelationshipMetadata(r.SchemaName, XrmService, recordType))
                .ToArray();
        }

        public IEnumerable<IRecord> GetRelatedRecords(IRecord record, IMany2ManyRelationshipMetadata relationship,
            bool from1)
        {
            return
                from1
                    ? ToIRecords(_xrmService.GetAssociatedEntities(relationship.RecordType1,
                        _xrmService.GetPrimaryKeyField(relationship.RecordType1),
                        relationship.Entity1IntersectAttribute, new Guid(record.Id), relationship.IntersectEntityName,
                        relationship.RecordType2, relationship.Entity2IntersectAttribute,
                        _xrmService.GetPrimaryKeyField(relationship.RecordType2)))
                    : ToIRecords(_xrmService.GetAssociatedEntities(relationship.RecordType2,
                        _xrmService.GetPrimaryKeyField(relationship.RecordType2),
                        relationship.Entity2IntersectAttribute, new Guid(record.Id), relationship.IntersectEntityName,
                        relationship.RecordType1, relationship.Entity1IntersectAttribute,
                        _xrmService.GetPrimaryKeyField(relationship.RecordType1)));
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
            var parsedValue = _xrmService.ParseField(fieldName,
                entityType,
                temp);
            if (parsedValue is EntityReference)
                parsedValue = ((EntityReference)parsedValue).Id;
            else if (parsedValue is OptionSetValue)
                parsedValue = ((OptionSetValue)parsedValue).Value;
            else if (parsedValue is Money)
                parsedValue = ((Money)parsedValue).Value;
            return parsedValue;
        }

        public void ProcessResults(string recordType, IEnumerable<string> fields, IEnumerable<Condition> conditions, Action<IEnumerable<IRecord>> processEachResultSet)
        {
            var query = XrmService.BuildQuery(recordType, fields, ToConditionExpressions(conditions, recordType), null);
            XrmService.ProcessQueryResults(query, (entities) =>
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
            var queryExpression = new QueryExpression(query.RecordType);
            queryExpression.ColumnSet = XrmService.CreateColumnSet(query.Fields);
            queryExpression.Criteria = ToFilterExpression(query.RootFilter, query.RecordType);
            //if (query.Top > -1)
            //    queryExpression.TopCount = query.Top;
            queryExpression.Orders.AddRange(ToOrderExpressions(query.Sorts));
            return ToIRecords(XrmService.RetrieveFirstX(queryExpression, query.Top));
        }

        public string WebUrl
        {
            get
            {
                return XrmService.WebUrl;
            }
        }

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
    }
}
