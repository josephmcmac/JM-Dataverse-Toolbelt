using System;

namespace JosephM.CustomisationExporter.Type
{
    public class PluginTriggerExport
    {
        public PluginTriggerExport(string name, string assemblyName, string entityType, string stage, string mode, string filteringAttributes, string impersonatingUser, string description, string supportedDeployment, string state, string modifiedBy, DateTime modifiedOn)
        {
            Name = name;
            AssemblyName = assemblyName;
            EntityType = entityType;
            Stage = stage;
            Mode = mode;
            FilteringAttributes = filteringAttributes;
            ImpersonatingUser = impersonatingUser;
            Description = description;
            SupportedDeployment = supportedDeployment;
            State = state;
            ModifiedBy = modifiedBy;
            ModifiedOn = modifiedOn;
        }

        public string Name { get; set; }
        public string AssemblyName { get; }
        public string EntityType { get; }
        public string Stage { get; }
        public string Mode { get; }
        public string FilteringAttributes { get; }
        public string ImpersonatingUser { get; }
        public string Description { get; }
        public string SupportedDeployment { get; }
        public string State { get; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}