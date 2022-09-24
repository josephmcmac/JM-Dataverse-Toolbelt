using JosephM.Application.Application;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Core.Security;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.Desktop.Module.ApplicationInsights
{
    public class ApplicationInsightsLogger : IApplicationLogger
    {
        public ApplicationInsightsLogger(string instrumentationKey, IApplicationController applicationController)
        {
            InstrumentationKey = instrumentationKey;
            ApplicationController = applicationController;
            SessionId = Guid.NewGuid().ToString();
            AnonymousString = "Anonymous " + StringEncryptor.HashString(UserName);

            #if DEBUG
                IsDebugMode = true;
            #endif

            var telemetryConfiguration = new TelemetryConfiguration();
            telemetryConfiguration.ConnectionString = $"InstrumentationKey={InstrumentationKey}";

            //this tells to promptly send data if debugging
            telemetryConfiguration.TelemetryChannel.DeveloperMode = IsDebugMode;
            //for when debugging if want to send data uncomment this line
            //IsDebugMode = false;

            var tc = new TelemetryClient(telemetryConfiguration);
            tc.Context.Cloud.RoleInstance = ApplicationController.ApplicationName;
            tc.Context.User.UserAgent = $"{ApplicationController.ApplicationName} {ApplicationController.Version}";
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

        private string AnonymousString { get; set; }

        private string UserName
        {
            get
            {
                return Environment.UserName;
            }
        }

        public void LogEvent(string eventName, IDictionary<string, string> properties = null)
        {
            var settings = ApplicationController.ResolveType<ApplicationInsightsSettings>();
            if (!IsDebugMode && settings.AllowUseLogging)
            {
                properties = properties ?? new Dictionary<string, string>();
                void addProperty(string name, string value)
                {
                    if (!properties.ContainsKey(name))
                    {
                        properties.Add(name, value);
                    }
                }
                addProperty("App", ApplicationController.ApplicationName);
                addProperty("App Version", ApplicationController.Version);

                foreach (var property in properties.ToArray())
                {
                    properties[property.Key] = property.Value.ReplaceIgnoreCase(UserName, "{UserName}");
                }

                TelemetryClient.Context.User.Id = AnonymousString;
                TelemetryClient.TrackEvent(eventName, properties);
            }
        }

        public void LogException(Exception ex)
        {
            LogEvent("General Error", new Dictionary<string, string> { { "Is Error", true.ToString() }, { "Error", ex.Message }, { "Error Trace", ex.DisplayString() } });
        }
    }
}
