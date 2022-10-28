using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.CodeGenerator.JavaScriptOptions
{
    [Group(Sections.JavaScript, Group.DisplayLayoutEnum.HorizontalLabelAbove)]
    public class JavaScriptOptionsResponse : ServiceResponseBase<ServiceResponseItem>
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