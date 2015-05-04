#region

using System;
using JosephM.Record.Application.ApplicationOptions;
using JosephM.Record.Application.Controller;

#endregion

namespace JosephM.Prism.Infrastructure.Prism
{
    /// <summary>
    /// Container Object For The Required Objects For A Prism Module To Initialise
    /// </summary>
    public interface IPrismModuleController
    {
        PrismContainer Container { get; }
        PrismSettingsManager SettingsManager { get; }
        ApplicationOptionsViewModel ApplicationOptions { get; }
        IApplicationController ApplicationController { get; }
    }
}