using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.CodeGenerator.FetchToJavascript
{
    [DisplayName("Convert Fetch To Javascript")]
    [Instruction("A JavaScript Statement Will Be Output Which Initialises A JavaScript Variable To The FetchXml String")]
    public class FetchToJavascriptRequest : ServiceRequestBase
    {
        [Multiline]
        [RequiredProperty]
        public string Fetch { get; set; }
    }
}