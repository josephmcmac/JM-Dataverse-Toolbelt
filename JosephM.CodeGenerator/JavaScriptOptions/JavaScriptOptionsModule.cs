using JosephM.Application.Modules;
using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.XrmConnection;

namespace JosephM.CodeGenerator.JavaScriptOptions
{
    [MyDescription("Generate JavaScript Code Constants For A Picklist Field")]
    [DependantModule(typeof(XrmConnectionModule))]
    public class JavaScriptOptionsModule :
        ServiceRequestModule
            <JavaScriptOptionsDialog, JavaScriptOptionsService, JavaScriptOptionsRequest, JavaScriptOptionsResponse, ServiceResponseItem>
    {
        public override string MenuGroup => "Code Generation";

        public override string MainOperationName => "JavaScript Options";
    }
}