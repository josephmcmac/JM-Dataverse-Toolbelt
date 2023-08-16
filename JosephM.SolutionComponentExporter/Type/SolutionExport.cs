using System;

namespace JosephM.SolutionComponentExporter.Type
{
    public class SolutionExport
    {
        public SolutionExport(string packageType, string displayName, string version, string publisher, string description, string modifiedBy, DateTime modifiedOn)
        {
            PackageType = packageType;
            DisplayName = displayName;
            Version = version;
            Publisher = publisher;
            Description = description;
            ModifiedBy = modifiedBy;
            ModifiedOn = modifiedOn;
        }

        public string PackageType { get; private set; }
        public string DisplayName { get; private set; }
        public string Version { get; private set; }
        public string Publisher { get; private set; }
        public string Description { get; private set; }
        public string ModifiedBy { get; private set; }
        public DateTime ModifiedOn { get; private set; }
    }
}