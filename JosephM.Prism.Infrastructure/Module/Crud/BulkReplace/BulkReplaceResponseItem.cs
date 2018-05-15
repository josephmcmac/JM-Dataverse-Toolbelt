using JosephM.Core.Service;
using System;

namespace JosephM.Application.Desktop.Module.Crud.BulkReplace
{
    public class BulkReplaceResponseItem : ServiceResponseItem
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public BulkReplaceResponseItem(string id, string name, Exception ex)
        {
            Id = id;
            Name = name;
            Exception = ex;
        }
    }
}