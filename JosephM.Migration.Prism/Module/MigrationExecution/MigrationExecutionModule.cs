using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module;

namespace JosephM.Migration.Prism.Module.MigrationExecution
{
    public class MigrationExecutionModule : PrismModuleBase
    {
        public override void RegisterTypes()
        {
            AddOption("Migration Executions", OpenList);
            RegisterTypeForNavigation<MigrationExecutionListDialog>();
            RegisterTypeForNavigation<MigrationExecutionDialog>();
        }

        public void OpenList()
        {
            NavigateTo<MigrationExecutionListDialog>();
        }

        public override void InitialiseModule()
        {

        }

        
    }
}