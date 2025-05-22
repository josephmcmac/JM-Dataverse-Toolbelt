using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.IService;
using JosephM.Xrm.Schema;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.XrmModule.Crud.RemoveRoles
{
    [Group(Sections.UsersDetails, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10, displayLabel: false)]
    [Group(Sections.Role, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20, displayLabel: false)]
    public class RemoveRolesRequest : ServiceRequestBase
    {
        public RemoveRolesRequest(IEnumerable<IRecord> usersToUpdate)
            : this()
        {
            _usersToUpdate = usersToUpdate;
        }

        public RemoveRolesRequest()
        {
        }

        private IEnumerable<IRecord> _usersToUpdate { get; set; }

        public IEnumerable<IRecord> GetUsersToUpdate()
        {
            return _usersToUpdate;
        }

        [Group(Sections.UsersDetails)]
        [DisplayOrder(20)]
        public int UserCount { get { return _usersToUpdate?.Count() ?? 0; } }

        [Group(Sections.Role)]
        [DisplayOrder(24)]
        [RequiredProperty]
        [ReferencedType(Entities.role)]
        [UsePicklist(Fields.role_.name, Fields.role_.businessunitid)]
        public Lookup SecurityRoleToRemove { get; set; }

        private static class Sections
        {
            public const string UsersDetails = "Users Update";
            public const string Role = "Security Role to Remove";
        }
    }
}