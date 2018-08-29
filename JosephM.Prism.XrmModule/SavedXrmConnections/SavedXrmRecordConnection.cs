using JosephM.Core.Attributes;
using JosephM.ObjectMapping;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.XrmModule.SavedXrmConnections
{
    [Instruction("Enter Details For Connecting To The Dynamics Instance\n\nThe Discovery Service Address And Organisation Unique Name Are Found By Navigating To Settings -> Customizations -> Developer Resources In The Main Menu Of The Dynamics Web Application")]
    public class SavedXrmRecordConfiguration : XrmRecordConfiguration
    {
        [PropertyInContextByPropertyValue(nameof(HideActive), false)]
        [MyDescription("Set This Connection As Active")]
        [DisplayOrder(1)]
        [UniqueOn]
        [GridWidth(65)]
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
