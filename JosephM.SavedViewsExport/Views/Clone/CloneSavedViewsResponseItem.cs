using JosephM.Core.Service;
using System;

namespace JosephM.UserSavedObjectsUtility.Views.Clone
{
    public class CloneSavedViewsResponseItem : ServiceResponseItem
    {
        public string Name { get; set; }

        public CloneSavedViewsResponseItem(string name, Exception ex)
        {
            Name = name;
            Exception = ex;
        }
    }
}