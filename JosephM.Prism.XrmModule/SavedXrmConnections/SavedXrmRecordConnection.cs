using JosephM.Core.Attributes;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Prism.XrmModule.SavedXrmConnections
{
    public class SavedXrmRecordConfiguration : XrmRecordConfiguration
    {
        [DisplayOrder(1)]
        [UniqueOn]
        [GridWidth(60)]
        public bool Active { get; set; }

        public override string ToString()
        {
            return OrganizationUniqueName;
        }
    }
}
