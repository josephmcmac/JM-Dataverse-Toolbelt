using JosephM.Application.Modules;
using System.Diagnostics;
using System.Reflection;

namespace JosephM.Application.Desktop.Module.OpenLink
{
    public abstract class OpenLinkModule : SettingsModuleBase
    {
        public abstract string UrlToOpen { get; }

        public override void DialogCommand()
        {
            Process.Start(UrlToOpen);
        }
    }
}