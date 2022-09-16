using JosephM.Core.Service;
using System;

namespace JosephM.Xrm.TextSearch
{
    public class TextSearchResponseItem : ServiceResponseItem
    {
        public string Type { get; set; }
        public string SchemaName { get; set; }
        public string FieldName { get; set; }

        public TextSearchResponseItem(string type, string schemaName, Exception ex)
        {
            Type = type;
            SchemaName = schemaName;
            Exception = ex;
        }

        public TextSearchResponseItem(string type, string schemaName, string fieldName, Exception ex)
        {
            Type = type;
            SchemaName = schemaName;
            FieldName = fieldName;
            Exception = ex;
        }
    }
}