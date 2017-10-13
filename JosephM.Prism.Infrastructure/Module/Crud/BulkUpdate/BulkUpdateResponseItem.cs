using JosephM.Core.Service;
using System;

namespace JosephM.Prism.Infrastructure.Module.Crud.BulkUpdate
{
    public class BulkUpdateResponseItem : ServiceResponseItem
    {
        public string ErrorType { get; set; }
        public string Type { get; set; }

        public BulkUpdateResponseItem(string errorType, string type, Exception ex)
        {
            ErrorType = errorType;
            Type = type;
            Exception = ex;
        }
    }
}