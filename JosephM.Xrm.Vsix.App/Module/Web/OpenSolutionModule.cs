using JosephM.Application.Modules;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.Xrm.Vsix.Module.Web
{
    [MenuItemVisibleSolutionConfigured]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class OpenSolutionModule : OptionActionModule
    {
        public override string MainOperationName => "Open Solution";

        public override string MenuGroup => "Web";

        public override void DialogCommand()
        {
            ApplicationController.NavigateTo(typeof(OpenSolutionDialog), null);
        }
    }
}
