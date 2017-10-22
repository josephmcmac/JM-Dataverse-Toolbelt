using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.InstanceComparer
{
    [Group(Sections.Summary, true)]
    public class InstanceComparerTypeSummary
    {
        public static IEnumerable<InstanceComparerTypeSummary> CreateSummaries(IEnumerable<InstanceComparerDifference> differences)
        {
            return differences
                .GroupBy(d => d.Type)
                .Select(g => new InstanceComparerTypeSummary(g.Key, g.ToArray()))
                .ToArray();
        }

        public InstanceComparerTypeSummary(string type, IEnumerable<InstanceComparerDifference> differences)
        {
            Type = type;
            Differences = differences;
        }

        [GridField]
        [Group(Sections.Summary)]
        [DisplayOrder(10)]
        public string Type { get; }
        [GridWidth(120)]
        [GridField]
        [Group(Sections.Summary)]
        [DisplayOrder(20)]
        public int Total { get { return Differences.Count(); } }
        [GridWidth(120)]
        [GridField]
        [Group(Sections.Summary)]
        [DisplayOrder(30)]
        public int OnlyIn1 { get { return Differences.Count(d => d.Id2 == null); } }
        [GridWidth(120)]
        [GridField]
        [Group(Sections.Summary)]
        [DisplayOrder(40)]
        public int OnlyIn2 { get { return Differences.Count(d => d.Id1 == null); } }
        [GridWidth(120)]
        [GridField]
        [Group(Sections.Summary)]
        [DisplayOrder(50)]
        public int Different { get { return Differences.Count(d => d.Id1 != null && d.Id2 != null); } }

        [DisplayOrder(1000)]
        [AllowDownload]
        public IEnumerable<InstanceComparerDifference> Differences { get; }

        private static class Sections
        {
            public const string Summary = "Summary";
        }
    }
}
