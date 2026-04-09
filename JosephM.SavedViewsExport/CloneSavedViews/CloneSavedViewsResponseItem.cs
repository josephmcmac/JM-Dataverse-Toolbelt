using JosephM.Core.Service;
using System;

namespace JosephM.SavedViewsExport.CloneSavedViews
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