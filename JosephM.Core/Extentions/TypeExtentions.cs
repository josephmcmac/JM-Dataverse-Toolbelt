#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JosephM.Core.Attributes;

#endregion

namespace JosephM.Core.Extentions
{
    public static class TypeExtentions
    {
        public static bool HasConstructorFor(this Type type, Type constructorInjectionType)
        {
            var matchingCOnstructors = GetMatchingConstructors(type, constructorInjectionType);
            return matchingCOnstructors.Any();
        }

        private static IEnumerable<ConstructorInfo> GetMatchingConstructors(Type type, Type constructorInjectionType)
        {
            var matchingCOnstructors = type.GetConstructors().Where(
                c => c.GetParameters().Count() == 1 && constructorInjectionType.IsTypeOf(c.GetParameters()[0].ParameterType));
            return matchingCOnstructors;
        }

        public static object CreateFromConstructorFor(this Type type, object constructorInjection)
        {
            return GetMatchingConstructors(type, constructorInjection.GetType()).First().Invoke(new object[] { constructorInjection });
        }

        public static bool HasStringConstructor(this Type type)
        {
            return
                type.GetConstructors().Any(
                    c => c.GetParameters().Count() == 1 && c.GetParameters()[0].ParameterType == typeof (string));
        }

        public static bool HasParameterlessConstructor(this Type type)
        {
            return
                type.GetConstructors().Any(c => !c.GetParameters().Any());
        }

        public static bool IsTypeOf(this Type thisType, Type otherType)
        {
            return
                thisType.IsSubclassOf(otherType) || thisType == otherType || thisType.GetInterfaces().Contains(otherType);
        }

        public static object CreateFromParameterlessConstructor(this Type type)
        {
            var ctr = type.GetConstructors().First(c => !c.GetParameters().Any());
            return ctr.Invoke(new object[] {});
        }

        public static object CreateFromStringConstructor(this Type type, string stringArgument)
        {
            var ctr = type.GetConstructors().First(
                c => c.GetParameters().Count() == 1 && c.GetParameters()[0].ParameterType == typeof (string));
            return ctr.Invoke(new object[] {stringArgument});
        }

        public static string GetPropertyDisplayName(this Type type, string property)
        {
            return type.GetProperty(property).GetDisplayName();
        }

        public static string GetDisplayName(this PropertyInfo info)
        {
            var displayNameProperties =
                info.GetCustomAttributes(typeof(DisplayNameAttribute), true).Cast<DisplayNameAttribute>();
            if (displayNameProperties.Any())
                return displayNameProperties.First().Label;
            else
                return info.Name.SplitCamelCase();
        }

        public static string GetDisplayName(this Type type)
        {
            var displayNameProperties =
                type.GetCustomAttributes(typeof (DisplayNameAttribute), true).Cast<DisplayNameAttribute>();
            if (displayNameProperties.Any())
                return displayNameProperties.First().Label;
            else
                return type.Name.SplitCamelCase();
        }

        public static IEnumerable<PropertyInfo> GetReadWriteProperties(this Type type)
        {
            return
                type.GetProperties().Where(p => p.CanWrite && p.CanRead);
        }

        public static IEnumerable<PropertyInfo> GetWritableProperties(this Type type)
        {
            return
                type.GetProperties().Where(p => p.CanWrite);
        }

        public static IEnumerable<PropertyInfo> GetReadableProperties(this Type type)
        {
            return
                type.GetProperties().Where(p => p.CanRead);
        }

        public static IEnumerable<PropertyValidator> GetValidatorAttributes(this Type type, string propertyName)
        {
            var validatorAttributes =
                type
                    .GetProperty(propertyName)
                    .GetCustomAttributes(typeof (PropertyValidator), true)
                    .Cast<PropertyValidator>();
            return validatorAttributes;
        }

        public static bool IsIEnumerableOfT(this Type type)
        {
            return type.Name == "IEnumerable`1";
        }

        public static object ToNewTypedEnumerable(this Type genericType, IEnumerable<object> objectEnumerable)
        {
            var typedEnumerable = typeof (Enumerable)
                .GetMethod("Cast", new[] {typeof (IEnumerable)})
                .MakeGenericMethod(genericType)
                .Invoke(null, new object[] {objectEnumerable});
            typedEnumerable = typeof (Enumerable)
                .GetMethod("ToArray")
                .MakeGenericMethod(genericType)
                .Invoke(null, new object[] {typedEnumerable});
            return typedEnumerable;
        }
    }
}