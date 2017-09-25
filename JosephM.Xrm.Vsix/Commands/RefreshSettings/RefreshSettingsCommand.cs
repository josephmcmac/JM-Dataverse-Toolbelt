using JosephM.Record.Xrm.XrmRecord;
using JosephM.XRM.VSIX.Commands.PackageSettings;
using JosephM.XRM.VSIX.Commands.RefreshConnection;
using JosephM.XRM.VSIX.Dialogs;
using JosephM.XRM.VSIX.Utilities;
using System;

namespace JosephM.XRM.VSIX.Commands.RefreshSettings
{
    internal sealed class RefreshSettingsCommand : CommandBase<RefreshSettingsCommand>
    {
        public override int CommandId
        {
            get { return 0x0106; }
        }

        public override void DoDialog()
        {

            var xrmConfig = VsixUtility.GetXrmConfig(ServiceProvider, true);
            //if no discovery service address this is a new connection
            if (string.IsNullOrWhiteSpace(xrmConfig.DiscoveryServiceAddress))
            {
                //if no connection has yet been loaded for this solution
                //then we need one before loading the package settings form
                xrmConfig = new XrmRecordConfiguration();
                var connectionDialog = new ConnectionEntryDialog(CreateDialogController(GetPackageSettings()), xrmConfig, GetVisualStudioService(), true);
                DialogUtility.LoadDialog(connectionDialog, showCompletion: false, postCompletion: () => { TryDoSomething(ActuallyDoDialog); });
            }
            else
            {
                ActuallyDoDialog();
            }
        }

        private void ActuallyDoDialog()
        {
            var settings = GetPackageSettings();
            if (settings == null)
                settings = new XrmPackageSettings();
            XrmRecordService xrmService = null;
            xrmService = GetXrmRecordService();
            var dialog = new XrmPackageSettingDialog(CreateDialogController(settings), settings, GetVisualStudioService(), true, xrmService);

            DialogUtility.LoadDialog(dialog);
        }
    }
}
