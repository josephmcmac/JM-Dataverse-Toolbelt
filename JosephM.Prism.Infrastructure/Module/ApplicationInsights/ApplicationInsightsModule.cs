using JosephM.Application.Desktop.Module.Settings;

namespace JosephM.Application.Desktop.Module.ApplicationInsights
{
    public abstract class ApplicationInsightsModule : SettingsModule<ApplicationInsightsDialog, ApplicationInsightsSettings, ApplicationInsightsSettings>
    {
        public abstract string InstrumentationKey { get; }

        public override void RegisterTypes()
        {
            base.RegisterTypes();

            var applicationInsightsLogger = new ApplicationInsightsLogger(InstrumentationKey, ApplicationController);
            ApplicationController.AddLogger(applicationInsightsLogger);
        }

        public override string MainOperationName => "Use Logging";
    }
}
