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
using JosephM.Record.Extentions;
using JosephM.Core.AppConfig;
using JosephM.Core.Service;
using JosephM.Record.Attributes;
using JosephM.Record.Query;

#endregion

namespace JosephM.Record.Service
{
    /// <summary>
    ///     Implementation Of IRecordService To Interface To A A Standard CLR Object
    /// </summary>
    public class ObjectRecordService : RecordServiceBase
    {
        public ObjectRecordService(object objectToEnter, IResolveObject objectResolver, IDictionary<string, Type> objectTypeMaps = null)
            : this(objectToEnter, null, null, objectResolver, objectTypeMaps)
        {
        }

        public ObjectRecordService(object objectToEnter, IRecordService lookupService,
            IDictionary<string, IEnumerable<string>> optionSetLimitedValues, IResolveObject objectResolver, IDictionary<string, Type> objectTypeMaps = null)
            : this(objectToEnter, lookupService, optionSetLimitedValues, null, null, objectResolver, objectTypeMaps)
        {
        }

        public ObjectRecordService(object objectToEnter, IRecordService lookupService,
            IDictionary<string, IEnumerable<string>> optionSetLimitedValues, ObjectRecordService parentService, string parentServiceReference, IResolveObject objectResolver, IDictionary<string, Type> objectTypeMaps = null)
        {
            ObjectResolver = objectResolver;
            ParentServiceReference = parentServiceReference;
            ParentService = parentService;
            ObjectToEnter = objectToEnter;
            _lookupService = lookupService;
            OptionSetLimitedValues = optionSetLimitedValues;
            ObjectTypeMaps = objectTypeMaps;
            var objectTypeFieldMetadata = new List<FieldMetadata>();

            var type = ObjectToEnter.GetType();
            objectTypeFieldMetadata.AddRange(RecordMetadataFactory.GetClassFieldMetadata(type, ObjectTypeMaps));
            FieldMetadata.Add(type.AssemblyQualifiedName, objectTypeFieldMetadata);
            foreach (var field in objectTypeFieldMetadata.Where(f => f.FieldType == RecordFieldType.Enumerable))
            {
                //need to add the field metadata for any nested types
                var asEnumerable = (EnumerableFieldMetadata)field;
                if (!FieldMetadata.ContainsKey(asEnumerable.EnumeratedTypeQualifiedName))
                {
                    var metadata = RecordMetadataFactory.GetClassFieldMetadata(GetClassType(asEnumerable.EnumeratedTypeQualifiedName), ObjectTypeMaps);
                    FieldMetadata.Add(asEnumerable.EnumeratedTypeQualifiedName, metadata);
                }

                //if the property is an interface we also want to load the metadata for any instance types
                var propertyValue = ObjectToEnter.GetPropertyValue(field.SchemaName);
                if (propertyValue != null)
                {
                    var enumerable = ((IEnumerable)propertyValue);
                    foreach(var item in enumerable)
                    {
                        var instanceType = item.GetType();
                        if (!FieldMetadata.ContainsKey(instanceType.AssemblyQualifiedName))
                        {
                            var metadata = RecordMetadataFactory.GetClassFieldMetadata(instanceType, ObjectTypeMaps);
                            FieldMetadata.Add(instanceType.AssemblyQualifiedName, metadata);
                        }
                    }
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

        private IResolveObject ObjectResolver { get; set; }

        private readonly Dictionary<string, IEnumerable<FieldMetadata>> _fieldMetadata =
            new Dictionary<string, IEnumerable<FieldMetadata>>();

        //DON'T HAVE THIS REFERENCE ITSELF OR WILL HAVE INFINITE LOOP!!
        private ObjectRecordService ParentService { get; set; }
        private string ParentServiceReference { get; set; }

        private Dictionary<string, IEnumerable<FieldMetadata>> FieldMetadata
        {
            get { return _fieldMetadata; }
        }

        public IDictionary<string, Type> ObjectTypeMaps { get; private set; }

        public override IRecord NewRecord(string recordType)
        {
            //need to get the class constructor and instantiate
            var type = GetClassType(recordType);
            if (!type.HasParameterlessConstructor())
                throw new NullReferenceException(
                    string.Format("Type {0} Does Not Have A Parameterless Constructor To Create The Object", recordType));
            return new ObjectRecord(type.CreateFromParameterlessConstructor());
        }

        public override IRecord Get(string recordType, string id)
        {
            throw new NotImplementedException();
        }

        public Type GetClassType(string recordType)
        {
            Type type = null;
            if (recordType == ObjectType.AssemblyQualifiedName)
                type = ObjectType;
            else
            {
                var fieldMetadata = GetFieldMetadata(ObjectType.AssemblyQualifiedName);
                foreach (var metadata in fieldMetadata.Where(m => m.FieldType == RecordFieldType.Enumerable))
                {
                    if (((EnumerableFieldMetadata)metadata).EnumeratedTypeQualifiedName == recordType)
                    {
                        var propertyName = metadata.SchemaName;
                        if (ObjectTypeMaps != null && ObjectTypeMaps.ContainsKey(propertyName))
                        {
                            type = ObjectTypeMaps[propertyName];
                        }
                        else
                            type = ObjectType.GetProperty(propertyName).PropertyType.GetGenericArguments()[0];

                        break;
                    }
                    else
                    {
                        var propertyValue = ObjectToEnter.GetPropertyValue(metadata.SchemaName);
                        if (propertyValue != null)
                        {
                            var enumerable = ((IEnumerable)propertyValue);
                            foreach (var item in enumerable)
                            {
                                var instanceType = item.GetType();
                                if (instanceType.AssemblyQualifiedName == recordType)
                                {
                                    type = instanceType;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (type == null)
            {
                type = Type.GetType(recordType);
            }

            return type;
        }



        public override string Create(IRecord record, IEnumerable<string> fields)
        {
            throw new NotImplementedException();
        }

        public override void Delete(string recordType, string id)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IRecord> GetFirstX(string type, int x, IEnumerable<string> fields, IEnumerable<Condition> conditions, IEnumerable<SortExpression> sort)
        {
            var query = new QueryDefinition(type);
            query.Top = x;
            query.RootFilter = new Filter();
            if (conditions != null)
                query.RootFilter.Conditions = conditions.ToList();
            if (sort != null)
                query.Sorts = sort.ToList();
            return RetreiveAll(query);
        }

        public override IsValidResponse VerifyConnection()
        {
            var response = new IsValidResponse();
            if(ObjectToEnter == null)
                response.AddInvalidReason("The object to enter is null");
            return response;
        }

        public override IEnumerable<IRecord> GetLinkedRecords(string linkedEntityType, string entityTypeFrom,
            string linkedEntityLookup, string entityFromId)
        {
            return GetPropertyObjectsAsRecords(linkedEntityLookup);
        }

        private IEnumerable<IRecord> GetPropertyObjectsAsRecords(string linkedEntityLookup)
        {
            var propertyValue = ObjectToEnter.GetPropertyValue(linkedEntityLookup);
            if (propertyValue == null)
                return new IRecord[0];
            var enumerable = ((IEnumerable)propertyValue);
            var objectList = new List<ObjectRecord>();
            foreach (var item in enumerable)
            {
                objectList.Add(new ObjectRecord(item));
            }
            return objectList;
        }

        public override IEnumerable<IRecord> RetreiveAll(QueryDefinition query)
        {
            //okay this is initially implemented purely for querying an IEnumerable property of the primary object
            var objects = new List<IRecord>();
            foreach (var property in ObjectType.GetProperties())
            {
                if (property.PropertyType.GenericTypeArguments.Count() > 0
                    && property.PropertyType.GenericTypeArguments[0].AssemblyQualifiedName == query.RecordType)
                {
                    objects.AddRange(GetPropertyObjectsAsRecords(property.Name));
                }
            }
            if (!query.IsQuickFind)
            {
                if (query.RootFilter.SubFilters.Any()
                    || query.RootFilter.Conditions.Count > 1
                    || (query.RootFilter.Conditions.Count == 1 && query.RootFilter.Conditions.First().ConditionType != ConditionType.Equal))
                {
                    throw new NotImplementedException("Queries With Conditions Are Not Implemented");
                }
            }
            else
            {
                if (query.QuickFindText != null)
                {
                    var quickFindToLower = query.QuickFindText.ToLower();
                    var props = GetPropertyInfos(query.RecordType);
                    var quickFinds = props
                        .Where(p => p.GetCustomAttribute<QuickFind>() != null)
                        .Select(p => p.Name)
                        .ToArray();
                    if(!quickFinds.Any())
                    {
                        quickFinds = GetFieldMetadata(query.RecordType)
                            .Where(p => p.Searchable)
                            .Select(p => p.SchemaName)
                            .ToArray();
                    }


                    objects = objects
                        .Where(o =>
                        {
                            var instance = ((ObjectRecord)o).Instance;
                            return quickFinds
                            .Any(p =>
                            {
                                var propValue = instance.GetPropertyValue(p);
                                return propValue != null && propValue.ToString().ToLower().Contains(quickFindToLower);
                            });
                        })
                        .ToList();
                }
            }
            var newSorts = new List<SortExpression>(query.Sorts);
            if (!newSorts.Any())
            {
                newSorts.OrderBy(o => o.ToString()).ToList();
            }
            else
            {
                newSorts.Reverse();
                foreach (var sort in newSorts.Take(1))
                {
                    var comparer = new ObjectComparer(sort.FieldName);
                    objects.Sort(comparer);
                    if (sort.SortType == SortType.Descending)
                        objects.Reverse();
                }
            }
            objects = query.Top > 0
                ? objects.Take(query.Top).ToList()
                : objects;
            return objects;
        }

        public override IEnumerable<IFieldMetadata> GetFieldMetadata(string recordType)
        {
            lock (_lockObject)
            {
                if (!FieldMetadata.ContainsKey(recordType))
                {
                    FieldMetadata.Add(recordType, RecordMetadataFactory.GetClassFieldMetadata(Type.GetType(recordType)));
                    var fieldMetadata = FieldMetadata[recordType];
                }
                return FieldMetadata[recordType];
            }
        }

        public override void Update(IRecord record, IEnumerable<string> changedPersistentFields)
        {
            throw new NotImplementedException();
        }

        public override IRecordTypeMetadata GetRecordTypeMetadata(string recordType)
        {
            var type = GetClassType(recordType);
            return new ObjectRecordMetadata()
            {
                SchemaName = recordType,
                DisplayName = type.GetDisplayName(),
                CollectionName = type.GetDisplayName() + "s"
            };
        }

        public bool HasSetAccess(string fieldName, string recordType)
        {
            return GetPropertyInfo(fieldName, recordType).CanWrite;
        }

        public PropertyInfo GetPropertyInfo(string fieldName, string recordType)
        {
            var properties = GetPropertyInfos(recordType);
            foreach (var property in properties)
                if (property.Name == fieldName)
                    return property;
            return null;
        }

        public IEnumerable<PropertyInfo> GetPropertyInfos(string recordType)
        {
            var classType = GetClassType(recordType);
            return classType.GetAllPropertyInfos();
        }


        /// <summary>
        /// If Nullable Return The Nullable Type
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="recordType"></param>
        /// <returns></returns>
        public Type GetPropertyType(string fieldName, string recordType)
        {
            var type = GetPropertyInfo(fieldName, recordType).PropertyType;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GenericTypeArguments[0];
            return type;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="recordType"></param>
        /// <param name="dependantValue">Various uses...</param>
        /// <param name="record">The record containing the field we are getting the options for</param>
        /// <returns></returns>
        public override IEnumerable<PicklistOption> GetPicklistKeyValues(string fieldName, string recordType, string dependantValue, IRecord record)
        {
            //if the property is type RecordType
            //then get the record types from the lookup service
            var fieldType = this.GetFieldType(fieldName, recordType);
            switch (fieldType)
            {
                case RecordFieldType.RecordType:
                    {
                        var lookupService = GetLookupService(fieldName, recordType, dependantValue, record);

                        if (OptionSetLimitedValues != null
                            && OptionSetLimitedValues.ContainsKey(fieldName)
                            && OptionSetLimitedValues[fieldName].Any())
                            return OptionSetLimitedValues[fieldName]
                                .Select(at => new RecordType(at, LookupService.GetRecordTypeMetadata(at).DisplayName))
                                .OrderBy(rt => rt.Value)
                                .ToArray();
                        else
                        {
                            return lookupService == null
                                ? new RecordType[0]
                                : lookupService.GetAllRecordTypes()
                                .Select(r => new RecordType(r, lookupService.GetRecordTypeMetadata(r).DisplayName))
                                .ToArray();
                        }
                    }
                case RecordFieldType.RecordField:
                    {
                        if (dependantValue == null)
                            return new RecordField[0];

                        var options = new List<PicklistOption>();
                        var type = dependantValue;
                        string parentReference = null;
                        if (dependantValue != null && dependantValue.Contains(':'))
                        {
                            type = ((string)dependantValue).Split(':').ElementAt(0);
                            parentReference = ((string)dependantValue).Split(':').ElementAt(1);
                        }
                        var lookupService = GetLookupService(fieldName, recordType, parentReference, record);

                        var allFieldsMetadata = type.IsNullOrWhiteSpace() || lookupService == null
                            ? new IFieldMetadata[0]
                            : lookupService
                                .GetFieldMetadata(type);
                        var propertyInfo = GetPropertyInfo(fieldName, recordType);
                        var lookupConditions = propertyInfo.GetCustomAttributes<LookupCondition>();
                        foreach (var condition in lookupConditions.Select(lc => lc.ToCondition()))
                        {
                            allFieldsMetadata =
                                allFieldsMetadata.Where(f => MeetsCondition(f, condition)).ToArray();
                        }
                        var onlyInclude = OptionSetLimitedValues == null || !OptionSetLimitedValues.ContainsKey(fieldName)
                            ? null
                            : OptionSetLimitedValues[fieldName];

                        return allFieldsMetadata
                            .Select(f => new RecordField(f.SchemaName, f.DisplayName))
                            .Where(f => !f.Value.IsNullOrWhiteSpace())
                            .Where(f => onlyInclude == null || onlyInclude.Contains(f.Key))
                            .OrderBy(f => f.Value)
                            .ToArray();
                    }
                case RecordFieldType.Picklist:
                    {
                        var type = GetPropertyType(fieldName, recordType);
                        var options = new List<PicklistOption>();
                        if (type.Name == "IEnumerable`1")
                            type = type.GenericTypeArguments[0];
                        foreach (Enum item in type.GetEnumValues())
                        {
                            var enumMember = item.GetType().GetMember(item.ToString()).First();
                            var validForFieldAttribute = enumMember.GetCustomAttribute<ValidForFieldTypes>();
                            if (validForFieldAttribute == null || dependantValue == null)
                            {
                                options.Add(PicklistOption.EnumToPicklistOption(item));
                            }
                            else
                            {
                                if(validForFieldAttribute.FieldTypes.Contains(dependantValue.ParseEnum<RecordFieldType>()))
                                {
                                    options.Add(PicklistOption.EnumToPicklistOption(item));
                                }
                            }
                        }
                        var propertyInfo = GetPropertyInfo(fieldName, recordType);
                        var limitAttribute = propertyInfo.GetCustomAttribute<LimitPicklist>();
                        if (limitAttribute != null)
                        {
                            options =
                                options.Where(
                                    o =>
                                        limitAttribute.ToInclude.Select(kv => Convert.ToInt32(kv).ToString())
                                            .Contains(o.Key))
                                            .ToList();
                        }
                        return options;
                    }
            }
            return null;
        }

        public Condition GetLookupConditionFors(string fieldName, string recordType, string reference, IRecord record)
        {

            if (record != null && (!(record is ObjectRecord)))
                throw new TypeLoadException(string.Format("Expected {0} Of Type {1}", typeof(IRecord).Name, typeof(ObjectRecord).Name));

            var resolveAttributeReference = GetReferencingAttribute<LookupConditionFor>("TargetProperty", fieldName, recordType, reference, record);
            if (resolveAttributeReference != null && resolveAttributeReference.ReferencingProperty != null)
            {
                var attr2 = (LookupConditionFor) resolveAttributeReference.ReferencingAttribute;
                var objectValue =
                    resolveAttributeReference.ObjectContaining.GetPropertyValue(
                        resolveAttributeReference.ReferencingProperty.Name);
                if (objectValue != null)
                {
                    return new Condition(attr2.FieldName, ConditionType.Equal, objectValue);
                }
            }
            //check for reference in parent form
            else if (ParentService != null)
            {
                return ParentService.GetLookupConditionFors(reference + "." + fieldName, ObjectType.Name, ParentServiceReference, new ObjectRecord(ObjectToEnter));
            }
            return null;
        }

        private object _lockObject = new object();
        private Dictionary<object, IRecordService> _serviceConnections = new Dictionary<object, IRecordService>();

        public override IRecordService GetLookupService(string fieldName, string recordType, string reference, IRecord record)
        {

            if (record != null && (!(record is ObjectRecord)))
                throw new TypeLoadException(string.Format("Expected {0} Of Type {1}", typeof(IRecord).Name, typeof(ObjectRecord).Name));

            var resolveAttributeReference = GetReferencingAttribute<ConnectionFor>(nameof(ConnectionFor.Property), fieldName, recordType, reference, record);
            if (resolveAttributeReference != null && resolveAttributeReference.ReferencingProperty != null)
            {
                return GetLookupServiceForConnectionFor(resolveAttributeReference);
            }
            //check for reference inj parent form
            if (ParentService != null)
            {
                return ParentService.GetLookupService(reference + "." + fieldName, ObjectType.Name, ParentServiceReference, new ObjectRecord(ObjectToEnter));
            }

            return LookupService;
        }

        private GetReferencingAttributeResponse GetReferencingAttribute<T>(string attributeFieldNameProperty, string fieldName, string recordType, string reference, IRecord record)
                where T : Attribute
        {
            //needed to implement several inspections to get referencing attributes

            //may be object in a grid where have to check other properties for object in that row
            //or property of object in main form

            //need to inspect one of several objects with one of several combinations of the reference and field name

            var objectsToInspect = new List<object>();
            objectsToInspect.Add(ObjectToEnter);
            if (record != null && ((ObjectRecord)record).Instance != ObjectToEnter)
                objectsToInspect.Add(((ObjectRecord)record).Instance);

            var referencesToResolve = new List<string>();
            referencesToResolve.Add(fieldName);
            if (reference != null)
            {
                var splitReference = reference.Split('.');
                for(var index = splitReference.Length - 1; index >= 0; index--)
                {
                    referencesToResolve.Add(string.Join(".", splitReference.Skip(index)) + "." + fieldName);
                    for (var index2 = splitReference.Length - 1; index2 >= index; index2--)
                    {
                        referencesToResolve.Add(string.Join(".", splitReference.Skip(index).Take((index2 - index) + 1)));
                    }
                }
            }

            foreach(var objectToInspect in objectsToInspect)
            {
                foreach(var referenceToResolve in referencesToResolve)
                {
                    var response = GetReferencingAttribute<T>(attributeFieldNameProperty, referenceToResolve, objectToInspect);
                    if (response != null)
                        return response;
                }
            }
            return null;
        }

        private GetReferencingAttributeResponse GetReferencingAttribute<T>(string attributeFieldNameProperty, string fieldName, object objectToEnter)
            where T : Attribute
        {
            var props = GetPropertyInfos(objectToEnter.GetType().AssemblyQualifiedName);
            foreach (var prop in props)
            {
                var connectionsFor = prop.GetCustomAttributes<T>(true)
                    .Where(c => (string)c.GetPropertyValue(attributeFieldNameProperty) == fieldName)
                    .ToArray();
                if (connectionsFor.Any())
                {
                    return new GetReferencingAttributeResponse()
                    {
                        ObjectContaining = objectToEnter,
                        ReferencingProperty = prop,
                        ReferencingAttribute = connectionsFor.First()
                    };
                }
            }
            return null;
        }

        private class GetReferencingAttributeResponse
        {
            public object ObjectContaining { get; set; }
            public PropertyInfo ReferencingProperty { get; set; }
            public Attribute ReferencingAttribute { get; set; }
        }

        private IRecordService GetLookupServiceForConnectionFor(
            GetReferencingAttributeResponse resolvedAttributeReference)
        {
            var prop = resolvedAttributeReference.ReferencingProperty;
            var attr = (ConnectionFor)resolvedAttributeReference.ReferencingAttribute;
            var value = resolvedAttributeReference.ObjectContaining.GetPropertyValue(prop.Name);
            lock (_lockObject)
            {
                if (value != null)
                {
                    if (_serviceConnections.ContainsKey(value))
                        return _serviceConnections[value];

                    if (attr is LookupConnectionFor)
                    {
                        if (!(value is Lookup))
                            throw new Exception(
                                string.Format(
                                    "Value is required to be of type {0} for {1} attribute. Actual type is {2}. The property name is"
                                    , typeof (Lookup).Name, value.GetType().Name, prop.Name));
                        var lookup = (Lookup) value;
                        var lookupLookupService = GetLookupService(prop.Name, resolvedAttributeReference.ObjectContaining.GetType().Name, null,
                            null);
                        var lookupConnectionFor = (LookupConnectionFor) attr;
                        var parsedService = TypeLoader.LoadService(lookup, lookupLookupService, lookupConnectionFor,
                            ObjectResolver);
                        _serviceConnections.Add(value, parsedService);
                        return parsedService;
                    }

                    var serviceType = attr.ServiceType;
                    var connectionFieldType = value.GetType();
                    if (serviceType == null)
                    {
                        var serviceConnectionAttr =
                            connectionFieldType.GetCustomAttribute<ServiceConnection>(true);
                        if (serviceConnectionAttr == null)
                            throw new NullReferenceException(
                                string.Format(
                                    "The Property {0} Is Specified With A {1} Attribute However It's Type {2} Does Not Have The {3} Attribute Record To Create The {4}",
                                    prop.Name, typeof (ConnectionFor).Name, connectionFieldType.Name,
                                    typeof (ServiceConnection).Name, typeof (IRecordService).Name));
                        serviceType = serviceConnectionAttr.ServiceType;
                    }
                    var service = TypeLoader.LoadServiceForConnection(value, serviceType);
                    _serviceConnections.Add(value, service);
                    return service;
                }
            }
            return null;
        }

        //private IRecordService GetLookupServiceForConnectionFor(string fieldName, object objectToEnter)
        //{
        //    var referencingResponse = GetReferencingAttribute<ConnectionFor>(nameof(ConnectionFor.Property), fieldName, objectToEnter);
        //    if (referencingResponse != null && referencingResponse.ReferencingProperty != null)
        //    {
        //        var prop = referencingResponse.ReferencingProperty;
        //        var attr = (ConnectionFor)referencingResponse.ReferencingAttribute ;
        //        var value = objectToEnter.GetPropertyValue(prop.Name);
        //        lock (_lockObject)
        //        {
        //            if (value != null)
        //            {
        //                if (_serviceConnections.ContainsKey(value))
        //                    return _serviceConnections[value];

        //                if (attr is LookupConnectionFor)
        //                {
        //                    if (!(value is Lookup))
        //                        throw new Exception(
        //                            string.Format(
        //                                "Value is required to be of type {0} for {1} attribute. Actual type is {2}. The property name is"
        //                                , typeof (Lookup).Name, value.GetType().Name, prop.Name));
        //                    var lookup = (Lookup) value;
        //                    var lookupLookupService = GetLookupService(prop.Name, objectToEnter.GetType().Name, null,
        //                        null);
        //                    var lookupConnectionFor = (LookupConnectionFor)attr;
        //                    var parsedService = TypeLoader.LoadService(lookup, lookupLookupService, lookupConnectionFor,
        //                        ObjectResolver);
        //                    _serviceConnections.Add(value, parsedService);
        //                    return parsedService;
        //                }

        //                var serviceType = attr.ServiceType;
        //                var connectionFieldType = value.GetType();
        //                if (serviceType == null)
        //                {
        //                    var serviceConnectionAttr =
        //                        connectionFieldType.GetCustomAttribute<ServiceConnection>(true);
        //                    if (serviceConnectionAttr == null)
        //                        throw new NullReferenceException(
        //                            string.Format(
        //                                "The Property {0} Is Specified With A {1} Attribute However It's Type {2} Does Not Have The {3} Attribute Record To Create The {4}",
        //                                prop.Name, typeof (ConnectionFor).Name, connectionFieldType.Name,
        //                                typeof (ServiceConnection).Name, typeof (IRecordService).Name));
        //                    serviceType = serviceConnectionAttr.ServiceType;
        //                }
        //                var service = TypeLoader.LoadServiceForConnection(value, serviceType);
        //                _serviceConnections.Add(value, service);
        //                return service;
        //            }
        //        }
        //    }
        //    return null;
        //}

        public override object ParseField(string fieldName, string recordType, object value)
        {
            var fieldType = this.GetFieldType(fieldName, recordType);
            if (fieldType == RecordFieldType.Picklist)
            {
                if (value is PicklistOption)
                    value =
                        Enum.Parse(
                            GetPropertyType(fieldName, recordType),
                            ((PicklistOption)value).Key);
                if (value is IEnumerable<PicklistOption>)
                {
                    var enumType = GetPropertyType(fieldName, recordType).GenericTypeArguments[0];
                    value =
                        ((IEnumerable<PicklistOption>)value).Select(p => Enum.Parse(
                            enumType,
                            (p.Key))).ToArray();
                    value = enumType.ToNewTypedEnumerable((IEnumerable<object>)value);
                }
                return value;
            }
            if (fieldType == RecordFieldType.Integer && value is string)
            {
                if (((string)value).IsNullOrWhiteSpace())
                    return
                        this.GetFieldMetadata(fieldName, recordType).IsNonNullable
                            ? 0
                            : (int?)null;
                else
                    return int.Parse((string)value);
            }
            if (fieldType == RecordFieldType.Money)
            {
                if (value is decimal)
                    value = new Money((decimal)value);
                return value;
            }
            return base.ParseField(fieldName, recordType, value);
        }

        public IEnumerable<PropertyValidator> GetValidatorAttributes(string fieldName, string recordType)
        {
            return GetClassType(recordType).GetValidatorAttributes(fieldName);
        }

        public override IEnumerable<ViewMetadata> GetViews(string recordType)
        {
            var viewFields = new List<ViewField>();
            var allObjects = RetreiveAll(new QueryDefinition(recordType));
            var areExplicitGridFields = false;
            foreach (var propertyInfo in GetPropertyInfos(recordType))
            {
                var hiddenAttribute = propertyInfo.GetCustomAttribute<HiddenAttribute>();
                if (propertyInfo.CanRead && hiddenAttribute == null)
                {
                    var gridFieldAttribute = propertyInfo.GetCustomAttribute<GridField>();
                    if(gridFieldAttribute != null && !areExplicitGridFields)
                    {
                        areExplicitGridFields = true;
                        viewFields.Clear();
                    }
                    //so lets not display a field if it is read only and is out of contxt for all records
                    if (!propertyInfo.CanWrite && allObjects.All(o => {
                        var instance = ((ObjectRecord)o).Instance;
                        return instance.GetPropertyValue(propertyInfo.Name) == null || !instance.IsInContext(propertyInfo.Name);
                    }))
                        continue;
                    if (areExplicitGridFields && gridFieldAttribute == null)
                        continue;
                    //these initial values repeated
                    var viewField = new ViewField(propertyInfo.Name,10000, 200);
                    var orderAttribute = propertyInfo.GetCustomAttribute<DisplayOrder>();
                    if (orderAttribute != null)
                        viewField.Order = orderAttribute.Order;
                    var widthAttribute = propertyInfo.GetCustomAttribute<GridWidth>();
                    if (widthAttribute != null)
                        viewField.Width = widthAttribute.Width;
                    viewFields.Add(viewField);
                }
            }
            return new[] { new ViewMetadata(viewFields.OrderBy(o => o.Order).ToArray()) { ViewType = ViewType.LookupView } };
        }

        private bool MeetsCondition(object instance, Condition condition)
        {
            var propertyValue = instance.GetPropertyValue(condition.FieldName);
            switch (condition.ConditionType)
            {
                case ConditionType.Equal:
                    {
                        return condition.Value.Equals(propertyValue);
                    }
                case ConditionType.In:
                    {
                        var enumerable = condition.Value as IEnumerable;
                        if (enumerable == null)
                            throw new Exception(string.Format("{0} must be {1} for {2} {3}", "Value", typeof(IEnumerable).Name, typeof(Condition).Name, condition.ConditionType));
                        foreach (var item in enumerable)
                        {
                            if (item.Equals(instance.GetPropertyValue(condition.FieldName)))
                                return true;
                        }
                        return false;
                    }
            }
            throw new NotImplementedException(string.Format("{0} not implemented", condition.ConditionType));
        }

        public class ObjectComparer : IComparer<IRecord>
        {
            private string PropertyName { get; set; }
            public ObjectComparer(string propertyName)
            {
                PropertyName = propertyName;
            }

            public int Compare(IRecord x, IRecord y)
            {
                var value1 = x.GetField(PropertyName);
                var value2 = y.GetField(PropertyName);
                if (value1 == null && value2 == null)
                {
                    return 0;
                }
                if (value2 == null)
                {
                    return 1;
                }
                else if (!(value1 is Enum) && value1 is IComparable)
                {
                    return ((IComparable)value1).CompareTo(value2);
                }
                var sortString1 = value1.ToString();
                var sortString2 = value2.ToString();
                if (value1 is Enum)
                    sortString1 = ((Enum)value1).GetDisplayString();
                if (value2 is Enum)
                    sortString2 = ((Enum)value2).GetDisplayString();
                return String.Compare(sortString1, sortString2, StringComparison.Ordinal);
            }
        }
    }
}