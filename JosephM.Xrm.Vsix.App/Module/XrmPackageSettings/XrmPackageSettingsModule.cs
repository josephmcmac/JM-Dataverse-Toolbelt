using JosephM.Application.Modules;
using JosephM.Application.Desktop.Module.Settings;
using JosephM.XrmModule.XrmConnection;

namespace JosephM.Xrm.Vsix.Module.PackageSettings
{
    [DependantModule(typeof(XrmConnectionModule))]
    public class XrmPackageSettingsModule : SettingsModule<XrmPackageSettingsDialog, XrmPackageSettings, XrmPackageSettings>
    {
        public override void DialogCommand()
        {
            ApplicationController.NavigateTo(typeof(XrmPackageSettingsDialog), null);
        }
    }
}