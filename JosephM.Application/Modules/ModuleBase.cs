#region

using JosephM.Application.Application;
using JosephM.Application.Options;
using JosephM.Core.AppConfig;
using System;
using System.IO;

#endregion

namespace JosephM.Application.Modules
{
    /// <summary>
    ///     Base Class For Implementing Modules To Plug Into The Application Framework
    /// </summary>
    public abstract class ModuleBase
    {
        public ModuleController Controller { get; set; }

        public IApplicationController ApplicationController
        {
            get { return Controller.ApplicationController; }
        }

        protected IDependencyResolver Container
        {
            get { return Controller.Container; }
        }

        public IApplicationOptions ApplicationOptions
        {
            get { return Controller.ApplicationOptions; }
        }

        public abstract void RegisterTypes();

        public abstract void InitialiseModule();

        public void NavigateTo<T>()
        {
            NavigateTo<T>(null);
        }

        public void NavigateTo<T>(UriQuery uriQuery)
        {
            ApplicationController.RequestNavigate(RegionNames.MainTabRegion, typeof(T), uriQuery);
        }

        public void RegisterTypeForNavigation<T>()
        {
            Container.RegisterTypeForNavigation<T>();
        }

        public void RegisterInstance<T>(T instance)
        {
            Container.RegisterInstance<T>(instance);
        }

        public void RegisterType<I, T>()
        {
            Container.RegisterType<I, T>();
        }

        public T Resolve<T>()
        {
            return Container.ResolveType<T>();
        }

        public void AddOption(string group, string label, Action action, string description = null)
        {
            ApplicationOptions.AddOption(group, label, action, description: description);
        }

        public void AddSetting(string label, Action action, string description = null)
        {
            ApplicationOptions.AddOption("Setting", label, action, description: description);
        }
    }
}