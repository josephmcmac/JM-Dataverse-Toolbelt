using Microsoft.Xrm.Sdk.Client;
using $safeprojectname$.Core;

namespace $safeprojectname$.Xrm
{
    public class XrmSetting : IXrmConfiguration
    {
        public string Name { get; set; }
        public bool UseXrmToolingConnector { get; set; }
        public string ToolingConnectionId { get; set; }
        public AuthenticationProviderType AuthenticationProviderType { get; set; }
        public string DiscoveryServiceAddress { get; set; }
        public string OrganizationUniqueName { get; set; }
        [PropertyInContextByPropertyValues("AuthenticationProviderType",
        new object[]
        {
             AuthenticationProviderType.ActiveDirectory
        })]
        public string Domain { get; set; }
        public string Username { get; set; }
        public Password Password { get; set; }
        string IXrmConfiguration.Password => Password == null ? null : Password.GetRawPassword();
    }
}