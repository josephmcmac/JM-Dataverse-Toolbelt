using JosephM.Core.Attributes;
using JosephM.ObjectMapping;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.XrmModule.SavedXrmConnections
{
    [Group("Hidden", isHiddenSection: true)]
    public class SavedXrmRecordConfiguration : XrmRecordConfiguration
    {
        [Group("Hidden")]
        [MyDescription("Set this connection active")]
        [DisplayOrder(1)]
        [UniqueOn]
        [GridWidth(65)]
        [GridField]
        public bool Active { get; set; }

        public static SavedXrmRecordConfiguration CreateNew(IXrmRecordConfiguration xrmRecordConfiguration)
        {
            var mapper = new ClassSelfMapper();
            var savedConnection = new SavedXrmRecordConfiguration();
            mapper.Map(xrmRecordConfiguration, savedConnection);
            return savedConnection;
        }
    }
}
