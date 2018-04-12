using JosephM.Application.Application;
using JosephM.Application.Desktop.Module.Dialog;
using JosephM.Core.AppConfig;

namespace JosephM.Application.Desktop.Module.Settings
{
    /// <summary>
    ///     Base Class For A Module Which Plugs A Settings Type Into The Application
    /// </summary>
    public abstract class SettingsModule<TSettingsDialog, TSettingsInterface, TSettingsGetSetClass> : DialogModule<TSettingsDialog>
        where TSettingsDialog : AppSettingsDialog<TSettingsInterface, TSettingsGetSetClass>
        where TSettingsGetSetClass : TSettingsInterface, new()
    {
        public override void RegisterTypes()
        {
            base.RegisterTypes();
            var configManager = Resolve<ISettingsManager>();
            var settings = configManager.Resolve<TSettingsGetSetClass>();
            Container.RegisterInstance<TSettingsInterface>(settings);
        }

        public override void InitialiseModule()
        {
            AddSetting(MainOperationName, DialogCommand, OperationDescription);
        }
    }
}