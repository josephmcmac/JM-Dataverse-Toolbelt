#region

using System;
using System.IO;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Record.IService;

#endregion

namespace JosephM.Deployment
{
    public class DataImportResponseItem : ServiceResponseItem
    {
        public string Entity { get; set; }

        public string Name { get; set; }

        public string Field { get; set; }

        public string Message { get; set; }

        public DataImportResponseItem(string entity, string field, string name, string message, Exception ex)
            : this(message, ex)
        {
            Entity = entity;
            Field = field;
            Name = name;
        }

        public DataImportResponseItem(string message, Exception ex)
        {
            Message = message;
            Exception = ex;
        }
    }
}