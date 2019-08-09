using JosephM.Application.Modules;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.Xrm.Vsix.Module.Web
{
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class OpenWebModule : OptionActionModule
    {
        public override string MainOperationName => "Open Web";

        public override string MenuGroup => "Web";

        public override void DialogCommand()
        {
            ApplicationController.NavigateTo(typeof(OpenWebDialog), null);
        }
    }
}
