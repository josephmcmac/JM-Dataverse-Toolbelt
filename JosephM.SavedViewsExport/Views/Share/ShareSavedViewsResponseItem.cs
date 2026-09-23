using JosephM.Core.Service;
using System;

namespace JosephM.UserSavedObjectsUtility.Views.Share
{
    public class ShareSavedViewsResponseItem : ServiceResponseItem
    {
        public string Name { get; set; }

        public ShareSavedViewsResponseItem(string name, Exception ex)
        {
            Name = name;
            Exception = ex;
        }
    }
}