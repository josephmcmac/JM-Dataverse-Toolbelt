using JosephM.Core.Attributes;
using JosephM.Core.Service;
using System.Collections.Generic;

namespace JosephM.UserSavedObjectsUtility.Charts
{
    public class UserSavedChartsUtilityResponse : ServiceResponseBase<UserSavedChartsUtilityResponseItem>
    {
        [DoNotAllowGridOpen]
        [AllowDownload]
        public IEnumerable<SavedChart> SavedCharts { get; set; }
    }
}