using JosephM.Core.Service;

namespace JosephM.InstanceComparer
{
    public class InstanceComparerResponse : ServiceResponseBase<InstanceComparerResponseItem>
    {
        public string FileName { get; set; }
        public bool Differences { get; set; }
    }
}