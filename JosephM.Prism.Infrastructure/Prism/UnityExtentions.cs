#region

using JosephM.Application;
using JosephM.Application.Application;
using JosephM.Application.Options;
using JosephM.Application.ViewModel.ApplicationOptions;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.HTML;
using JosephM.Record.Application.Fakes;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;

#endregion

namespace JosephM.Prism.Infrastructure.Prism
{
    /// <summary>
    ///     Extention Methods On Unity Classes
    /// </summary>
    public static class UnityExtentions
    {
        public static void RegisterInfrastructure(this UnityBootstrapper bootstrapper, string applicationName)
        {
            var applicationController = new PrismApplicationController(
                bootstrapper.Container.Resolve<IRegionManager>(),
                applicationName, new PrismDependencyContainer(bootstrapper.Container));

            applicationController.RegisterInfrastructure(new ApplicationOptionsViewModel(applicationController));
            applicationController.RegisterTypeForNavigation<HtmlFileModel>();
            applicationController.RegisterType<IDialogController, DialogController>();
        }


        /// <summary>
        ///     Loads The Inital View Models Including Application Options To The UI
        /// </summary>
        /// <param name="bootstrapper"></param>
        public static void InitialiseLoadShell(this UnityBootstrapper bootstrapper)
        {
            var applicationOptionsViewModel = bootstrapper.Container.Resolve<IApplicationOptions>();
            bootstrapper.Container.Resolve<IRegionManager>()
                .RegisterViewWithRegion(RegionNames.ApplicationOptions,
                    () => applicationOptionsViewModel);
            var applicationViewModel = (ApplicationOptionsViewModel)bootstrapper.Container.Resolve<IApplicationOptions>();
            bootstrapper.Container.Resolve<IRegionManager>()
                .RegisterViewWithRegion(RegionNames.Heading,
                    () => applicationViewModel.ApplicationController);
        }
    }
}