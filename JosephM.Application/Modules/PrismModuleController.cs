#region

using JosephM.Application.Application;
using JosephM.Application.Options;
using JosephM.Core.AppConfig;

#endregion

namespace JosephM.Application.Modules
{
    /// <summary>
    /// Container Object For The Required Objects For A Prism Module To Initialise
    /// </summary>
    public class PrismModuleController
    {
        public PrismModuleController(
            IApplicationController applicationController,
            PrismSettingsManager settingsManager,
            IApplicationOptions applicationOptions
            )
        {
            ApplicationOptions = applicationOptions;
            ApplicationController = applicationController;
            SettingsManager = settingsManager;
        }

        public IDependencyResolver Container
        {
            get { return ApplicationController; }
        }

        public IApplicationOptions ApplicationOptions { get; private set; }

        public PrismSettingsManager SettingsManager { get; private set; }

        public IApplicationController ApplicationController { get; private set; }
    }
}