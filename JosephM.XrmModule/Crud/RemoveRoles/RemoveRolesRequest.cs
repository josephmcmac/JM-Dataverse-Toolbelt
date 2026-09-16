using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.IService;
using JosephM.Xrm.Schema;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.XrmModule.Crud.RemoveRoles
{
    [Group(Sections.PrincipalDetails, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10, displayLabel: false)]
    [Group(Sections.Role, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20, displayLabel: false)]
    public class RemoveRolesRequest : ServiceRequestBase
    {
        public RemoveRolesRequest(IEnumerable<IRecord> principalsToUpdate)
            : this()
        {
            _principalsToUpdate = principalsToUpdate;
        }

        public RemoveRolesRequest()
        {
        }

        private IEnumerable<IRecord> _principalsToUpdate { get; set; }

        public IEnumerable<IRecord> GetPrincipalsToUpdate()
        {
            return _principalsToUpdate;
        }

        [Group(Sections.PrincipalDetails)]
        [DisplayOrder(20)]
        [PropertyInContextByPropertyValue(nameof(IsUsers), true)]
        public int UserCount { get { return _principalsToUpdate?.Count(p => p.Type == Entities.systemuser) ?? 0; } }

        [Group(Sections.PrincipalDetails)]
        [DisplayOrder(20)]
        [PropertyInContextByPropertyValue(nameof(IsTeams), true)]
        public int TeamCount { get { return _principalsToUpdate?.Count(p => p.Type == Entities.team) ?? 0; } }

        [Hidden]
        public bool IsUsers { get { return _principalsToUpdate.All(p => p.Type == Entities.systemuser); } }

        [Hidden]
        public bool IsTeams { get { return _principalsToUpdate.All(p => p.Type == Entities.team); } }

        [Group(Sections.Role)]
        [DisplayOrder(24)]
        [RequiredProperty]
        [ReferencedType(Entities.role)]
        [UsePicklist(Fields.role_.name, Fields.role_.businessunitid)]
        public Lookup SecurityRoleToRemove { get; set; }

        private static class Sections
        {
            public const string PrincipalDetails = "Items To Process";
            public const string Role = "Security Role to Remove";
        }
    }
}