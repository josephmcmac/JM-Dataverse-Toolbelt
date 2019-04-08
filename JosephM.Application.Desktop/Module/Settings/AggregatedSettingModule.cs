using JosephM.Application.Application;
using JosephM.Application.Modules;
using JosephM.Core.AppConfig;

namespace JosephM.Application.Desktop.Module.Settings
{
    [DependantModule(typeof(SettingsAggregatorModule))]
    /// <summary>
    ///     Base Class For A Module Which Plugs A Settings Type Into The Application
    /// </summary>
    public abstract class AggregatedSettingModule<SettingsType> : ModuleBase
        where SettingsType : new()
    {
        public override void RegisterTypes()
        {
            var configManager = Resolve<ISettingsManager>();
            var settings = configManager.Resolve<SettingsType>();
            Container.RegisterInstance<SettingsType>(settings);
        }

        public override void InitialiseModule()
        {
            var aggregatedSettings = ApplicationController.ResolveType<SettingsAggregator>();
            aggregatedSettings.AddSettingType(typeof(SettingsType));
        }
    }
}