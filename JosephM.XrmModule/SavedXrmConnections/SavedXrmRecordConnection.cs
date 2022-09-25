using JosephM.Core.Attributes;
using JosephM.ObjectMapping;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.XrmModule.SavedXrmConnections
{
    [Instruction("Direct form entry for you login will fail if authentication requires MFA. If your connection requires multi factor authentication click 'Use XRM Tooling Connector' and login using the Microsoft SDK connection dialog\n\nIf not using the XRM tooling option, connection is done with an SDK connection string. Your password is encrypted by .NET assemblies protected at local user scope")]
    public class SavedXrmRecordConfiguration : XrmRecordConfiguration
    {
        [PropertyInContextByPropertyValue(nameof(HideActive), false)]
        [MyDescription("Set this connection active")]
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
