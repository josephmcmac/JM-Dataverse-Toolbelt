using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JosephM.Core.AppConfig;
using Microsoft.Practices.Unity;

namespace JosephM.Migration.Notification
{
    public class WebServiceDependencyContainer : IDependencyResolver
    {
        private IUnityContainer UnityContainer { get; set; }

        public WebServiceDependencyContainer(IUnityContainer unityContainer)
        {
            UnityContainer = unityContainer;
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
            UnityContainer.RegisterType(typeof(I), typeof(T));
        }

        public void RegisterTypeForNavigation<T>()
        {
            UnityContainer.RegisterType(typeof(object), typeof(T), typeof(T).FullName);
        }
    }
}
