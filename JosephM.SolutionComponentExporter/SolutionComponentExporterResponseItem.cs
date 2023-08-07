using JosephM.Core.Service;
using System;

namespace JosephM.SolutionComponentExporter
{
    public class SolutionComponentExporterResponseItem : ServiceResponseItem
    {
        public string Type { get; set; }
        public string SchemaName { get; set; }

        public SolutionComponentExporterResponseItem(string type, string schemaName, Exception ex)
        {
            Type = type;
            SchemaName = schemaName;
            Exception = ex;
        }
    }
}