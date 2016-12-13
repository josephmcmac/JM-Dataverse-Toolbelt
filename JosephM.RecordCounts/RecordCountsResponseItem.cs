using System;
using JosephM.Core.Service;

namespace JosephM.RecordCounts.Exporter
{
    public class RecordCountsResponseItem : ServiceResponseItem
    {
        public string RecordType { get; set; }

        public string Message { get; set; }

        public RecordCountsResponseItem(string recordType, string message, Exception ex)
        {
            Exception = ex;
            RecordType = recordType;
            Message = message;
        }
    }
}