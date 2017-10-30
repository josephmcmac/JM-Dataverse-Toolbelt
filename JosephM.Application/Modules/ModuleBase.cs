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

        public void AddOption(string group, string label, Action action)
        {
            ApplicationOptions.AddOption(group, label, action);
        }

        public void AddSetting(string label, Action action)
        {
            ApplicationOptions.AddOption("Setting", label, action);
        }

        public void AddHelpUrl(string optionLabel, string htmlFileName)
        {
            ApplicationOptions.AddOption("Help", optionLabel, () => ApplicationController.OpenFile("https://github.com/josephmcmac/XRM-Developer-Tool/wiki/" + htmlFileName));
        }

        private void HelpCommand(string fileName)
        {
            var qualified = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HelpFiles", fileName);
            ApplicationController.OpenHelp(qualified);
        }
    }
}