using System;
using JosephM.Core.Service;

namespace JosephM.CustomisationExporter
{
    public class CustomisationExporterResponseItem : ServiceResponseItem
    {
        public string Type { get; set; }
        public string SchemaName { get; set; }

        public CustomisationExporterResponseItem(string type, string schemaName, Exception ex)
        {
            Type = type;
            SchemaName = schemaName;
            Exception = ex;
        }
    }
}