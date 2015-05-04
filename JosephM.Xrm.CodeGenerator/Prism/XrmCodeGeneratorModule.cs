using JosephM.Prism.Infrastructure.Attributes;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.Xrm;
using JosephM.Xrm.CodeGenerator.Service;

namespace JosephM.Xrm.CodeGenerator.Prism
{
    [DependantModule(typeof(XrmModuleModule))]
    public class XrmCodeGeneratorModule :
        ServiceRequestModule
            <XrmCodeGeneratorDialog, XrmCodeGeneratorService, CodeGeneratorRequest, CodeGeneratorResponse, CodeGeneratorResponseItem>
    {
        protected override string MainOperationName
        {
            get { return "Generate Code"; }
        }

        public override void InitialiseModule()
        {
            base.InitialiseModule();
            ApplicationOptions.AddHelp("Generate Code", "Code Generator Help.htm");
        }
    }
}