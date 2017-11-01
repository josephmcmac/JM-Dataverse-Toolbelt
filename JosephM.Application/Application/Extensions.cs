using JosephM.Application.Modules;
using JosephM.Application.Options;
using JosephM.Core.AppConfig;
using JosephM.Core.Log;

namespace JosephM.Application.Application
{
    public static class Extensions
    {
        public static void RegisterInfrastructure(this IApplicationController applicationController, IApplicationOptions applicationOptions, ISettingsManager settingsManager)
        {
            applicationController.RegisterInstance<ISettingsManager>(settingsManager);
            applicationController.RegisterInstance<IApplicationController>(applicationController);
            applicationController.RegisterInstance<IResolveObject>(applicationController);

            applicationController.RegisterInstance<IApplicationOptions>(applicationOptions);

            var prismModuleController = new ModuleController(
                applicationController,
                applicationController.ResolveType<ISettingsManager>(),
                applicationController.ResolveType<IApplicationOptions>()
                );

            applicationController.RegisterInstance<ModuleController>(prismModuleController);
            applicationController.RegisterInstance(new LogController());
        }
    }
}
