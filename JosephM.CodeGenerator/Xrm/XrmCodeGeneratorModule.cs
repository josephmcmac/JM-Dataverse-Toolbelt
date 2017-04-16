using JosephM.Application.Modules;
using JosephM.CodeGenerator.Service;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.XrmConnection;

namespace JosephM.CodeGenerator.Xrm
{
    [DependantModule(typeof(XrmConnectionModule))]
    public class XrmCodeGeneratorModule :
        ServiceRequestModule
            <XrmCodeGeneratorDialog, XrmCodeGeneratorService, CodeGeneratorRequest, CodeGeneratorResponse,
                CodeGeneratorResponseItem>
    {
        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddHelpUrl("Code Generation", "CodeGeneration");
        }

        protected override string MainOperationName
        {
            get { return "Code Generation"; }
        }
    }
}