namespace JosephM.Xrm
{
    public interface IXrmConfiguration
    {
        string Name { get; }
        string ToolingConnectionId { get; }
        XrmConnectionType? ConnectionType { get; }
        string ClientId { get; }
        string ClientSecret { get; }
        string WebUrl { get; }
    }
}