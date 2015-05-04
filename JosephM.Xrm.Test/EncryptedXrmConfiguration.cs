using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using Microsoft.Xrm.Sdk.Client;

namespace JosephM.Xrm.Test
{
    public class EncryptedXrmConfiguration
    {
        [DisplayOrder(20)]
        [RequiredProperty]
        public AuthenticationProviderType AuthenticationProviderType { get; set; }

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
                AuthenticationProviderType.ActiveDirectory, AuthenticationProviderType.Federation
                , AuthenticationProviderType.OnlineFederation
            })]
        public string Domain { get; set; }

        [DisplayOrder(60)]
        [RequiredProperty]
        [GridWidth(100)]
        [PropertyInContextByPropertyValues("AuthenticationProviderType",
            new object[]
            {
                AuthenticationProviderType.ActiveDirectory, AuthenticationProviderType.Federation
                , AuthenticationProviderType.OnlineFederation,
                AuthenticationProviderType.LiveId
            })]
        public string Username { get; set; }

        [DisplayOrder(70)]
        [RequiredProperty]
        [GridWidth(100)]
        [PropertyInContextByPropertyValues("AuthenticationProviderType",
            new object[]
            {
                AuthenticationProviderType.ActiveDirectory, AuthenticationProviderType.Federation
                , AuthenticationProviderType.OnlineFederation,
                AuthenticationProviderType.LiveId
            })]
        public Password Password { get; set; }
    }
}