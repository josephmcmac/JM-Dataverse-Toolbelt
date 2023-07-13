using JosephM.Core.Attributes;
using JosephM.Core.FieldType;

namespace JosephM.Record.Xrm.XrmRecord
{
    public interface IXrmRecordConfiguration
    {
        string Name { get; }
        string ToolingConnectionId { get; }
        XrmRecordConfigurationConnectionType? ConnectionType { get; }
        string ClientId { get; }
        Password ClientSecret { get; }
        string WebUrl { get; }
    }
}