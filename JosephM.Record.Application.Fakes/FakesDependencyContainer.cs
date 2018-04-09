using System;
using JosephM.Core.AppConfig;
using Microsoft.Practices.Unity;

namespace JosephM.Application.ViewModel.Fakes
{
    public class FakesDependencyContainer : IDependencyResolver
    {
        private UnityContainer UnityContainer { get; set; }

        public FakesDependencyContainer()
        {
            UnityContainer = new UnityContainer();
        }

        public object ResolveType(Type type)
        {
            return UnityContainer.Resolve(type);
        }

        public void RegisterInstance(Type type, object instance)
        {
            UnityContainer.RegisterInstance(type, instance);
        }

        public object ResolveType(string name)
        {
            return UnityContainer.Resolve(typeof(object), name);
        }

        public void RegisterType<I, T>()
        {
            UnityContainer.RegisterType(typeof (I), typeof (T));
        }

        public void RegisterTypeForNavigation<T>()
        {
            UnityContainer.RegisterType(typeof(T), typeof(T));
            UnityContainer.RegisterType(typeof(object), typeof(T), typeof(T).FullName);
        }

        public void RegisterInstance(Type type, string key, object instance)
        {
            UnityContainer.RegisterInstance(type, key, instance);
        }

        public object ResolveInstance(Type type, string key)
        {
            return UnityContainer.Resolve(type, key);
        }
    }
}
