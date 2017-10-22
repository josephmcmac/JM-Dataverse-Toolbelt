using JosephM.Core.Attributes;
using JosephM.Core.Service;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.InstanceComparer
{
    public class InstanceComparerResponse : ServiceResponseBase<InstanceComparerResponseItem>
    {
        [Hidden]
        public bool AreDifferences { get { return AllDifferences != null && AllDifferences.Any(); } }

        [DisplayOrder(10)]
        [AllowDownload]
        public IEnumerable<InstanceComparerTypeSummary> Summary
        {
            get
            {
                return InstanceComparerTypeSummary.CreateSummaries(AllDifferences);
            }
        }

        [DisplayOrder(20)]
        [AllowDownload]
        [PropertyInContextByPropertyValue(nameof(AreDifferences), true)]
        public IEnumerable<InstanceComparerDifference> AllDifferences { get; set; }

    }
}