using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.CodeGenerator.FetchToJavascript
{
    public class FetchToJavascriptRequest : ServiceRequestBase
    {
        [Multiline]
        [RequiredProperty]
        public string Fetch { get; set; }
    }
}