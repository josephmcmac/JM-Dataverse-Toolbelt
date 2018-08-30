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
                var themePath = "Resources/Themes/Light.xaml";
                if (theme != null)
                {
                    themePath = theme?.SelectedTheme?.ResourceRelativePathForLoading ?? themePath;
                }
                var res = System.Windows.Application.LoadComponent(
                     new Uri("/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + themePath,
                     UriKind.RelativeOrAbsolute)) as ResourceDictionary;
                userControl.Resources.MergedDictionaries.Add(res);
                System.Windows.Application.Current.MainWindow.Resources.MergedDictionaries.Add(res);
            }
        }

        private static ApplicationControllerBase GetAppController(this UserControl userControl)
        {
            var app = userControl.DataContext as ApplicationBase;
            return app?.Controller;
        }
    }
}
