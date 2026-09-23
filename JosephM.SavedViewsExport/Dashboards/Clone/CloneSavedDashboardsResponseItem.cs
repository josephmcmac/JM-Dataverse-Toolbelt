using JosephM.Core.Service;
using System;

namespace JosephM.UserSavedObjectsUtility.Dashboards.Clone
{
    public class CloneSavedDashboardsResponseItem : ServiceResponseItem
    {
        public string Name { get; set; }

        public CloneSavedDashboardsResponseItem(string name, Exception ex)
        {
            Name = name;
            Exception = ex;
        }
    }
}