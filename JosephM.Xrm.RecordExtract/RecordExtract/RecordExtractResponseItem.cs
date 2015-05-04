using System;
using JosephM.Core.Service;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    public class RecordExtractResponseItem : ServiceResponseItem
    {
        public string Type { get; set; }
        public string SchemaName { get; set; }

        public RecordExtractResponseItem(string type, string schemaName, Exception ex)
        {
            Type = type;
            SchemaName = schemaName;
            Exception = ex;
        }
    }
}