using System;

namespace JosephM.CustomisationExporter.Exporter
{
    public class PluginAssemblyExport
    {
        public PluginAssemblyExport(string name, string isolationMode, string sourceType, string version, string modifiedBy, DateTime modifiedOn)
        {
            Name = name;
            IsolationMode = isolationMode;
            SourceType = sourceType;
            Version = version;
            ModifiedBy = modifiedBy;
            ModifiedOn = modifiedOn;
        }

        public string Name { get; private set; }
        public string IsolationMode { get; private set; }
        public string SourceType { get; private set; }
        public string Version { get; private set; }
        public string ModifiedBy { get; private set; }
        public DateTime ModifiedOn { get; private set; }
    }
}