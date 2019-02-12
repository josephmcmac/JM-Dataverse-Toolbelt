using JosephM.Application.Application;
using JosephM.Wpf.Resources.Themes;
using System.Windows.Controls;
using JosephM.Core.AppConfig;
using System.Reflection;
using System.Windows;
using System;

namespace JosephM.Wpf.Extentions
{
    public static class UserControlExtentions
    {
        public static void DoThemeLoading(this UserControl userControl)
        {
            var appController = GetAppController(userControl);
            if (appController != null)
            {
                appController.AddOnInstanceRegistered<Theme>(() => appController.DoOnMainThread(() => ReloadTheme(userControl)));
            }
            ReloadTheme(userControl);
        }

        private static void ReloadTheme(UserControl userControl)
        {
            var appController = GetAppController(userControl);
            if (appController != null)
            {
                var theme = appController.ResolveType<Theme>();
                var themePath = "Resources/Themes/Dark.xaml";
                if (theme != null)
                {
                    themePath = theme?.SelectedTheme?.ResourceRelativePathForLoading ?? themePath;
                }
                var res = System.Windows.Application.LoadComponent(
                     new Uri("/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + themePath,
                     UriKind.RelativeOrAbsolute)) as ResourceDictionary;
                userControl.Resources.MergedDictionaries.Add(res);
                var window = Window.GetWindow(userControl);
                window.Resources.MergedDictionaries.Clear();
                window.Resources.MergedDictionaries.Add(res);
            }
        }

        private static ApplicationControllerBase GetAppController(this UserControl userControl)
        {
            if (userControl.DataContext is ApplicationBase)
                return ((ApplicationBase)userControl.DataContext).Controller;
            if (userControl.DataContext is ApplicationControllerBase)
                return (ApplicationControllerBase)userControl.DataContext;
            return null;
        }
    }
}
