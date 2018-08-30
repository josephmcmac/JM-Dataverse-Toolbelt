using JosephM.Application.Desktop.Module.Settings;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Wpf.Resources.Themes;

namespace JosephM.Application.Desktop.Module.Themes
{
    public class ThemeDialog : AppSettingsDialog<Theme, Theme>
    {
        public ThemeDialog(IDialogController dialogController)
            : base(dialogController)
        {
        }
    }
}