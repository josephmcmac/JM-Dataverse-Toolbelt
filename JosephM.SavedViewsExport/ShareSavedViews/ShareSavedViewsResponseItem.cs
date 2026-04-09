using JosephM.Core.Service;
using System;

namespace JosephM.SavedViewsExport.ShareSavedViews
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