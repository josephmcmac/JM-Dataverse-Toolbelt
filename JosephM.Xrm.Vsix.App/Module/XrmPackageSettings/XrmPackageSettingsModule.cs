using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.XrmConnection;

namespace JosephM.Xrm.Vsix.Module.PackageSettings
{
    [DependantModule(typeof(XrmConnectionModule))]
    public class XrmPackageSettingsModule : SettingsModule<XrmPackageSettingsDialog, XrmPackageSettings, XrmPackageSettings>
    {
        public override void DialogCommand()
        {
            ApplicationController.RequestNavigate("Main", typeof(XrmPackageSettingsDialog), null);
        }
    }
}