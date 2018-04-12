using JosephM.Application.Modules;

namespace JosephM.XrmTestModule.TestSettings
{
    public class XrmSettingsModule : ModuleBase
    {
        public override void RegisterTypes()
        {
            RegisterTypeForNavigation<XrmSettingsDialog>();
        }

        public override void InitialiseModule()
        {
            AddOption("TESTSETTINGS", "Save Xrm Test Connection", DialogCommand);
        }

        private void DialogCommand()
        {
            NavigateTo<XrmSettingsDialog>();
        }
    }
}