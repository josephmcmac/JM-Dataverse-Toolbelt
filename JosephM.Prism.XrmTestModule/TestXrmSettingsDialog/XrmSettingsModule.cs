using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Constants;
using JosephM.Prism.Infrastructure.Module;

namespace JosephM.Prism.XrmTestModule.TestXrmSettingsDialog
{
    public class XrmSettingsModule : ModuleBase
    {
        public override void RegisterTypes()
        {
            RegisterTypeForNavigation<XrmSettingsDialog>();
        }

        public override void InitialiseModule()
        {
            AddOption("Save Xrm Test Connection", DialogCommand);
        }

        private void DialogCommand()
        {
            NavigateTo<XrmSettingsDialog>();
        }
    }
}