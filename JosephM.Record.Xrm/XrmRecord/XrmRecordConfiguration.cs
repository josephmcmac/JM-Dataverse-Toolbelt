﻿using JosephM.Core.Attributes;
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
        public XrmRecordConfiguration()
        {
            AuthenticationProviderType = XrmRecordAuthenticationProviderType.LiveId;
        }

        [RequiredProperty]
        [GridField]
        [GridWidth(300)]
        [MyDescription("Name For Your Connection")]
        [DisplayOrder(2)]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name ?? OrganizationUniqueName;
        }

        [GridField]
        [GridReadOnly]
        [GridWidth(85)]
        [DisplayOrder(15)]
        [MyDescription("This Option Will Use The Connection Controls Provided By Assemblies Within The SDK. This Option Supports MFA And Stored Connections Will Be Managed By The SDK Assemblies")]
        public bool UseXrmToolingConnector { get; set; }

        [DisplayOrder(16)]
        [PropertyInContextByPropertyValue(nameof(UseXrmToolingConnector), true)]
        [ReadOnlyWhenSet]
        public string ToolingConnectionId { get; set; }

        [PropertyInContextByPropertyValue(nameof(UseXrmToolingConnector), false)]
        [MyDescription("The User Authentication Type For The CRM Instance")]
        [DisplayOrder(20)]
        [RequiredProperty]
        [DisplayName("Authentication Type")]
        public XrmRecordAuthenticationProviderType AuthenticationProviderType { get; set; }

        [PropertyInContextByPropertyValue(nameof(UseXrmToolingConnector), false)]
        [PropertyInContextByPropertyValues(nameof(AuthenticationProviderType),
            new object[]
            {
                XrmRecordAuthenticationProviderType.ActiveDirectory, XrmRecordAuthenticationProviderType.Federation
                , XrmRecordAuthenticationProviderType.OnlineFederation,
                XrmRecordAuthenticationProviderType.None
            })]
        [MyDescription("The Discovery Service Address For The Instance. Accessible At Settings -> Customizations -> Developer Resources")]
        [DisplayOrder(30)]
        [RequiredProperty]
        public string DiscoveryServiceAddress { get; set; }

        [GridField]
        [GridReadOnly]
        [PropertyInContextByPropertyValue(nameof(UseXrmToolingConnector), false)]
        [PropertyInContextByPropertyValue(nameof(AreDetailsForOrganisations), true)]
        [MyDescription("The Unique Name Of The Instance. Accessible At Settings -> Customizations -> Developer Resources")]
        [DisplayOrder(80)]
        [RequiredProperty]
        [GridWidth(300)]
        [DisplayName("Org Unique Name")]
        public string OrganizationUniqueName { get; set; }

        [MyDescription("If Active Directory Authentication The Domain For Your Login")]
        [DisplayOrder(50)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(UseXrmToolingConnector), false)]
        [PropertyInContextByPropertyValues(nameof(AuthenticationProviderType),
            new object[]
            {
                XrmRecordAuthenticationProviderType.ActiveDirectory
            })]
        public string Domain { get; set; }

        [GridField]
        [GridReadOnly]
        [GridWidth(400)]
        [MyDescription("The Username Used To Login To The Instance")]
        [DisplayOrder(60)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(UseXrmToolingConnector), false)]
        [PropertyInContextByPropertyValues(nameof(AuthenticationProviderType),
            new object[]
            {
                XrmRecordAuthenticationProviderType.ActiveDirectory, XrmRecordAuthenticationProviderType.Federation
                , XrmRecordAuthenticationProviderType.OnlineFederation,
                XrmRecordAuthenticationProviderType.LiveId
            })]
        public string Username { get; set; }

        [MyDescription("The Password For Your User")]
        [DisplayOrder(70)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(UseXrmToolingConnector), false)]
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
        public bool AreDetailsForOrganisations
        {
            get
            {
                return (!string.IsNullOrWhiteSpace(DiscoveryServiceAddress) || AuthenticationProviderType == XrmRecordAuthenticationProviderType.LiveId)
                    && (!this.IsInContext(nameof(Domain)) || !string.IsNullOrWhiteSpace(Domain))
                    && (!this.IsInContext(nameof(Username)) || !string.IsNullOrWhiteSpace(Username))
                    && (!this.IsInContext(nameof(Password)) || (Password != null && !string.IsNullOrWhiteSpace(Password.GetRawPassword())));
            }
        }
    }
}