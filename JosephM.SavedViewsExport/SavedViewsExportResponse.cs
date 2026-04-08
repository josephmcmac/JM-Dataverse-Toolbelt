using JosephM.Core.Attributes;
using JosephM.Core.Service;
using System.Collections.Generic;

namespace JosephM.SavedViewsExport
{
    public class SavedViewsExportResponse : ServiceResponseBase<SavedViewsExportResponseItem>
    {
        [DoNotAllowGridOpen]
        [AllowDownload]
        public IEnumerable<SavedView> SavedViewsExport { get; set; }
    }
}