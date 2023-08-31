namespace $safeprojectname$.Xrm
{
    public interface IXrmConfiguration
    {
    string Name { get; }
    string ClientId { get; }
    string ClientSecret { get; }
    string WebUrl { get; }
}
}