using System;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;

namespace JosephM.Record.Xrm.XrmRecord
{
    [ServiceConnection(typeof(XrmRecordService))]
    public class XrmRecordConfiguration : IXrmRecordConfiguration, IValidatableObject
    {
        [GridWidth(150)]
        [MyDescription("Name For Your Connection")]
        [DisplayOrder(2)]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name ?? OrganizationUniqueName;
        }

        [GridWidth(175)]
        [MyDescription("The User Authentication Type For The CRM Instance")]
        [DisplayOrder(20)]
        [RequiredProperty]
        public XrmRecordAuthenticationProviderType AuthenticationProviderType { get; set; }

        [MyDescription("The Discovery Service Address For The Instance. Accessible At Settings -> Customizations -> Developer Resources")]
        [DisplayOrder(30)]
        [RequiredProperty]
        [GridWidth(400)]
        public string DiscoveryServiceAddress { get; set; }

        [MyDescription("The Unique Name Of The Instance. Accessible At Settings -> Customizations -> Developer Resources")]
        [DisplayOrder(40)]
        [RequiredProperty]
        [GridWidth(160)]
        public string OrganizationUniqueName { get; set; }

        [MyDescription("If Active Directory Authentication The Domain For Your Login")]
        [DisplayOrder(50)]
        [RequiredProperty]
        [GridWidth(100)]
        [PropertyInContextByPropertyValues("AuthenticationProviderType",
            new object[]
            {
                XrmRecordAuthenticationProviderType.ActiveDirectory
            })]
        public string Domain { get; set; }

        [GridWidth(300)]
        [MyDescription("The Username Used To Login To The Instance")]
        [DisplayOrder(60)]
        [RequiredProperty]
        [PropertyInContextByPropertyValues("AuthenticationProviderType",
            new object[]
            {
                XrmRecordAuthenticationProviderType.ActiveDirectory, XrmRecordAuthenticationProviderType.Federation
                , XrmRecordAuthenticationProviderType.OnlineFederation,
                XrmRecordAuthenticationProviderType.LiveId
            })]
        public string Username { get; set; }

        [GridWidth(150)]
        [MyDescription("The Password For Your User")]
        [DisplayOrder(70)]
        [RequiredProperty]
        [PropertyInContextByPropertyValues("AuthenticationProviderType",
            new object[]
            {
                XrmRecordAuthenticationProviderType.ActiveDirectory, XrmRecordAuthenticationProviderType.Federation
                , XrmRecordAuthenticationProviderType.OnlineFederation,
                XrmRecordAuthenticationProviderType.LiveId
            })]
        public Password Password { get; set; }

        public IsValidResponse Validate()
        {
            return new XrmRecordService(this, new LogController()).VerifyConnection();
        }
    }
}