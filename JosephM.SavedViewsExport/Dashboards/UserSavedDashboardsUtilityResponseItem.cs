using JosephM.Core.Service;
using System;

namespace JosephM.UserSavedObjectsUtility.Dashboards
{
    public class UserSavedDashboardsUtilityResponseItem : ServiceResponseItem
    {
        public Guid UserId { get; set; }

        public string Username { get; set; }

        public UserSavedDashboardsUtilityResponseItem(Guid userId, string userName, Exception ex)
        {
            Exception = ex;
            UserId = userId;
            Username = userName;
        }
    }
}