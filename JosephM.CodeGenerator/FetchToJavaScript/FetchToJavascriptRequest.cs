using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.CodeGenerator.FetchToJavascript
{
    [Group(Sections.Fetch)]
    [DisplayName("Convert Fetch To Javascript")]
    [Instruction("A JavaScript Statement Will Be Output Which Initialises A JavaScript Variable To The Entered FetchXml String")]
    public class FetchToJavascriptRequest : ServiceRequestBase
    {
        [Multiline]
        [RequiredProperty]
        [DoNotLimitDisplayHeight]
        [Group(Sections.Fetch)]
        public string Fetch { get; set; }

        private static class Sections
        {
            public const string Fetch = "Fetch XML";
        }
    }
}