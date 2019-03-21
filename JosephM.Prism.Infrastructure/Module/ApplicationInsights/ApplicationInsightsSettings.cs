using JosephM.Core.Attributes;

namespace JosephM.Application.Desktop.Module.ApplicationInsights
{
    [Instruction("This setting is for information on feature use and errors being submitted to application insights. Some location information is automatically captured but unless you opt in your user and machine name is not included in the data submitted\n\nThe logged data is used by the author to improve functionality, count feature use, and identify unknown errors being thrown by the application\n\nIf you do not want any of your use logged set the flag to false. To allow your username to be included in the information submitted please set this option. This will assist with reporting on numbers of users")]
    [Group(Sections.Main, wrapHorizontal:true)]
    public class ApplicationInsightsSettings
    {
        public ApplicationInsightsSettings()
        {
            AllowUseLogging = true;
        }

        [Group(Sections.Main)]
        [DisplayOrder(10)]
        public bool AllowUseLogging { get; set; }

        [Group(Sections.Main)]
        [DisplayOrder(20)]
        [PropertyInContextByPropertyValue(nameof(AllowUseLogging), true)]
        public bool AllowCaptureUsername { get; set; }

        public class Sections
        {
            public const string Main = "Main";
        }
    }
}