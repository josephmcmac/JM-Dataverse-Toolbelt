using JosephM.Core.Service;
using System;

namespace JosephM.Application.Desktop.Module.Crud.BulkUpdate
{
    public class BulkUpdateResponseItem : ServiceResponseItem
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public BulkUpdateResponseItem(string id, string name, Exception ex)
        {
            Id = id;
            Name = name;
            Exception = ex;
        }
    }
}