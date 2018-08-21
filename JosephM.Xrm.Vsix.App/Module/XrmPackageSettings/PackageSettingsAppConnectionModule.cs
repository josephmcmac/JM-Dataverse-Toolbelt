using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.AppConfig;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Application;
using JosephM.XrmModule.AppConnection;

namespace JosephM.Xrm.Vsix.Module.PackageSettings
{
    /// <summary>
    /// Registers the application to redirect to enter package settings when a connection is required by a dialog which is navigated to
    /// </summary>
    [DependantModule(typeof(XrmPackageSettingsModule))]
    public class PackageSettingsAppConnectionModule : XrmRecordServiceAppConnectionModule
    {
        protected override DialogViewModel CreateRedirectDialog(DialogViewModel dialog, XrmRecordService xrmRecordService)
        {
            var visualStudioService = dialog.ApplicationController.ResolveType<IVisualStudioService>();
            var xrmPackageSettings = dialog.ApplicationController.ResolveType<XrmPackageSettings>();
            return new XrmPackageSettingsDialog(dialog, xrmPackageSettings, visualStudioService, xrmRecordService);
        }
    }
}