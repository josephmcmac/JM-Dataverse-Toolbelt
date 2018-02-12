using JosephM.Application.Application;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Prism.Infrastructure.Dialog;

namespace JosephM.Prism.Infrastructure.Module
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