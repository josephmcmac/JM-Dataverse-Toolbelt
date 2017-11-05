using JosephM.Core.Attributes;
using JosephM.Core.Service;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.RecordCounts.Exporter
{
    public class RecordCountsResponse : ServiceResponseBase<RecordCountsResponseItem>
    {
        [AllowDownload]
        [PropertyInContextByPropertyValue(nameof(AreRecordCountsByOwner), false)]
        public IEnumerable<RecordCount> RecordCounts { get; set; }

        [Hidden]
        public bool AreRecordCountsByOwner
        {
            get
            {
                return RecordCountsByOwner.Any();
            }
        }

        [AllowDownload]
        [PropertyInContextByPropertyValue(nameof(AreRecordCountsByOwner), true)]
        public IEnumerable<RecordCountByOwner> RecordCountsByOwner
        {
            get
            {
                return RecordCounts
                    .Where(rc => rc is RecordCountByOwner)
                    .Cast<RecordCountByOwner>()
                    .ToArray();
            }
        }
    }
}