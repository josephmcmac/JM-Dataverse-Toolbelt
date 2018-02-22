using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.Infrastructure.Dialog;

namespace JosephM.Application.Prism.Module.ReleaseCheckModule
{
    public class UpdateSettingsDialog : AppSettingsDialog<UpdateSettings, UpdateSettings>
    {
        public UpdateSettingsDialog(IDialogController dialogController) : base(dialogController)
        {
        }
    }
}
