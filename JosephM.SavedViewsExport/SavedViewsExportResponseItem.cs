using System;
using JosephM.Core.Service;

namespace JosephM.SavedViewsExport
{
    public class SavedViewsExportResponseItem : ServiceResponseItem
    {
        public Guid UserId { get; set; }

        public string Username { get; set; }

        public SavedViewsExportResponseItem(Guid userId, string userName, Exception ex)
        {
            Exception = ex;
            UserId = userId;
            Username = userName;
        }
    }
}