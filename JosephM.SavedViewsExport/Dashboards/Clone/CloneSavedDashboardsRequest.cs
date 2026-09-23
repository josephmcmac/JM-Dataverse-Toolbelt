using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.Schema;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.UserSavedObjectsUtility.Dashboards.Clone
{
    [Group(Sections.CloneDetails, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10)]
    public class CloneSavedDashboardsRequest : ServiceRequestBase
    {
        public CloneSavedDashboardsRequest(IEnumerable<SavedDashboard> objectsToClone)
            : this()
        {
            _objectsToClone = objectsToClone;
        }

        public CloneSavedDashboardsRequest()
        {
        }

        private IEnumerable<SavedDashboard> _objectsToClone { get; set; }

        public IEnumerable<SavedDashboard> GetObjectsToClone()
        {
            return _objectsToClone;
        }

        [ReferencedType(Entities.systemuser)]
        [UsePicklist]
        [Group(Sections.CloneDetails)]
        [DisplayOrder(10)]
        [RequiredProperty]
        public Lookup CloneToUser { get; set; }

        [Group(Sections.CloneDetails)]
        [DisplayOrder(20)]
        public int RecordCount { get { return _objectsToClone?.Count() ?? 0; } }

        private static class Sections
        {
            public const string CloneDetails = "Clone Details";
        }
    }
}