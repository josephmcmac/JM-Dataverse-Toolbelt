#region

using System;
using Microsoft.Practices.Unity;
using JosephM.Record.Application.ApplicationOptions;
using JosephM.Record.Application.Controller;

#endregion

namespace JosephM.Prism.Infrastructure.Prism
{
    /// <summary>
    /// Container Object For The Required Objects For A Prism Module To Initialise
    /// </summary>
    public class PrismModuleController : IPrismModuleController
    {
        public PrismModuleController(
            IUnityContainer container,
            IApplicationController regionController,
            PrismSettingsManager settingsManager,
            ApplicationOptionsViewModel applicationOptions
            )
        {
            ApplicationOptions = applicationOptions;
            Container = new PrismContainer(container);
            ApplicationController = regionController;
            SettingsManager = settingsManager;
        }

        public PrismContainer Container { get; private set; }

        public ApplicationOptionsViewModel ApplicationOptions { get; private set; }


        public PrismSettingsManager SettingsManager { get; private set; }

        public IApplicationController ApplicationController { get; private set; }
    }
}