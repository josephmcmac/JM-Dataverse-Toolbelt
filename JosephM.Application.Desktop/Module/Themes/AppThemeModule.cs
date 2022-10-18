using JosephM.Application.Modules;
using System.Windows.Controls;

namespace JosephM.Application.Desktop.Module.Themes
{
    public abstract class AppThemeModule<T> : ModuleBase
        where T : UserControl
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
            ApplicationController.AppImageType = typeof(T);
        }
    }
}
