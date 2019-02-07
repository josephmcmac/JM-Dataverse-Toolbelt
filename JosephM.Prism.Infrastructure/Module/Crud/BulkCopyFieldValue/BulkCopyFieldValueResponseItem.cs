using JosephM.Core.Service;
using System;

namespace JosephM.Application.Desktop.Module.Crud.BulkCopyFieldValue
{
    public class BulkCopyFieldValueResponseItem : ServiceResponseItem
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public BulkCopyFieldValueResponseItem(string id, string name, Exception ex)
        {
            Id = id;
            Name = name;
            Exception = ex;
        }
    }
}