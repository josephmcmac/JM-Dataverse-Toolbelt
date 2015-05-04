using System;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;

namespace JosephM.Record.Xrm.XrmRecord
{
    public class XrmRecordConfiguration : IXrmRecordConfiguration, IValidatableObject
    {
        [DisplayOrder(20)]
        [RequiredProperty]
        public XrmRecordAuthenticationProviderType AuthenticationProviderType { get; set; }

        [DisplayOrder(30)]
        [RequiredProperty]
        [GridWidth(400)]
        public string DiscoveryServiceAddress { get; set; }

        [DisplayOrder(40)]
        [RequiredProperty]
        public string OrganizationUniqueName { get; set; }

        [DisplayOrder(50)]
        [RequiredProperty]
        [GridWidth(100)]
        [PropertyInContextByPropertyValues("AuthenticationProviderType",
            new object[]
            {
                XrmRecordAuthenticationProviderType.ActiveDirectory, XrmRecordAuthenticationProviderType.Federation
                , XrmRecordAuthenticationProviderType.OnlineFederation
            })]
        public string Domain { get; set; }

        [DisplayOrder(60)]
        [RequiredProperty]
        [GridWidth(100)]
        [PropertyInContextByPropertyValues("AuthenticationProviderType",
            new object[]
            {
                XrmRecordAuthenticationProviderType.ActiveDirectory, XrmRecordAuthenticationProviderType.Federation
                , XrmRecordAuthenticationProviderType.OnlineFederation,
                XrmRecordAuthenticationProviderType.LiveId
            })]
        public string Username { get; set; }

        [DisplayOrder(70)]
        [RequiredProperty]
        [GridWidth(100)]
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