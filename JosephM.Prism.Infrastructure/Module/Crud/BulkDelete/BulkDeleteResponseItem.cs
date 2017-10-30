using JosephM.Core.Service;
using System;

namespace JosephM.Prism.Infrastructure.Module.Crud.BulkDelete
{
    public class BulkDeleteResponseItem : ServiceResponseItem
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public BulkDeleteResponseItem(string id, string name, Exception ex)
        {
            Id = id;
            Name = name;
            Exception = ex;
        }
    }
}