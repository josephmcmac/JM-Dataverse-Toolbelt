using JosephM.XRM.VSIX.Commands.PackageSettings;
using JosephM.XRM.VSIX.Dialogs;

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
            var settings = GetPackageSettings();
            if (settings == null)
                settings = new XrmPackageSettings();
            var xrmService = GetXrmRecordService();
            var dialog = new XrmPackageSettingDialog(DialogUtility.CreateDialogController(), settings, GetVisualStudioService(), true, xrmService);

            DialogUtility.LoadDialog(dialog);
        }
    }
}
