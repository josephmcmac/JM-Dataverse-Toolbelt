using JosephM.Core.Attributes;
using JosephM.Core.Service;
using System.Collections.Generic;

namespace JosephM.UserSavedObjectsUtility.Dashboards
{
    public class UserSavedDashboardsUtilityResponse : ServiceResponseBase<UserSavedDashboardsUtilityResponseItem>
    {
        [DoNotAllowGridOpen]
        [AllowDownload]
        public IEnumerable<SavedDashboard> SavedDashboards { get; set; }
    }
}