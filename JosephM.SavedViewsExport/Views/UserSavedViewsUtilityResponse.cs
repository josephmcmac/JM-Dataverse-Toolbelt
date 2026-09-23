using JosephM.Core.Attributes;
using JosephM.Core.Service;
using System.Collections.Generic;

namespace JosephM.UserSavedObjectsUtility.Views
{
    public class UserSavedViewsUtilityResponse : ServiceResponseBase<UserSavedViewsUtilityResponseItem>
    {
        [DoNotAllowGridOpen]
        [AllowDownload]
        public IEnumerable<SavedView> SavedViews { get; set; }
    }
}