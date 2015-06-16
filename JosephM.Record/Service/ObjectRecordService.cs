#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.IService;
using JosephM.Record.Metadata;

#endregion

namespace JosephM.Record.Service
{
    /// <summary>
    ///     Implementation Of IRecordService To Interface To A A Standard CLR Object
    /// </summary>
    public class ObjectRecordService : RecordServiceBase
    {
        public ObjectRecordService(object objectToEnter)
            : this(objectToEnter, null, null)
        {
        }

        public ObjectRecordService(object objectToEnter, IRecordService lookupService,
            IDictionary<string, IEnumerable<string>> optionSetLimitedValues)
            : this(objectToEnter, lookupService, optionSetLimitedValues, null, null)
        {
        }

        public ObjectRecordService(object objectToEnter, IRecordService lookupService,
            IDictionary<string, IEnumerable<string>> optionSetLimitedValues, ObjectRecordService parentService, string parentServiceReference)
        {
            ParentServiceReference = parentServiceReference;
            ParentService = parentService;
            ObjectToEnter = objectToEnter;
            _lookupService = lookupService;
            OptionSetLimitedValues = optionSetLimitedValues;
            var objectTypeFieldMetadata = new List<FieldMetadata>();

            var type = ObjectToEnter.GetType();
            objectTypeFieldMetadata.AddRange(RecordMetadataFactory.GetClassFieldMetadata(type));
            FieldMetadata.Add(type.Name, objectTypeFieldMetadata);
            foreach (var field in objectTypeFieldMetadata.Where(f => f.FieldType == RecordFieldType.Enumerable))
            {
                //need to add the field metadata for any nested types
                var propertyType = type.GetProperty(field.SchemaName).PropertyType;
                var genericEnumerableType = propertyType.GetGenericArguments()[0];
                if (!FieldMetadata.ContainsKey(genericEnumerableType.Name))
                {
                    var metadata = RecordMetadataFactory.GetClassFieldMetadata(genericEnumerableType);
                    FieldMetadata.Add(genericEnumerableType.Name, metadata);
                }
            }
        }

        private Type ObjectType
        {
            get { return ObjectToEnter.GetType(); }
        }

        private readonly IRecordService _lookupService;

        public override IRecordService LookupService
        {
            get
            {
                return _lookupService;
            }
        }

        public IDictionary<string, IEnumerable<string>> OptionSetLimitedValues { get; private set; }

        public object ObjectToEnter { get; set; }

        private readonly Dictionary<string, IEnumerable<FieldMetadata>> _fieldMetadata =
            new Dictionary<string, IEnumerable<FieldMetadata>>();

        //DON'T HAVE THIS REFERENCE ITSELF OR WILL HAVE INFINITE LOOP!!
        private ObjectRecordService ParentService { get; set; }
        private string ParentServiceReference { get; set; }

        private Dictionary<string, IEnumerable<FieldMetadata>> FieldMetadata
        {
            get { return _fieldMetadata; }
        }

        public override IRecord GetFirst(string entityType, string fieldName, object fieldValue)
        {
            throw new NotImplementedException();
        }

        public override void Update(IRecord record)
        {
            throw new NotImplementedException();
        }

        public override IRecord NewRecord(string recordType)
        {
            //need to get the class constructor and instantiate
            var type = GetClassType(recordType);
            if (!type.HasParameterlessConstructor())
                throw new NullReferenceException(
                    string.Format("Type {0} Does Not Have A Parameterless Constructor To Create The Object", recordType));
            return new ObjectRecord(type.CreateFromParameterlessConstructor());
        }

        public Type GetClassType(string recordType)
        {
            Type type = null;
            if (recordType == ObjectType.Name)
                type = ObjectType;
            else
            {
                var fieldMetadata = GetFieldMetadata(ObjectType.Name);
                foreach (var metadata in fieldMetadata.Where(m => m.FieldType == RecordFieldType.Enumerable))
                {
                    if (((EnumerableFieldMetadata) metadata).EnumeratedType == recordType)
                    {
                        var propertyName = metadata.SchemaName;
                        type = ObjectType.GetProperty(propertyName).PropertyType.GetGenericArguments()[0];
                        break;
                    }
                }
            }
            if (type == null)
                throw new NullReferenceException(string.Format("Could Not Resolve Class Of Type {0}", recordType));
            return type;
        }
        


        public override string Create(IRecord record)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IRecord> GetLinkedRecords(string linkedEntityType, string entityTypeFrom,
            string linkedEntityLookup, string entityFromId)
        {
            var propertyValue = ObjectToEnter.GetPropertyValue(linkedEntityLookup);
            if (propertyValue == null)
                return new IRecord[0];
            var enumerable = ((IEnumerable) propertyValue);
            var objectList = new List<ObjectRecord>();
            foreach (var item in enumerable)
            {
                objectList.Add(new ObjectRecord(item));
            }
            return objectList;
        }

        public IEnumerable<FieldMetadata> GetFieldMetadata(string recordType)
        {
            if (FieldMetadata.ContainsKey(recordType))
                return FieldMetadata[recordType];
            throw new ArgumentOutOfRangeException("recordType",
                "No Field Metadata Has Been Created For Type " + recordType);
        }

        public override FieldMetadata GetFieldMetadata(string field, string recordType)
        {
            var fieldMetadata = GetFieldMetadata(recordType);
            if (fieldMetadata.Any(f => f.SchemaName == field))
                return GetFieldMetadata(recordType).First(fm => fm.SchemaName == field);
            throw new ArgumentOutOfRangeException("field",
                string.Format("{0} Field Metadata Does Not Contain Field With Name {1}", recordType, field));
        }

        public override void Update(IRecord record, IEnumerable<string> changedPersistentFields)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IRecord> RetrieveMultiple(string recordType, string searchString, int count)
        {
            throw new NotImplementedException();
        }

        public override string GetPrimaryField(string recordType)
        {
            return "ToString";
        }

        public override IEnumerable<string> GetFields(string recordType)
        {
            return GetFieldMetadata(recordType).Select(rm => rm.SchemaName);
        }

        public bool HasSetAccess(string fieldName, string recordType)
        {
            return GetPropertyInfo(fieldName, recordType).CanWrite;
        }

        public PropertyInfo GetPropertyInfo(string fieldName, string recordType)
        {
            return GetClassType(recordType).GetProperty(fieldName);
        }

        public IEnumerable<PropertyInfo> GetPropertyInfos(string recordType)
        {
            return GetClassType(recordType).GetProperties();
        }

        /// <summary>
        /// If Nullable Return The Nullable Type
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="recordType"></param>
        /// <returns></returns>
        public Type GetPropertyType(string fieldName, string recordType)
        {
            var type = GetClassType(recordType).GetProperty(fieldName).PropertyType;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GenericTypeArguments[0];
            return type;
        }

        public override IEnumerable<PicklistOption> GetPicklistKeyValues(string fieldName, string recordType)
        {
            return GetPicklistKeyValues(fieldName, recordType, null);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="recordType"></param>
        /// <param name="dependantValue">Various uses...</param>
        /// <returns></returns>
        public override IEnumerable<PicklistOption> GetPicklistKeyValues(string fieldName, string recordType,
            string dependantValue)
        {
            //if the property is type RecordType
            //then get the record types from the lookup service
            var fieldType = GetFieldType(fieldName, recordType);
            switch (fieldType)
            {
                case RecordFieldType.RecordType:
                {
                    var lookupService = (IRecordService)GetConnectionFor(fieldName, recordType, dependantValue);

                    if (OptionSetLimitedValues != null
                        && OptionSetLimitedValues.ContainsKey(fieldName)
                        && OptionSetLimitedValues[fieldName].Any())
                        return OptionSetLimitedValues[fieldName]
                            .Select(at => new RecordType(at, GetOptionLabel(at, fieldName, recordType)))
                            .OrderBy(rt => rt.Value)
                            .ToArray();
                    else
                    {
                        return lookupService
                            .GetAllRecordTypes()
                            .Select(r => new RecordType(r, lookupService.GetDisplayName(r)))
                            .ToArray();
                    }
                }
                case RecordFieldType.RecordField:
                {
                    if (dependantValue == null)
                        return new RecordField[0];
                    return LookupService
                        .GetFields(dependantValue)
                        .Select(f => new RecordField(f, LookupService.GetFieldLabel(f, dependantValue)))
                        .Where(f => !f.Value.IsNullOrWhiteSpace())
                        .OrderBy(f => f.Value)
                        .ToArray();
                }
                case RecordFieldType.Picklist:
                {
                    var type = GetPropertyType(fieldName, recordType);
                    var options = new List<PicklistOption>();
                    foreach (Enum item in type.GetEnumValues())
                        options.Add(new PicklistOption(item.ToString(), item.GetDisplayString()));
                    return options;
                }
            }
            throw new ArgumentOutOfRangeException(
                string.Format("GetPicklistOptions Not Implemented For Fiel Of Type {0} Field: {1} Type {2}", fieldType,
                    fieldName, recordType));
        }

        private object GetConnectionFor(string fieldName, string recordType, string reference)
        {
            //todo could improve performance by cache
            var props = GetPropertyInfos(ObjectType.Name);
            foreach (var prop in props)
            {
                var connectionFor = prop.GetCustomAttributes<ConnectionFor>()
                    .Where(c => c.PropertyPaths.Count() > 1 && c.PropertyPath1 == reference && c.PropertyPath2 == fieldName);
                if (connectionFor.Any())
                {
                    var value = ObjectToEnter.GetPropertyValue(prop.Name);
                    if (value != null)
                    {
                        //todo improve error messages
                        var connectionFieldType = value.GetType();
                        var serviceConnectionAttr = connectionFieldType.GetCustomAttribute<ServiceConnection>(true);
                        if(serviceConnectionAttr == null)
                            throw new NullReferenceException(string.Format("Type {0} Does Not Have {1} Attribute", connectionFieldType.Name, typeof(ServiceConnection).Name));
                        if (!serviceConnectionAttr.ServiceType.HasConstructorFor(connectionFieldType))
                            throw new NullReferenceException(string.Format("Type {0} Does Not Have Constructor For Type {1}", serviceConnectionAttr.ServiceType.Name, value.GetType().Name));
                        var service = serviceConnectionAttr.ServiceType.CreateFromConstructorFor(value);
                        return service;
                    }
                }
            }
            if (ParentService != null)
            {
                return ParentService.GetConnectionFor(fieldName, recordType, reference);
            }
            return LookupService;
        }

        public override ParseFieldResponse ParseFieldRequest(ParseFieldRequest parseFieldRequest)
        {
            var fieldType = GetFieldType(parseFieldRequest.FieldName, parseFieldRequest.RecordType);
            if (fieldType == RecordFieldType.Picklist)
            {
                var parsedValue = parseFieldRequest.Value;
                if (parseFieldRequest.Value is PicklistOption)
                    parsedValue =
                        Enum.Parse(
                            GetPropertyType(parseFieldRequest.FieldName, parseFieldRequest.RecordType),
                            ((PicklistOption) parseFieldRequest.Value).Key);
                return new ParseFieldResponse(parsedValue);
            }
            if (fieldType == RecordFieldType.Integer && parseFieldRequest.Value is string)
            {
                if (((string) parseFieldRequest.Value).IsNullOrWhiteSpace())
                    return
                        new ParseFieldResponse(IsNotNullable(parseFieldRequest.FieldName, parseFieldRequest.RecordType)
                            ? 0
                            : (int?) null);
                else
                    return new ParseFieldResponse(int.Parse((string) parseFieldRequest.Value));
            }
            return base.ParseFieldRequest(parseFieldRequest);
        }

        public override string GetOptionLabel(string optionKey, string field, string recordType)
        {
            //if the property is type RecordType
            //then get the record types from the lookup service
            if (ObjectType.GetProperty(field).PropertyType == typeof (RecordType))
            {
                return LookupService.GetDisplayName(optionKey);
            }
            return base.GetOptionLabel(optionKey, field, recordType);
        }

        public IEnumerable<PropertyValidator> GetValidatorAttributes(string fieldName, string recordType)
        {
            return GetClassType(recordType).GetValidatorAttributes(fieldName);
        }

        public override bool IsWritable(string fieldName, string recordType)
        {
            return GetPropertyInfo(fieldName, recordType).GetSetMethod() != null;
        }

        public override IEnumerable<ViewMetadata> GetViews(string recordType)
        {
            //very similar logic in form get grid metadata
            var viewFields = new List<ViewField>();
            var type = GetClassType(recordType);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    //these initial values repeated
                    var viewField = new ViewField(propertyInfo.Name, int.MaxValue, 200);
                    var orderAttribute = propertyInfo.GetCustomAttribute<DisplayOrder>();
                    if (orderAttribute != null)
                        viewField.Order = orderAttribute.Order;
                    var widthAttribute = propertyInfo.GetCustomAttribute<GridWidth>();
                    if (widthAttribute != null)
                        viewField.Width = widthAttribute.Width;
                    viewFields.Add(viewField);
                }
            }
            return new [] { new ViewMetadata(viewFields) { ViewType = ViewType.LookupView} };
        }

        public override bool IsNotNullable(string fieldName, string recordType)
        {
            var propertyInfo = GetPropertyInfo(fieldName, recordType);
            var type = propertyInfo.PropertyType;
            if (type == typeof (int))
                return true;
            return false;
        }
    }
}