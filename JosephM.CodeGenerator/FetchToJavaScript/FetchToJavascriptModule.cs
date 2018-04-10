using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.CodeGenerator.FetchToJavascript
{
    [MyDescription("Generate C# Or JavaScript Code For A Multiline String Value")]
    public class FetchToJavascriptModule :
        ServiceRequestModule
            <FetchToJavascriptDialog, FetchToJavascriptService, FetchToJavascriptRequest, FetchToJavascriptResponse, ServiceResponseItem>
    {
        public override string MenuGroup => "Code Generation";

        public override string MainOperationName => "Fetch 2 Javascript";
    }
}