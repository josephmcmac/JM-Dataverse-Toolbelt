using JosephM.Application.Prism.Module.Settings;
using JosephM.Application.ViewModel.Dialog;

namespace JosephM.Application.Prism.Module.ReleaseCheckModule
{
    public class UpdateSettingsDialog : AppSettingsDialog<UpdateSettings, UpdateSettings>
    {
        public UpdateSettingsDialog(IDialogController dialogController) : base(dialogController)
        {
        }
    }
}
