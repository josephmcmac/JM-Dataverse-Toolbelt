using System;
using System.Collections.Generic;
using System.Linq;

namespace $safeprojectname$.Core
{
    public static class ObjectExtentions
    {
        public static object GetPropertyValue(this object instance, string propertyName)
        {
            var type = instance.GetType();
            var property = type.GetProperty(propertyName);
            if (property == null)
                throw new NullReferenceException(string.Format("Class {0} Does Not Have A Property Named {1}", type.Name,
                    propertyName));
            if (!property.CanRead)
                throw new MemberAccessException(string.Format("Property {0} Does Not Have Read Access In {1} Class",
                    propertyName, type.Name));
            return property
                .GetGetMethod()
                .Invoke(instance, new object[] { });
        }

        public static object SetPropertyValue(this object instance, string propertyName, object value)
        {
            var type = instance.GetType();
            var property = type.GetProperty(propertyName);
            if (property == null)
                throw new NullReferenceException(string.Format("Class {0} Does Not Have A Property Named {1}", type.Name,
                    propertyName));
            if (!property.CanWrite)
                throw new MemberAccessException(string.Format("Property {0} Does Not Have Write Access In {1} Class",
                    propertyName, type.Name));
            return property
                .GetSetMethod()
                .Invoke(instance, new object[] { value });
        }

        /// <summary>
        ///     Sets The Property Value Of The Object From The Raw String Value Loaded From app.config
        /// </summary>
        public static void SetPropertyByString(this object theObject, string propertyName, string rawConfigString)
        {
            if (string.IsNullOrWhiteSpace(rawConfigString))
                theObject.SetPropertyValue(propertyName, null);

            var property = theObject.GetType().GetProperty(propertyName);
            var propertyType = property.PropertyType;
            object newValue = null;

            if (propertyType == typeof(bool))
                newValue = rawConfigString == "1" || rawConfigString == "true";
            else if (propertyType.IsEnum)
                newValue = Enum.Parse(propertyType, rawConfigString);
            else if (propertyType == typeof(IEnumerable<string>))
                newValue = rawConfigString.Split(',');
            else if (propertyType == typeof(int))
                newValue = int.Parse(rawConfigString);
            else if (propertyType == typeof(Password))
                newValue = Password.CreateFromRawPassword(rawConfigString);
            else if (propertyType.HasStringConstructor())
                newValue = propertyType.CreateFromStringConstructor(rawConfigString);
            else
                newValue = rawConfigString;

            theObject.SetPropertyValue(propertyName, newValue);
        }

        public static bool IsInContext(this object instance, string propertyName)
        {
            var inContextAttributes =
                instance.GetType()
                    .GetProperty(propertyName)
                    .GetCustomAttributes(typeof(PropertyInContext), true)
                    .Cast<PropertyInContext>();
            if (!inContextAttributes.Any())
                return true;
            return inContextAttributes.All(a => a.IsInContext(instance));
        }
    }
}