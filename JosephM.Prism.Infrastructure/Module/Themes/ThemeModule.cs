using JosephM.Application.Desktop.Module.Settings;
using JosephM.Wpf.Resources.Themes;

namespace JosephM.Application.Desktop.Module.Themes
{
    public class ThemeModule : SettingsModule<ThemeDialog, Theme, Theme>
    {
        public override void RegisterTypes()
        {
            base.RegisterTypes();
            RegisterInstance(XamlThemes.LoadThemes());
        }
    }
}
