using JosephM.Application.Modules;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.XrmConnection;

namespace JosephM.Xrm.Vsix.Module.Web
{
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class OpenDefaultSolutionModule : OptionActionModule
    {
        public override string MainOperationName => "Open Default Solution";

        public override string MenuGroup => "Web";

        public override void DialogCommand()
        {
            ApplicationController.NavigateTo(typeof(OpenDefaultSolutionDialog), null);
        }
    }
}
