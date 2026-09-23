using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.Schema;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.UserSavedObjectsUtility.Views.Clone
{
    [Group(Sections.CloneDetails, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10)]
    public class CloneSavedViewsRequest : ServiceRequestBase
    {
        public CloneSavedViewsRequest(IEnumerable<SavedView> objectsToClone)
            : this()
        {
            _objectsToClone = objectsToClone;
        }

        public CloneSavedViewsRequest()
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