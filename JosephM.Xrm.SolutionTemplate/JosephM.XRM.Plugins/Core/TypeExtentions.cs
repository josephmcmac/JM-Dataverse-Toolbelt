#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion

namespace $safeprojectname$.Core
{
     public static class TypeExtentions
    {
        public static bool HasStringConstructor(this Type type)
        {
            return
                type.GetConstructors().Any(
                    c => c.GetParameters().Count() == 1 && c.GetParameters()[0].ParameterType == typeof(string));
        }

        public static bool HasParameterlessConstructor(this Type type)
        {
            return
                type.GetConstructors().Any(c => !c.GetParameters().Any());
        }

        public static bool IsTypeOf(this Type thisType, Type otherType)
        {
            return
                thisType.IsSubclassOf(otherType) || thisType == otherType;
        }

        public static object CreateFromParameterlessConstructor(this Type type)
        {
            var ctr = type.GetConstructors().First(c => !c.GetParameters().Any());
            return ctr.Invoke(new object[] { });
        }

        public static object CreateFromStringConstructor(this Type type, string stringArgument)
        {
            var ctr = type.GetConstructors().First(
                c => c.GetParameters().Count() == 1 && c.GetParameters()[0].ParameterType == typeof(string));
            return ctr.Invoke(new object[] { stringArgument });
        }


        public static ConstructorInfo GetStringConstructorInfo(this Type type)
        {
            return
                type.GetConstructors().Single(
                    c => c.GetParameters().Count() == 1 && c.GetParameters()[0].ParameterType == typeof(string));
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
    }
}