using JosephM.Application.Application;
using JosephM.Application.Desktop.Module.Settings;

namespace JosephM.TestModule.TestSettings
{
    public class TestSettingsModule : SettingsModule<TestSettingsDialog, ITestSettings, TestSettings>
    {
        public override void RegisterTypes()
        {
            var configManager = Resolve<ISettingsManager>();
            configManager.ProcessNamespaceChange(GetType().Namespace, "JosephM.Prism.TestModule.Prism.TestSettings");
            base.RegisterTypes();
        }
    }
}