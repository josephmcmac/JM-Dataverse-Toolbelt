using JosephM.Application.Desktop.Module.Dialog;
using JosephM.Core.AppConfig;

namespace JosephM.Application.Desktop.Module.Settings
{
    /// <summary>
    ///     Base Class For A Module Which Plugs A Settings Type Into The Application
    /// </summary>
    public class SettingsAggregatorModule : DialogModule<SettingsAggregatorDialog>
    {
        public override void RegisterTypes()
        {
            base.RegisterTypes();
            ApplicationController.RegisterInstance(new SettingsAggregator());
        }

        public override void InitialiseModule()
        {
            AddSetting(MainOperationName, DialogCommand, OperationDescription, 99998);
        }

        public override string MainOperationName => "App Settings";
    }
}