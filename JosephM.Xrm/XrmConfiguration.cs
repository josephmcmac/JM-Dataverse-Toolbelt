using Microsoft.Xrm.Sdk.Client;

// These namespaces are found in the Microsoft.Xrm.Sdk.dll assembly
// located in the SDK\bin folder of the SDK download.

namespace JosephM.Xrm
{
    public class XrmConfiguration : IXrmConfiguration
    {
        public string Name { get; set; }
        public bool UseXrmToolingConnector { get; set; }
        public string ToolingConnectionId { get; set; }
        public AuthenticationProviderType AuthenticationProviderType { get; set; }
        public string DiscoveryServiceAddress { get; set; }
        public string OrganizationUniqueName { get; set; }
        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}