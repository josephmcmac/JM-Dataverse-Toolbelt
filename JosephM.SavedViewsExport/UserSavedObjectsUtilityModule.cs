using JosephM.Application.Modules;
using JosephM.UserSavedObjectsUtility.Charts;
using JosephM.UserSavedObjectsUtility.Dashboards;
using JosephM.UserSavedObjectsUtility.Views;

namespace JosephM.UserSavedObjectsUtility
{
    [DependantModule(typeof(UserSavedViewsUtilityModule))]
    [DependantModule(typeof(UserSavedDashboardsUtilityModule))]
    [DependantModule(typeof(UserSavedChartsUtilityModule))]
    public class UserSavedObjectsUtilityModule : ModuleBase
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
        }
    }
}
