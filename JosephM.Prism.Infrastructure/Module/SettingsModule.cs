using JosephM.Application.Application;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Constants;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Prism;

namespace JosephM.Prism.Infrastructure.Module
{
    /// <summary>
    ///     Base Class For A Module Which Plugs A Settings Type Into The Application
    /// </summary>
    public class SettingsModule<TSettingsDialog, TSettingsInterface, TSettingsGetSetClass> : DialogModule<TSettingsDialog>
        where TSettingsDialog : AppSettingsDialog<TSettingsInterface, TSettingsGetSetClass>
        where TSettingsGetSetClass : TSettingsInterface, new()
    {
        public override void RegisterTypes()
        {
            base.RegisterTypes();
            var configManager = Resolve<PrismSettingsManager>();
            var settings = configManager.Resolve<TSettingsGetSetClass>();
            Container.RegisterInstance<TSettingsInterface>(settings);
        }

        public override void InitialiseModule()
        {
            AddSetting(typeof(TSettingsGetSetClass).GetDisplayName(), DialogCommand);
        }
    }
}