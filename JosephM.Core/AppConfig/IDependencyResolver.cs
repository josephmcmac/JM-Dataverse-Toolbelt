using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JosephM.Core.AppConfig
{
    public interface IDependencyResolver : IResolveObject
    {
        void RegisterInstance(Type type, object instance);
        object ResolveType(string typeName);
        void RegisterType<I, T>();
        void RegisterTypeForNavigation<T>();
        void RegisterInstance(Type type, string key, object instance);
        object ResolveInstance(Type type, string key);
    }
}
