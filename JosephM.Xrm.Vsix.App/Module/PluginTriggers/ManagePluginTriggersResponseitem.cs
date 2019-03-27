using JosephM.Core.Service;

namespace JosephM.Xrm.Vsix.Module.PluginTriggers
{
    public class ManagePluginTriggersResponseitem : ServiceResponseItem
    {
        public string Type { get; set; }
        public string Name { get; set; }
    }
}