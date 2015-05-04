using JosephM.Prism.Infrastructure.Attributes;
using JosephM.Prism.Infrastructure.Module;

namespace JosephM.Prism.TestModule.SearchModule
{
    public class SearchModule : PrismModuleBase
    {
        public override void RegisterTypes()
        {
            RegisterTypeForNavigation<SearchDialog>();
        }

        public override void InitialiseModule()
        {
            this.ApplicationOptions.AddOption("Search", "Main", StartSearch);
        }

        private void StartSearch()
        {
            NavigateTo<SearchDialog>();
        }
    }
}