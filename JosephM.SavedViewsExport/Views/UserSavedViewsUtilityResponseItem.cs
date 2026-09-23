using JosephM.Core.Service;
using System;

namespace JosephM.UserSavedObjectsUtility.Views
{
    public class UserSavedViewsUtilityResponseItem : ServiceResponseItem
    {
        public Guid UserId { get; set; }

        public string Username { get; set; }

        public UserSavedViewsUtilityResponseItem(Guid userId, string userName, Exception ex)
        {
            Exception = ex;
            UserId = userId;
            Username = userName;
        }
    }
}