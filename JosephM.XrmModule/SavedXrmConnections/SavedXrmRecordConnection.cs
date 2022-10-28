using JosephM.Core.Attributes;
using JosephM.ObjectMapping;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.XrmModule.SavedXrmConnections
{
    [Instruction("Direct form entry for your login will fail if authentication requires multi factor authentication (MFA). If MFA is required click 'Use XRM Tooling Connector' and login using the Microsoft SDK connection dialog\n\nIf not using the XRM tooling option, connection is done with an SDK connection string, and your password will be encrypted locally by .NET assemblies protected at local user scope")]
    public class SavedXrmRecordConfiguration : XrmRecordConfiguration
    {
        [Hidden]
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
