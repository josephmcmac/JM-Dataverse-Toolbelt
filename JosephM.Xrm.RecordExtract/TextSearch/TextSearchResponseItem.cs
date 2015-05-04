using System;
using JosephM.Core.Service;
using JosephM.Xrm.RecordExtract.RecordExtract;

namespace JosephM.Xrm.RecordExtract.TextSearch
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

        public TextSearchResponseItem(RecordExtractResponseItem responseItem)
        {
            Type = responseItem.Type;
            SchemaName = responseItem.SchemaName;
            Exception = responseItem.Exception;
        }
    }
}