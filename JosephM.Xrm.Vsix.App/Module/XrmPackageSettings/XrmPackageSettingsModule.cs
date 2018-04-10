using JosephM.Application.Modules;
using JosephM.Application.Prism.Module.Settings;
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