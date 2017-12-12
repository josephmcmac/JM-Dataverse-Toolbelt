#region

using JosephM.Application.Application;
using JosephM.Application.Options;
using JosephM.Core.AppConfig;
using System.Collections.Generic;

#endregion

namespace JosephM.Application.Modules
{
    /// <summary>
    /// Container Object For The Required Objects For A Prism Module To Initialise
    /// </summary>
    public class ModuleController
    {
        public ModuleController(
            IApplicationController applicationController,
            ISettingsManager settingsManager,
            IApplicationOptions applicationOptions
            )
        {
            ApplicationOptions = applicationOptions;
            ApplicationController = applicationController;
            SettingsManager = settingsManager;
            LoadedModules = new List<ModuleBase>();
        }

        public IDependencyResolver Container
        {
            get { return ApplicationController; }
        }

        public IApplicationOptions ApplicationOptions { get; private set; }

        public ISettingsManager SettingsManager { get; private set; }

        public IApplicationController ApplicationController { get; private set; }

        private List<ModuleBase> LoadedModules { get; set; }

        public void AddLoadedModule(ModuleBase module)
        {
            LoadedModules.Add(module);
        }

        public IEnumerable<ModuleBase> GetLoadedModules()
        {
            return LoadedModules;
        }
    }
}