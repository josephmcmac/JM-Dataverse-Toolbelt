using System;

namespace JosephM.SolutionComponentExporter.Type
{
    public class ReportExport
    {
        public ReportExport(string name, string description, string viewableBy, string isCustomReport, string reportType, string modifiedBy, DateTime modifiedOn)
        {
            Name = name;
            Description = description;
            ViewableBy = viewableBy;
            IsCustomReport = isCustomReport;
            ReportType = reportType;
            ModifiedBy = modifiedBy;
            ModifiedOn = modifiedOn;
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public string ViewableBy { get; }
        public string IsCustomReport { get; }
        public string ReportType { get; }
        public string ModifiedBy { get; private set; }
        public DateTime ModifiedOn { get; private set; }
    }
}