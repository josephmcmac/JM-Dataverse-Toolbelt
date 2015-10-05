using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module;

namespace JosephM.Migration.Prism.Module.MigrationSchedule
{
    public class MigrationScheduleModule : PrismModuleBase
    {
        public override void RegisterTypes()
        {
            AddOption("Migration Schedules", OpenList);
            RegisterTypeForNavigation<MigrationScheduleListDialog>();
        }

        public void OpenList()
        {
            NavigateTo<MigrationScheduleListDialog>();
        }

        public override void InitialiseModule()
        {

        }
    }
}
