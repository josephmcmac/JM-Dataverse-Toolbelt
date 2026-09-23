using JosephM.Core.Service;
using System;

namespace JosephM.UserSavedObjectsUtility.Dashboards.Share
{
    public class ShareSavedDashboardResponseItem : ServiceResponseItem
    {
        public string Name { get; set; }

        public ShareSavedDashboardResponseItem(string name, Exception ex)
        {
            Name = name;
            Exception = ex;
        }
    }
}