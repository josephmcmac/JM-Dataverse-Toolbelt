using JosephM.Core.Service;
using System.Collections.Generic;

namespace JosephM.InstanceComparer
{
    public class InstanceComparerResponse : ServiceResponseBase<InstanceComparerResponseItem>
    {
        public string FileName { get; set; }
        public bool AreDifferences { get; set; }

        public IEnumerable<InstanceComparerDifference> Differences { get; set; }
    }
}