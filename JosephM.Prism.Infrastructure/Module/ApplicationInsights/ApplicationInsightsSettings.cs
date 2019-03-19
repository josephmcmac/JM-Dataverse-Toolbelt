using JosephM.Core.Attributes;

namespace JosephM.Application.Desktop.Module.ApplicationInsights
{
    public class ApplicationInsightsSettings
    {
        public bool AllowUsageStatistics { get; set; }
        [Hidden]
        public bool AllowUsageStatisticsLoaded { get; set; }
    }
}