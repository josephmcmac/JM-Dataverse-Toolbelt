using Microsoft.Xrm.Sdk.Client;

namespace $safeprojectname$.Xrm
{
    public class XrmConfiguration : IXrmConfiguration
    {
        public virtual AuthenticationProviderType AuthenticationProviderType { get; set; }
        public virtual string DiscoveryServiceAddress { get; set; }
        public virtual string OrganizationUniqueName { get; set; }
        public virtual string Domain { get; set; }
        public virtual string Username { get; set; }
        public virtual string Password { get; set; }
    }
}