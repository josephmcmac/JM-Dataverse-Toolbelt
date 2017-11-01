using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.CodeGenerator.FetchToJavascript
{
    public class FetchToJavascriptResponse : ServiceResponseBase<ServiceResponseItem>
    {
        [Multiline]
        public string Javascript { get; set; }
    }
}