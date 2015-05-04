using JosephM.Prism.Infrastructure.Constants;
using JosephM.Prism.Infrastructure.Module;

namespace JosephM.Prism.XrmTestModule.TestXrmSettingsDialog
{
    public class XrmSettingsModule : PrismModuleBase
    {
        public override void RegisterTypes()
        {
            RegisterTypeForNavigation<XrmSettingsDialog>();
        }

        public override void InitialiseModule()
        {
            ApplicationOptions.AddOption("Save Xrm Test Connection", MenuNames.Crm, DialogCommand);
        }

        private void DialogCommand()
        {
            NavigateTo<XrmSettingsDialog>();
        }
    }
}