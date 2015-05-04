using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Test;

namespace JosephM.ObjectMapping.Test
{
    public class MappingTest : CoreTest
    {
        public MappingTest(MapperBase mapper)
        {
            Mapper = mapper;
        }

        protected MapperBase Mapper { get; set; }

        public void ValidateMapped(object to, object from)
        {
            Assert.AreNotEqual(to, from);
            var typeTo = to.GetType();
            foreach (var property in typeTo.GetReadWriteProperties())
            {
                ValidatePropertyMapped(property.Name, to, from);
            }
        }

        public void ValidatePropertyMapped(string property, object to, object from)
        {
            var mappedFrom = GetPropertyMappedFrom(property, to, from);
            if (mappedFrom != null)
            {
                var mappedFromProperty = from.GetType().GetProperty(mappedFrom);
                if (mappedFromProperty != null && mappedFromProperty.CanRead)
                {
                    var valueFrom = from.GetPropertyValue(mappedFrom);
                    var valueTo = to.GetPropertyValue(property);
                    var propertyFrom = from.GetType().GetProperty(mappedFrom);
                    var propertyTo = to.GetType().GetProperty(property);

                    if (propertyFrom.PropertyType == typeof (Guid) && propertyTo.PropertyType == typeof (string))
                    {
                        if ((Guid) valueFrom == Guid.Empty)
                            Assert.IsTrue(((string) valueTo).IsNullOrWhiteSpace());
                        else
                            Assert.AreEqual(valueFrom.ToString(), valueTo.ToString());
                    }
                    else if (propertyFrom.PropertyType == typeof (string) && propertyTo.PropertyType == typeof (Guid))
                    {
                        if (valueFrom == null)
                            Assert.IsTrue((Guid) valueTo == Guid.Empty);
                        else
                            Assert.AreEqual(valueFrom.ToString(), valueTo.ToString());
                    }
                    else if (propertyTo.PropertyType.IsEnum)
                        Assert.AreEqual(Enum.Parse(propertyTo.PropertyType, valueFrom.ToString()), valueTo);
                    else if (valueFrom == null)
                        Assert.IsNull(valueTo);
                    else if (propertyFrom.PropertyType.IsIEnumerableOfT())
                    {
                        var enumerableFrom = ToIEnumerableOfObject(valueFrom);
                        var enumerableTo = ToIEnumerableOfObject(valueTo);
                        Assert.AreEqual(enumerableFrom.Count(), enumerableTo.Count());
                        var count = enumerableFrom.Count();
                        if (count > 0)
                        {
                            Assert.IsFalse(Equals(valueFrom, valueTo));
                            for (var i = 0; i < count; i++)
                            {
                                var nestedObjectFrom = enumerableFrom.ElementAt(i);
                                var nestedObjectTo = enumerableTo.ElementAt(i);
                                ValidateMapped(nestedObjectTo, nestedObjectFrom);
                            }
                        }
                    }
                    else
                    {
                        var stringFrom = valueFrom.ToString();
                        var stringTo = valueTo.ToString();
                        if (valueFrom is Password)
                            stringFrom = ((Password) valueFrom).GetRawPassword();
                        if (valueTo is Password)
                            stringTo = ((Password) valueTo).GetRawPassword();
                        Assert.AreEqual(stringFrom, stringTo);
                    }
                }
            }
        }

        public static IEnumerable<object> ToIEnumerableOfObject(object iEnumerableOfgenericTypeInstance)
        {
            var objectList = new List<object>();
            foreach (var item in (IEnumerable)iEnumerableOfgenericTypeInstance)
            {
                objectList.Add(item);
            }
            return objectList;
        }

        protected virtual string GetPropertyMappedFrom(string property, object to, object from)
        {
            return Mapper.GetPropertyMappedFrom(property, from.GetType());
        }
    }
}