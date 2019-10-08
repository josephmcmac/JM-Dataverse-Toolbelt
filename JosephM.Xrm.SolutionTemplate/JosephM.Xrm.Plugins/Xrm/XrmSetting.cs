using Microsoft.Xrm.Sdk.Client;
using $safeprojectname$.Core;

namespace $safeprojectname$.Xrm
{
    public class XrmSetting : IXrmConfiguration
    {
        public string Name { get; set; }

        public bool UseXrmToolingConnector { get; set; }

        [PropertyInContextByPropertyValues(nameof(UseXrmToolingConnector), new object[] { true })]
        public string ToolingConnectionId { get; set; }

        [PropertyInContextByPropertyValues(nameof(UseXrmToolingConnector), new object[] { false })]
        public AuthenticationProviderType AuthenticationProviderType { get; set; }

        [PropertyInContextByPropertyValues(nameof(UseXrmToolingConnector), new object[] { false })]
        public string DiscoveryServiceAddress { get; set; }

        [PropertyInContextByPropertyValues(nameof(UseXrmToolingConnector), new object[] { false })]
        public string OrganizationUniqueName { get; set; }

        [PropertyInContextByPropertyValues(nameof(AuthenticationProviderType),
            new object[] { AuthenticationProviderType.ActiveDirectory })]
        public string Domain { get; set; }

        [PropertyInContextByPropertyValues(nameof(AuthenticationProviderType),
            new object[]
            {
                        AuthenticationProviderType.ActiveDirectory, AuthenticationProviderType.Federation
                        , AuthenticationProviderType.OnlineFederation,
                        AuthenticationProviderType.LiveId
            })]
        public string Username { get; set; }
        [PropertyInContextByPropertyValues(nameof(AuthenticationProviderType),
            new object[]
            {
                        AuthenticationProviderType.ActiveDirectory, AuthenticationProviderType.Federation
                        , AuthenticationProviderType.OnlineFederation,
                        AuthenticationProviderType.LiveId
            })]
        public Password Password { get; set; }
        string IXrmConfiguration.Password => Password == null ? null : Password.GetRawPassword();
    }
}