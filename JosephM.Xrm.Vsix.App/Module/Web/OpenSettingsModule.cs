using JosephM.Application.Modules;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.Xrm.Vsix.Module.Web
{
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class OpenSettingsModule : OptionActionModule
    {
        public override string MainOperationName => "Open Settings";

        public override string MenuGroup => "Web";

        public override void DialogCommand()
        {
            ApplicationController.NavigateTo(typeof(OpenSettingsDialog), null);
        }
    }
}
