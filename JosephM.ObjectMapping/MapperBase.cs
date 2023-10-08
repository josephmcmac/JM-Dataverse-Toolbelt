using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;

namespace JosephM.ObjectMapping
{
    public class MapperBase
    {
        public void Map<TFrom, TTo>(TFrom from, TTo to)
        {
            var fromType = from.GetType();
            var toType = to.GetType();
            foreach (var targetProperty in toType.GetWritableProperties())
            {
                MapPropertyValue(targetProperty.Name, from, to, fromType, toType);
            }
        }

        public virtual void MapPropertyValue<TFrom, TTo>(string propertyName, TFrom from, TTo to, Type fromType, Type toType)
        {
            var mapFromProperty = GetPropertyMappedFrom(propertyName, fromType);
            if (mapFromProperty != null)
            {
                MapPropertyValue(to, toType.GetProperty(propertyName), from,
                    fromType.GetProperty(mapFromProperty));
            }
        }

        public string GetPropertyMappedFrom(string propertyName, Type typeFrom)
        {
            var mapFromProperty = propertyName;
            if (PropertyMaps.Any(pm => pm.Type2Property == propertyName
                                       && pm.Type1 == typeFrom))
                mapFromProperty = PropertyMaps.First(pm => pm.Type2Property == propertyName
                                                           && pm.Type1 == typeFrom).Type1Property;
            else if (PropertyMaps.Any(pm => pm.Type1Property == propertyName
                                            && pm.Type2 == typeFrom))
                mapFromProperty = PropertyMaps.First(pm => pm.Type1Property == propertyName
                                                           && pm.Type2 == typeFrom).Type2Property;
            return mapFromProperty;
        }

        public void MapPropertyValue(object to, PropertyInfo propertyTo, object from, PropertyInfo propertyFrom)
        {
            if (propertyFrom != null && propertyFrom.CanRead && propertyTo.CanWrite)
            {
                var fromValue = from.GetPropertyValue(propertyFrom.Name);
                var mappedValue = fromValue;
                if (fromValue != null)
                {
                    var toType = propertyTo.PropertyType;
                    if (toType.Name == "Nullable`1")
                        toType = toType.GetGenericArguments()[0];
                    if (toType == typeof (string))
                    {
                        if (propertyFrom.PropertyType == typeof (Guid))
                            mappedValue = fromValue.Equals(Guid.Empty) ? "" : fromValue.ToString();
                        else if (propertyFrom.PropertyType == typeof (Password))
                            mappedValue = ((Password) fromValue).GetRawPassword();
                        else
                            mappedValue = fromValue.ToString();
                    }
                    else if (toType.IsEnum)
                    {
                        mappedValue = Enum.Parse(toType, fromValue.ToString());
                    }
                    else if (toType == typeof (Guid))
                    {
                        var stringValue = mappedValue.ToString();
                        mappedValue = stringValue.IsNullOrWhiteSpace()
                            ? Guid.Empty
                            : new Guid(fromValue.ToString());
                    }
                    else if (toType == typeof (Password))
                    {
                        if (propertyFrom.PropertyType == typeof (Password))
                        {
                            var password = (Password) fromValue;
                            mappedValue = new Password(password.GetRawPassword(), false, password.EncryptPassword);
                        }
                        else
                            mappedValue = new Password(fromValue.ToString(), false, true);
                    }
                    else if (propertyFrom.PropertyType.IsIEnumerableOfT())
                    {
                        var genericType = propertyFrom.PropertyType.GetGenericArguments()[0];
                        //map each object into a new IEnumerableObject
                        var enumerable = (IEnumerable) fromValue;
                        var objectList = new List<object>();
                        var classSelfMapper = new ClassSelfMapper();
                        foreach (var instance in enumerable)
                        {
                            objectList.Add(classSelfMapper.Map(instance));
                        }
                        mappedValue = genericType.ToNewTypedEnumerable(objectList);
                    }
                    else if (toType.HasParameterlessConstructor())
                    {
                        var classSelfMapper = new ClassSelfMapper();
                        mappedValue = classSelfMapper.Map(fromValue);
                    }
                    else
                        mappedValue = fromValue;
                    to.SetPropertyValue(propertyTo.Name, mappedValue);
                }
            }
        }

        protected void AddPropertyMap<TType1, TType2>(string property1, string property2)
        {
            PropertyMaps.Add(new PropertyMap(typeof (TType1), property1, typeof (TType2), property2));
        }

        private readonly List<PropertyMap> _propertyMaps = new List<PropertyMap>();

        private List<PropertyMap> PropertyMaps
        {
            get { return _propertyMaps; }
        }

        private class PropertyMap
        {
            public readonly string Type1Property;
            public readonly string Type2Property;
            public readonly Type Type1;
            public readonly Type Type2;

            public PropertyMap(Type type1, string type1Property, Type type2, string type2Property)
            {
                Type1 = type1;
                Type2 = type2;
                Type2Property = type2Property;
                Type1Property = type1Property;
            }
        }
    }
}
