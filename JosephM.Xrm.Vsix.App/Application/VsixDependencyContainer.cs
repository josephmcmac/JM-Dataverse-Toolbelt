using JosephM.Application.Application;
using JosephM.Core.AppConfig;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Record.Application.Fakes;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;

namespace JosephM.Xrm.Vsix.Application
{
    public class VsixDependencyContainer : PrismDependencyContainer
    {
        public VsixDependencyContainer()
            : base()
        {
        }

        public static VsixDependencyContainer Create(XrmPackageSettings settings, IVisualStudioService visualStudioService)
        {
            var container = new VsixDependencyContainer();
            container.RegisterInstance(typeof(ISettingsManager), new VsixSettingsManager(visualStudioService));
            container.RegisterInstance(typeof(ISavedXrmConnections), settings);
            return container;
        }
    }
}
