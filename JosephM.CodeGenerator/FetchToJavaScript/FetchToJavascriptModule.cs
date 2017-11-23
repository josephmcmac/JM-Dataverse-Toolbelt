using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Module;

namespace JosephM.CodeGenerator.FetchToJavascript
{
    [MyDescription("Generate C# Or JavaScript Code For A Multiline String Value")]
    public class FetchToJavascriptModule :
        ServiceRequestModule
            <FetchToJavascriptDialog, FetchToJavascriptService, FetchToJavascriptRequest, FetchToJavascriptResponse, ServiceResponseItem>
    {
        public override string MenuGroup => "Code Generation";
    }
}