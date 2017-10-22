using JosephM.Core.Attributes;
using JosephM.Core.Service;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.InstanceComparer
{
    public class InstanceComparerResponse : ServiceResponseBase<InstanceComparerResponseItem>
    {
        [Hidden]
        public bool AreDifferences { get { return Differences != null && Differences.Any(); } }

        [AllowDownload]
        [PropertyInContextByPropertyValue(nameof(AreDifferences), true)]
        public IEnumerable<InstanceComparerDifference> Differences { get; set; }
    }
}