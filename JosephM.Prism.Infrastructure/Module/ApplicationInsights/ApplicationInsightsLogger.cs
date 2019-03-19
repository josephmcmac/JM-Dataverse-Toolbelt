using JosephM.Application.Application;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using JosephM.Core.AppConfig;
using System.Collections.Generic;
using JosephM.Core.Extentions;

namespace JosephM.Application.Desktop.Module.ApplicationInsights
{
    public class ApplicationInsightsLogger : IApplicationLogger
    {
        public ApplicationInsightsLogger(string instrumentationKey, IApplicationController applicationController)
        {
            InstrumentationKey = instrumentationKey;
            ApplicationController = applicationController;
            SessionId = Guid.NewGuid().ToString();

            TelemetryConfiguration.Active.InstrumentationKey = InstrumentationKey;

            #if DEBUG
                IsDebugMode = true;
            #endif

            var telemetryConfiguration = new TelemetryConfiguration(InstrumentationKey);

            //this tells to promptly send data if debugging
            telemetryConfiguration.TelemetryChannel.DeveloperMode = IsDebugMode;
            //for when debuuging if want to send data uncomment this line
            //IsDebugMode = false;

            var tc = new TelemetryClient(telemetryConfiguration);
            tc.InstrumentationKey = InstrumentationKey;
            tc.Context.Cloud.RoleInstance = ApplicationController.ApplicationName;
            tc.Context.User.Id = string.Empty;
            tc.Context.Session.Id = SessionId;
            tc.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            TelemetryClient = tc;
        }

        public bool IsDebugMode { get; }

        public string InstrumentationKey { get; }
        public IApplicationController ApplicationController { get; }
        public string SessionId { get; }
        public TelemetryClient TelemetryClient { get; }

        public void LogEvent(string eventName, IDictionary<string, string> properties = null)
        {
            var settings = GetSettings();
            if (!settings.AllowUsageStatisticsLoaded)
            {
                var allow = ApplicationController.UserConfirmation("This application wants to capture anonymous usage statistics to help improve features\n\nClick yes to allow anonymous usage statistics or click no to opt out");
                settings.AllowUsageStatisticsLoaded = true;
                settings.AllowUsageStatistics = allow;
                ApplicationController.ResolveType<ISettingsManager>().SaveSettingsObject(settings);
                ApplicationController.RegisterInstance<ApplicationInsightsSettings>(settings);
            }
            if (!IsDebugMode && settings.AllowUsageStatistics)
            {
                TelemetryClient.TrackEvent(eventName, properties);
            }
        }

        private ApplicationInsightsSettings GetSettings()
        {
            return ApplicationController.ResolveType<ApplicationInsightsSettings>();
        }

        public void LogException(Exception ex)
        {
            var settings = GetSettings();
            if (!IsDebugMode && settings.AllowUsageStatistics)
            {
                ApplicationController.LogEvent("General Error", new Dictionary<string, string> { { "Error", ex.Message }, { "Error Trace", ex.DisplayString() } });
            }
        }
    }
}
