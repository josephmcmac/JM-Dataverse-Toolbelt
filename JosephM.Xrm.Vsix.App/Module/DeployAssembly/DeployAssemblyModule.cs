using System;
using JosephM.Application.Modules;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Xrm.Vsix.Module.PackageSettings;

namespace JosephM.Xrm.Vsix.Module.DeployAssembly
{
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class DeployAssemblyModule : OptionActionModule
    {
        public override string MainOperationName => "Deploy Assembly";

        public override string MenuGroup => "Plugins";

        public override void DialogCommand()
        {
            ApplicationController.RequestNavigate("Main", typeof(DeployAssemblyDialog), null);
        }
    }
}