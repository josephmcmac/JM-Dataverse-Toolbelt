using JosephM.Core.Service;
using System;

namespace JosephM.InstanceComparer
{
    public class InstanceComparerResponseItem : ServiceResponseItem
    {
        public string ErrorType { get; set; }
        public string Type { get; set; }

        public InstanceComparerResponseItem(string errorType, string type)
        {
            ErrorType = errorType;
            Type = type;
        }

        public InstanceComparerResponseItem(string errorType, string type, Exception ex)
        {
            ErrorType = errorType;
            Type = type;
            Exception = ex;
        }
    }
}