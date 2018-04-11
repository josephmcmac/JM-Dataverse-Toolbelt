using JosephM.Application.Desktop.Module.Settings;
using JosephM.Application.ViewModel.Dialog;

namespace JosephM.Application.Desktop.Module.ReleaseCheckModule
{
    public class UpdateSettingsDialog : AppSettingsDialog<UpdateSettings, UpdateSettings>
    {
        public UpdateSettingsDialog(IDialogController dialogController) : base(dialogController)
        {
        }
    }
}
