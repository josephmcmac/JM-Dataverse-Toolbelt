using $ext_safeprojectname$.Plugins.Xrm;
using Microsoft.Xrm.Sdk.Client;

namespace $safeprojectname$
{
    public class TestXrmConfiguration : XrmConfiguration
    {
        public override AuthenticationProviderType AuthenticationProviderType { get; set; }
        public override string DiscoveryServiceAddress { get; set; }
        public override string OrganizationUniqueName { get; set; }
        public override string Domain { get; set; }
        public override string Username { get; set; }
        public override string Password { get; set; }
    }
}
