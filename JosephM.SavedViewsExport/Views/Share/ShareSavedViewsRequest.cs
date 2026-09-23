using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.Schema;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.UserSavedObjectsUtility.Views.Share
{
    [Group(Sections.CloneDetails, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10)]
    [Group(Sections.AccessOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10, selectAll: true)]
    public class ShareSavedViewsRequest : ServiceRequestBase
    {
        public ShareSavedViewsRequest(IEnumerable<SavedView> objectsToClone)
            : this()
        {
            _objectsToClone = objectsToClone;
        }

        public ShareSavedViewsRequest()
        {
        }

        private IEnumerable<SavedView> _objectsToClone { get; set; }

        public IEnumerable<SavedView> GetObjectsToClone()
        {
            return _objectsToClone;
        }

        [ReferencedType(Entities.systemuser)]
        [UsePicklist]
        [Group(Sections.CloneDetails)]
        [DisplayOrder(10)]
        [RequiredProperty]
        public Lookup ShareToUser { get; set; }

        [Group(Sections.CloneDetails)]
        [DisplayOrder(20)]
        public int RecordCount { get { return _objectsToClone?.Count() ?? 0; } }

        [Group(Sections.AccessOptions)]
        [DisplayOrder(30)]
        public bool IncludeWriteAccess { get; set; }

        [Group(Sections.AccessOptions)]
        [DisplayOrder(40)]
        public bool IncludeShareAccess { get; set; }

        [Group(Sections.AccessOptions)]
        [DisplayOrder(40)]
        public bool IncludeAssignAccess { get; set; }

        [Group(Sections.AccessOptions)]
        [DisplayOrder(50)]
        public bool IncludeDeleteAccess { get; set; }

        private static class Sections
        {
            public const string CloneDetails = "Clone Details";
            public const string AccessOptions = "Access Options";
        }
    }
}