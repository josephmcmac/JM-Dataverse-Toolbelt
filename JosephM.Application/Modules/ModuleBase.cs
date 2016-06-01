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

        protected IApplicationController ApplicationController
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

        public void AddOption(string label, Action action)
        {
            ApplicationOptions.AddOption(label, action, ApplicationOptionType.Main);
        }

        public void AddSetting(string label, Action action)
        {
            ApplicationOptions.AddOption(label, action, ApplicationOptionType.Setting);
        }

        /// <summary>
        /// !!! YOU NEED TO HAVE THE HTML FILE IN A FOLDER NAMED "HelpFiles" INCLUDED IN THE PROJECT BUILD COPY TO OUTPUT
        /// Then ensure the applications build folder is included in the installer folders
        /// </summary>
        public void AddHelp(string optionLabel, string htmlFileName)
        {
            ApplicationOptions.AddOption(optionLabel, () => HelpCommand(htmlFileName), ApplicationOptionType.Help);
        }

        public void AddHelpUrl(string optionLabel, string htmlFileName)
        {
            ApplicationOptions.AddOption(optionLabel, () => ApplicationController.OpenFile("https://github.com/josephmcmac/XRM-Developer-Tool/wiki/" + htmlFileName), ApplicationOptionType.Help);
        }

        private void HelpCommand(string fileName)
        {
            var qualified = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HelpFiles", fileName);
            ApplicationController.OpenHelp(qualified);
        }
    }
}