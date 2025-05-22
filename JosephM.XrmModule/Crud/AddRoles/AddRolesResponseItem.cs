using JosephM.Core.Service;
using System;

namespace JosephM.XrmModule.Crud.AddRoles
{
    public class AddRolesResponseItem : ServiceResponseItem
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public AddRolesResponseItem(string id, string name, Exception ex)
        {
            Id = id;
            Name = name;
            Exception = ex;
        }
    }
}