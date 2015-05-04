#region

using System;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.Application.ApplicationOptions;
using JosephM.Record.Application.Constants;
using JosephM.Record.Application.Controller;
using JosephM.Record.Application.Navigation;

#endregion

namespace JosephM.Prism.Infrastructure.Module
{
    /// <summary>
    ///     Base Class For Implementing Modules To Plug Into The Application Framework
    /// </summary>
    public abstract class PrismModuleBase
    {
        internal IPrismModuleController Controller { get; set; }

        protected IApplicationController ApplicationController
        {
            get { return Controller.ApplicationController; }
        }

        protected PrismContainer Container
        {
            get { return Controller.Container; }
        }

        public ApplicationOptionsViewModel ApplicationOptions
        {
            get { return Controller.ApplicationOptions; }
        }

        public PrismSettingsManager SettingsManager
        {
            get { return Controller.SettingsManager; }
        }

        public abstract void RegisterTypes();

        public abstract void InitialiseModule();

        public void NavigateTo<T>()
        {
            NavigateTo<T>(null);
        }

        public void NavigateTo<T>(UriQuery uriQuery)
        {
            var prismQuery = new Microsoft.Practices.Prism.UriQuery();
            if (uriQuery != null)
            {
                foreach (var arg in uriQuery.Arguments)
                    prismQuery.Add(arg.Key, arg.Value);
            }
            var uri = new Uri(typeof (T).FullName + prismQuery, UriKind.Relative);
            ApplicationController.RequestNavigate(RegionNames.MainTabRegion, uri);
        }

        public void RegisterTypeForNavigation<T>()
        {
            Container.RegisterTypeForNavigation<T>();
        }

        public void RegisterInstance<T>(T instance)
        {
            Container.RegisterInstance(instance);
        }

        public void RegisterType<T>()
        {
            Container.RegisterType<T>();
        }

        public T Resolve<T>()
        {
            return Container.Resolve<T>();
        }
    }
}