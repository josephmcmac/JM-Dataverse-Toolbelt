using JosephM.Core.Service;
using System;

namespace JosephM.UserSavedObjectsUtility.Charts.Clone
{
    public class CloneSavedChartsResponseItem : ServiceResponseItem
    {
        public string Name { get; set; }

        public CloneSavedChartsResponseItem(string name, Exception ex)
        {
            Name = name;
            Exception = ex;
        }
    }
}