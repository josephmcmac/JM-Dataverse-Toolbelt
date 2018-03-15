using Microsoft.Xrm.Sdk.Client;
using $safeprojectname$.Core;

namespace $safeprojectname$.Xrm
{
    public class XrmSetting : IXrmConfiguration
    {
        public AuthenticationProviderType AuthenticationProviderType { get; set; }
        public string DiscoveryServiceAddress { get; set; }
        public string OrganizationUniqueName { get; set; }
        public string Domain { get; set; }
        public string Username { get; set; }
        public Password Password { get; set; }
        string IXrmConfiguration.Password => Password == null ? null : Password.GetRawPassword();
    }
}