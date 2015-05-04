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
    public class SettingsModule<TSettingsDialog, TSettingsInterface, TSettingsGetSetClass> : PrismModuleBase
        where TSettingsDialog : AppSettingsDialog<TSettingsInterface, TSettingsGetSetClass>
        where TSettingsGetSetClass : TSettingsInterface, new()
    {
        public override void RegisterTypes()
        {
            var configManager = Resolve<PrismSettingsManager>();
            var settings = configManager.Resolve<TSettingsGetSetClass>();
            Container.RegisterInstance((TSettingsInterface)settings);
            RegisterTypeForNavigation<TSettingsDialog>();
        }

        public override void InitialiseModule()
        {
            ApplicationOptions.AddSetting(typeof(TSettingsGetSetClass).GetDisplayName(), MenuNames.Settings, TextSearchSettings);
        }

        private void TextSearchSettings()
        {
            NavigateTo<TSettingsDialog>();
        }
    }
}