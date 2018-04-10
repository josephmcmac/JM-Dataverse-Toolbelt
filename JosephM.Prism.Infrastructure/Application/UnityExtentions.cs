using JosephM.Application.Application;
using JosephM.Application.Options;
using JosephM.Application.ViewModel.ApplicationOptions;
using JosephM.Application.ViewModel.Dialog;
using Prism.Regions;
using Prism.Unity;

namespace JosephM.Application.Prism.Application
{
    /// <summary>
    ///     Extention Methods On Unity Classes
    /// </summary>
    public static class UnityExtentions
    {
        public static void RegisterInfrastructure(this UnityBootstrapper bootstrapper, string applicationName)
        {
            var applicationController = new PrismApplicationController(
                bootstrapper.Container.TryResolve<IRegionManager>(),
                applicationName, new PrismDependencyContainer(bootstrapper.Container));

            applicationController.RegisterInfrastructure(new ApplicationOptionsViewModel(applicationController), new PrismSettingsManager(applicationController));
            applicationController.RegisterType<IDialogController, DialogController>();
        }


        /// <summary>
        ///     Loads The Inital View Models Including Application Options To The UI
        /// </summary>
        /// <param name="bootstrapper"></param>
        public static void InitialiseLoadShell(this UnityBootstrapper bootstrapper)
        {
            var applicationOptionsViewModel = bootstrapper.Container.TryResolve<IApplicationOptions>();
            bootstrapper.Container.TryResolve<IRegionManager>()
                .RegisterViewWithRegion(RegionNames.ApplicationOptions,
                    () => applicationOptionsViewModel);
            var applicationViewModel = (ApplicationOptionsViewModel)bootstrapper.Container.TryResolve<IApplicationOptions>();
            bootstrapper.Container.TryResolve<IRegionManager>()
                .RegisterViewWithRegion(RegionNames.Heading,
                    () => applicationViewModel.ApplicationController);
        }
    }
}