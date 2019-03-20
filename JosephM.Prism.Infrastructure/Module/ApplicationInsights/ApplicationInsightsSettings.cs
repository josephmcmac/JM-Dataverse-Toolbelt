using JosephM.Core.Attributes;

namespace JosephM.Application.Desktop.Module.ApplicationInsights
{
    [Instruction("This application logs anonymous usage statistics and errors to application insights. Some location information is automatically captured but no user or machine names are included in the data submitted\n\nThe logged data is used by the author to improve functionality, count feature use, and identify unknown errors being thrown by the application\n\nIf you do not want the application to log use set the flag to false")]
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