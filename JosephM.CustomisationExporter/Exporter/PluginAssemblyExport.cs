using System;

namespace JosephM.CustomisationExporter.Exporter
{
    public class PluginAssemblyExport
    {
        public PluginAssemblyExport(string name, string isolationMode, string sourceType, string version, string modifiedBy, DateTime modifiedOn)
        {
            Name = name;
            IsolationMode = isolationMode;
            SourceTyoe = sourceType;
            Version = version;
            ModifiedBy = modifiedBy;
            ModifiedOn = modifiedOn;
        }

        public string Name { get; private set; }
        public string IsolationMode { get; private set; }
        public string SourceTyoe { get; private set; }
        public string Version { get; private set; }
        public string ModifiedBy { get; private set; }
        public DateTime ModifiedOn { get; private set; }
    }
}