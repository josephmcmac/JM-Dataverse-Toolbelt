#region

using JosephM.Prism.Infrastructure.Constants;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.Xrm;
using JosephM.Record.Application.Navigation;

#endregion

namespace JosephM.Prism.XrmTestModule.Prism
{
    public class XrmTestModule : PrismModuleBase
    {
        public override void InitialiseModule()
        {
            ApplicationOptions.AddOption("Create Test Crm Record", MenuNames.Crm, CreateCommand);
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