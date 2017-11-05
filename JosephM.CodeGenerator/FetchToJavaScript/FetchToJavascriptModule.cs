using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Module;

namespace JosephM.CodeGenerator.FetchToJavascript
{
    public class FetchToJavascriptModule :
        ServiceRequestModule
            <FetchToJavascriptDialog, FetchToJavascriptService, FetchToJavascriptRequest, FetchToJavascriptResponse, ServiceResponseItem>
    {
        public override string MenuGroup => "Code Generation";
    }
}