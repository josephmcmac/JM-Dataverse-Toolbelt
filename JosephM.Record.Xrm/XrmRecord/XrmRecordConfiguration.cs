using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;

namespace JosephM.Record.Xrm.XrmRecord
{
    [Instruction("Enter Details For Connecting To The Dynamics Instance\n\nIf The Connection Requires Multi Factor Authentication Then The Xrm Tooling Connector Option Must Be Used\n\nOtherwise If Not Using The Xrm Tooling Connector Option, Connection Details Are Stored By This App And Passed Through Using The SDKs AuthenticationCredentials Class. Using This Method Authentication Will Fail If Multi Factor Authentication is Required For The User")]
    [ServiceConnection(typeof(XrmRecordService))]
    public class XrmRecordConfiguration : IXrmRecordConfiguration, IValidatableObject
    {
        [RequiredProperty]
        [GridField]
        [GridWidth(150)]
        [MyDescription("Name For Your Connection")]
        [DisplayOrder(2)]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name ?? OrganizationUniqueName;
        }

        [GridField]
        [GridWidth(85)]
        [DisplayOrder(15)]
        [MyDescription("This Option Will Use The Connection Controls Provided By Assemblies Within The SDK. This Option Supports MFA And Stored Connections Will Be Managed By The SDK Assemblies")]
        public bool UseXrmToolingConnector { get; set; }

        [DisplayOrder(16)]
        [PropertyInContextByPropertyValue(nameof(UseXrmToolingConnector), true)]
        [ReadOnlyWhenSet]
        public string ToolingConnectionId { get; set; }

        [GridField]
        [PropertyInContextByPropertyValue(nameof(UseXrmToolingConnector), false)]
        [GridWidth(160)]
        [MyDescription("The User Authentication Type For The CRM Instance")]
        [DisplayOrder(20)]
        [RequiredProperty]
        [DisplayName("Authentication Type")]
        public XrmRecordAuthenticationProviderType AuthenticationProviderType { get; set; }

        [GridField]
        [PropertyInContextByPropertyValue(nameof(UseXrmToolingConnector), false)]
        [MyDescription("The Discovery Service Address For The Instance. Accessible At Settings -> Customizations -> Developer Resources")]
        [DisplayOrder(30)]
        [RequiredProperty]
        [GridWidth(400)]
        public string DiscoveryServiceAddress { get; set; }

        [GridField]
        [PropertyInContextByPropertyValue(nameof(AreDiscoveryDetailsEntered), true)]
        [MyDescription("The Unique Name Of The Instance. Accessible At Settings -> Customizations -> Developer Resources")]
        [DisplayOrder(80)]
        [RequiredProperty]
        [GridWidth(160)]
        [DisplayName("Org Unique Name")]
        public string OrganizationUniqueName { get; set; }

        [GridField]
        [MyDescription("If Active Directory Authentication The Domain For Your Login")]
        [DisplayOrder(50)]
        [RequiredProperty]
        [GridWidth(100)]
        [PropertyInContextByPropertyValues(nameof(AuthenticationProviderType),
            new object[]
            {
                XrmRecordAuthenticationProviderType.ActiveDirectory
            })]
        public string Domain { get; set; }

        [GridField]
        [GridWidth(300)]
        [MyDescription("The Username Used To Login To The Instance")]
        [DisplayOrder(60)]
        [RequiredProperty]
        [PropertyInContextByPropertyValues(nameof(AuthenticationProviderType),
            new object[]
            {
                XrmRecordAuthenticationProviderType.ActiveDirectory, XrmRecordAuthenticationProviderType.Federation
                , XrmRecordAuthenticationProviderType.OnlineFederation,
                XrmRecordAuthenticationProviderType.LiveId
            })]
        public string Username { get; set; }

        [GridField]
        [GridWidth(150)]
        [MyDescription("The Password For Your User")]
        [DisplayOrder(70)]
        [RequiredProperty]
        [PropertyInContextByPropertyValues(nameof(AuthenticationProviderType),
            new object[]
            {
                XrmRecordAuthenticationProviderType.ActiveDirectory, XrmRecordAuthenticationProviderType.Federation
                , XrmRecordAuthenticationProviderType.OnlineFederation,
                XrmRecordAuthenticationProviderType.LiveId
            })]
        public Password Password { get; set; }

        public IsValidResponse Validate()
        {
            if (UseXrmToolingConnector)
                return new IsValidResponse();
            return new XrmRecordService(this, new LogController(), null).VerifyConnection();
        }

        [Hidden]
        public bool AreDiscoveryDetailsEntered
        {
            get
            {
                return !string.IsNullOrWhiteSpace(DiscoveryServiceAddress)
                    && (!this.IsInContext(nameof(Domain)) || !string.IsNullOrWhiteSpace(Domain))
                    && (!this.IsInContext(nameof(Username)) || !string.IsNullOrWhiteSpace(Username))
                    && (!this.IsInContext(nameof(Password)) || (Password != null && !string.IsNullOrWhiteSpace(Password.GetRawPassword())));
            }
        }
    }
}