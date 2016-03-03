#region

using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Navigation;
using JosephM.Prism.Infrastructure.Constants;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.Xrm;

#endregion

namespace JosephM.Prism.XrmTestModule.Prism
{
    public class XrmTestModule : ModuleBase
    {
        public override void InitialiseModule()
        {
            AddOption("Create Test Crm Record", CreateCommand);
        }

        public override void RegisterTypes()
        {
        }

        private void CreateCommand()
        {
            var uriQuery = new UriQuery();
            uriQuery.Add(NavigationParameters.RecordType, "new_testentity");
            NavigateTo<XrmCreateViewModel>(uriQuery);
        }
    }
}