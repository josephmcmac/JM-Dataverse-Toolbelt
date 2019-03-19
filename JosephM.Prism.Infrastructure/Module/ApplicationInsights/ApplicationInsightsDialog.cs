using JosephM.Application.Desktop.Module.Settings;
using JosephM.Application.ViewModel.Dialog;

namespace JosephM.Application.Desktop.Module.ApplicationInsights
{
    public class ApplicationInsightsDialog : AppSettingsDialog<ApplicationInsightsSettings, ApplicationInsightsSettings>
    {
        public ApplicationInsightsDialog(IDialogController dialogController) : base(dialogController)
        {
        }
    }
}
