using JosephM.Core.Attributes;
using JosephM.ObjectMapping;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.XrmModule.SavedXrmConnections
{
    [Instruction("Enter Details For Connecting To The Dynamics Instance\n\nIf The Connection Requires Multi Factor Authentication Then The Xrm Tooling Connector Option Must Be Used\n\nOtherwise If Not Using The Xrm Tooling Connector Option, Connection Details Are Stored By This App And Passed Through Using The SDKs AuthenticationCredentials Class. Using This Method Authentication Will Fail If Multi Factor Authentication is Required For The User")]
    public class SavedXrmRecordConfiguration : XrmRecordConfiguration
    {
        [PropertyInContextByPropertyValue(nameof(HideActive), false)]
        [MyDescription("Set This Connection As Active")]
        [DisplayOrder(1)]
        [UniqueOn]
        [GridWidth(65)]
        [GridField]
        public bool Active { get; set; }

        [Hidden]
        public bool HideActive { get; set; }

        public static SavedXrmRecordConfiguration CreateNew(IXrmRecordConfiguration xrmRecordConfiguration)
        {
            var mapper = new ClassSelfMapper();
            var savedConnection = new SavedXrmRecordConfiguration();
            mapper.Map(xrmRecordConfiguration, savedConnection);
            return savedConnection;
        }
    }
}
