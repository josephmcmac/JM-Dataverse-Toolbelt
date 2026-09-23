using JosephM.Core.Service;
using System;

namespace JosephM.UserSavedObjectsUtility.Charts.Share
{
    public class ShareSavedChartResponseItem : ServiceResponseItem
    {
        public string Name { get; set; }

        public ShareSavedChartResponseItem(string name, Exception ex)
        {
            Name = name;
            Exception = ex;
        }
    }
}