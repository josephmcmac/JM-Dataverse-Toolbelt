using JosephM.Core.Service;
using System;

namespace JosephM.XrmModule.Crud.BulkWorkflow
{
    public class BulkWorkflowResponseItem : ServiceResponseItem
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public BulkWorkflowResponseItem(string id, string name, Exception ex)
        {
            Id = id;
            Name = name;
            Exception = ex;
        }
    }
}