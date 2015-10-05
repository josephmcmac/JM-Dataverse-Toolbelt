using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JosephM.Core.AppConfig
{
    public static class Extentions
    {
        public static T ResolveType<T>(this IResolveObject objectResolver)
        {
            return (T)objectResolver.ResolveType(typeof(T));
        }

        public static void RegisterInstance<T>(this IDependencyResolver dependencyResolver, object instance)
        {
            dependencyResolver.RegisterInstance(typeof(T), instance);
        }

        public static void RegisterInstance(this IDependencyResolver dependencyResolver, object instance)
        {
            dependencyResolver.RegisterInstance(instance.GetType(), instance);
        }
    }
}
