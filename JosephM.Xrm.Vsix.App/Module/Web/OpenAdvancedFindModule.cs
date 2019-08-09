using JosephM.Application.Modules;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.Xrm.Vsix.Module.Web
{
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class OpenAdvancedFindModule : OptionActionModule
    {
        public override string MainOperationName => "Open Advanced Find";

        public override string MenuGroup => "Web";

        public override void DialogCommand()
        {
            ApplicationController.NavigateTo(typeof(OpenAdvancedFindDialog), null);
        }
    }
}
