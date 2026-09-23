using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.Schema;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.UserSavedObjectsUtility.Charts.Clone
{
    [Group(Sections.CloneDetails, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10)]
    public class CloneSavedChartsRequest : ServiceRequestBase
    {
        public CloneSavedChartsRequest(IEnumerable<SavedChart> ChartsToClone)
            : this()
        {
            _objectsToClone = ChartsToClone;
        }

        public CloneSavedChartsRequest()
        {
        }

        private IEnumerable<SavedChart> _objectsToClone { get; set; }

        public IEnumerable<SavedChart> GetObjectsToClone()
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