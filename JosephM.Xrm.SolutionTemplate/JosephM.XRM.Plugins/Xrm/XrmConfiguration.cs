﻿namespace $safeprojectname$.Xrm
{
    public class XrmConfiguration : IXrmConfiguration
    {
    public string Name { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string WebUrl { get; set; }
}
}