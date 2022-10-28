using JosephM.Core.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.InstanceComparer
{
    [Group(Sections.Summary, Group.DisplayLayoutEnum.HorizontalLabelAbove)]
    public class InstanceComparerTypeSummary
    {
        public static IEnumerable<InstanceComparerTypeSummary> CreateSummaries(IEnumerable<InstanceComparerDifference> differences)
        {
            var summary = differences
                .GroupBy(d => d.Type)
                .Select(g => new InstanceComparerTypeSummary(g.Key, g.ToArray()))
                .ToArray();
            return summary.Any()
                ? summary
                : new[] { new InstanceComparerTypeSummary("No Differences Found", null) };
        }

        public InstanceComparerTypeSummary(string type, IEnumerable<InstanceComparerDifference> differences)
        {
            Type = type;
            Differences = differences ?? new InstanceComparerDifference[0];
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
        [PropertyInContextByPropertyValue(nameof(HasDifferences), true)]
        public IEnumerable<InstanceComparerDifference> Differences { get; }

        [Hidden]
        public bool HasDifferences {  get { return Differences != null && Differences.Any(); } }

        private static class Sections
        {
            public const string Summary = "Summary";
        }
    }
}
