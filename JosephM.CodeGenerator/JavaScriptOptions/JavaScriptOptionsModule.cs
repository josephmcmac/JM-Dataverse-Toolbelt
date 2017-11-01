using JosephM.Application.Modules;
using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.XrmConnection;

namespace JosephM.CodeGenerator.JavaScriptOptions
{
    [DependantModule(typeof(XrmConnectionModule))]
    public class JavaScriptOptionsModule :
        ServiceRequestModule
            <JavaScriptOptionsDialog, JavaScriptOptionsService, JavaScriptOptionsRequest, JavaScriptOptionsResponse, ServiceResponseItem>
    {
        public override string MenuGroup => "Code Generation";
    }
}