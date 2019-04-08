using JosephM.Application.Desktop.Module.Settings;
using JosephM.Application.Modules;

namespace JosephM.Application.Desktop.Module.ApplicationInsights
{
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
