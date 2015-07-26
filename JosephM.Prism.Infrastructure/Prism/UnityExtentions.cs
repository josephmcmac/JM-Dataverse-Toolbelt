#region

using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using JosephM.Core.Log;
using JosephM.Record.Application.ApplicationOptions;
using JosephM.Record.Application.Constants;
using JosephM.Record.Application.Controller;
using JosephM.Record.Application.Dialog;
using JosephM.Record.Application.HTML;
using JosephM.Record.Application.RecordEntry.Form;

#endregion

namespace JosephM.Prism.Infrastructure.Prism
{
    /// <summary>
    ///     Extention Methods On Unity Classes
    /// </summary>
    public static class UnityExtentions
    {
        public static void RegisterTypeForNavigation<T>(this IUnityContainer container)
        {
            container.RegisterType(typeof (object), typeof (T), typeof (T).FullName);
        }

        public static void RegisterInfrastructure(this UnityBootstrapper bootstrapper, string applicationName)
        {
            var applicationController = new ApplicationController(
                bootstrapper.Container.Resolve<IRegionManager>(),
                applicationName, bootstrapper.Container);

            bootstrapper.Container.RegisterInfrastructure<DialogController>(applicationController);
        }

        public static void RegisterInfrastructure<TDialogController>(this IUnityContainer container,
            IApplicationController applicationController)
            where TDialogController : IDialogController
        {
            container.RegisterInstance(new PrismSettingsManager(applicationController));

            container.RegisterInstance(applicationController);

            var appController = container.Resolve<IApplicationController>();

            var applicationOptionsViewModel = new ApplicationOptionsViewModel(appController);
            container.RegisterInstance(applicationOptionsViewModel);
            var applicationViewModel = new ApplicationViewModel(appController);
            container.RegisterInstance(applicationViewModel);

            var prismModuleController = new PrismModuleController(
                container.Resolve<IUnityContainer>(),
                container.Resolve<IApplicationController>(),
                container.Resolve<PrismSettingsManager>(),
                container.Resolve<ApplicationOptionsViewModel>()
                );

            container.RegisterInstance<IPrismModuleController>(prismModuleController);

            container.RegisterTypeForNavigation<RecordEntryFormViewModel>();

            container.RegisterInstance(new LogController());

            container.RegisterType<IDialogController, TDialogController>();

            container.RegisterTypeForNavigation<HtmlFileModel>();
        }


        /// <summary>
        ///     Loads The Inital View Models Including Application Options To The UI
        /// </summary>
        /// <param name="bootstrapper"></param>
        public static void InitialiseLoadShell(this UnityBootstrapper bootstrapper)
        {
            var applicationOptionsViewModel = bootstrapper.Container.Resolve<ApplicationOptionsViewModel>();
            bootstrapper.Container.Resolve<IRegionManager>()
                .RegisterViewWithRegion(RegionNames.ApplicationOptions,
                    () => applicationOptionsViewModel);
            var applicationViewModel = bootstrapper.Container.Resolve<ApplicationViewModel>();
            bootstrapper.Container.Resolve<IRegionManager>()
                .RegisterViewWithRegion(RegionNames.Heading,
                    () => applicationViewModel);
        }
    }
}