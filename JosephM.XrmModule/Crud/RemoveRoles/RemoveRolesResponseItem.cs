using JosephM.Core.Service;
using System;

namespace JosephM.XrmModule.Crud.RemoveRoles
{
    public class RemoveRolesResponseItem : ServiceResponseItem
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public RemoveRolesResponseItem(string id, string name, Exception ex)
        {
            Id = id;
            Name = name;
            Exception = ex;
        }
    }
}