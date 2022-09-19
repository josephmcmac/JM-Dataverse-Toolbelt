using System;

namespace JosephM.CustomisationExporter.Type
{
    public class WebResourcesExport
    {
        public WebResourcesExport(string name, string displayName, string type, string description, string modifiedBy, DateTime modifiedOn)
        {
            Name = name;
            DisplayName = displayName;
            Type = type;
            Description = description;
            ModifiedBy = modifiedBy;
            ModifiedOn = modifiedOn;
        }

        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public string Type { get; }
        public string Description { get; }
        public string ModifiedBy { get; private set; }
        public DateTime ModifiedOn { get; private set; }
    }
}