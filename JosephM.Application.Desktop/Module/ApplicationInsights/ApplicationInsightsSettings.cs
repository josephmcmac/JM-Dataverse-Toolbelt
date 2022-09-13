using JosephM.Core.Attributes;

namespace JosephM.Application.Desktop.Module.ApplicationInsights
{
    [Instruction("This setting allows feature use, errors, and geographic information to be submitted to application insights.\n\nThe logged data is used by the publisher to improve features, count feature use, and identify errors being thrown by the application.\n\nTo opt out and prevent any information being logged uncheck 'Allow Logging'.")]
    [Group(Sections.Main, wrapHorizontal:true, displayLabel: false)]
    public class ApplicationInsightsSettings
    {
        public ApplicationInsightsSettings()
        {
            AllowUseLogging = true;
        }

        [DisplayName("Allow Logging")]
        [Group(Sections.Main)]
        [DisplayOrder(10)]
        public bool AllowUseLogging { get; set; }

        public class Sections
        {
            public const string Main = "Main";
        }
    }
}