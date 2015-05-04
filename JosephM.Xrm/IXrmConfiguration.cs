using Microsoft.Xrm.Sdk.Client;

namespace JosephM.Xrm
{
    public interface IXrmConfiguration
    {
        AuthenticationProviderType AuthenticationProviderType { get; }
        string DiscoveryServiceAddress { get; }
        string OrganizationUniqueName { get; }
        string Domain { get; }
        string Username { get; }
        string Password { get; }
    }
}