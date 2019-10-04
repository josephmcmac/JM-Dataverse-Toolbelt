using JosephM.Core.FieldType;

namespace JosephM.Record.Xrm.XrmRecord
{
    public interface IXrmRecordConfiguration
    {
        string Name { get; }
        bool UseXrmToolingConnector { get; }
        string ToolingConnectionId { get; }
        XrmRecordAuthenticationProviderType AuthenticationProviderType { get; }
        string DiscoveryServiceAddress { get; }
        string OrganizationUniqueName { get; }
        string Domain { get; }
        string Username { get; }
        Password Password { get; }
    }
}