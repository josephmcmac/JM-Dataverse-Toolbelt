using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.CodeGenerator.FetchToJavascript
{
    [Group(Sections.JavaScript, true)]
    public class FetchToJavascriptResponse : ServiceResponseBase<ServiceResponseItem>
    {
        [Group(Sections.JavaScript)]
        [Multiline]
        [DoNotLimitDisplayHeight]
        public string Javascript { get; set; }

        private static class Sections
        {
            public const string JavaScript = "Generated JavaScript";
        }
    }
}