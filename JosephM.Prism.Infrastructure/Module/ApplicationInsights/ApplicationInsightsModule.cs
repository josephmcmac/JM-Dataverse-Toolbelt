using JosephM.Application.Desktop.Module.Settings;
using JosephM.Application.Modules;
using JosephM.Core.AppConfig;

namespace JosephM.Application.Desktop.Module.ApplicationInsights
{
    [DependantModule(typeof(SettingsAggregatorModule))]
    public abstract class ApplicationInsightsModule : AggregatedSettingModule<ApplicationInsightsSettings>
    {
        public abstract string InstrumentationKey { get; }

        public override void RegisterTypes()
        {
            base.RegisterTypes();

            var applicationInsightsLogger = new ApplicationInsightsLogger(InstrumentationKey, ApplicationController);
            ApplicationController.AddLogger(applicationInsightsLogger);
        }
    }
}
