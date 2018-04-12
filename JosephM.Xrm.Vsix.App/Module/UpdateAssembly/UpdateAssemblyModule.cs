using JosephM.Application.Modules;
using JosephM.XrmModule.XrmConnection;
using JosephM.Xrm.Vsix.Module.PackageSettings;

namespace JosephM.Xrm.Vsix.Module.UpdateAssembly
{
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class UpdateAssemblyModule : OptionActionModule
    {
        public override string MainOperationName => "Update Assembly";

        public override string MenuGroup => "Plugins";

        public override void DialogCommand()
        {
            ApplicationController.NavigateTo(typeof(UpdateAssemblyDialog), null);
        }
    }
}
