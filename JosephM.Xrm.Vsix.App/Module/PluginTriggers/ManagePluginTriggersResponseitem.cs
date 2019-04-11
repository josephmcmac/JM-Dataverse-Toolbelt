using System;
using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.Xrm.Vsix.Module.PluginTriggers
{
    public class ManagePluginTriggersResponseitem : ServiceResponseItem
    {
        public ManagePluginTriggersResponseitem(string change, string name, Exception exception = null)
        {
            Exception = exception;
            ChangeMade = change;
            Name = name;
        }

        [DisplayOrder(10)]
        [GridWidth(135)]
        public string ChangeMade { get; set; }

        [DisplayOrder(20)]
        [GridWidth(600)]
        public string Name { get; set; }
    }
}