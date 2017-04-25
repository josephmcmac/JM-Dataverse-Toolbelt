using System.Collections.Generic;
using JosephM.Core.Attributes;

namespace JosephM.XRM.VSIX.Commands.DeployAssembly
{
    public class PluginAssembly
    {
        public PluginAssembly()
        {
            IsolationMode = IsolationMode_.Sandbox;
        }

        [Hidden]
        public string Id { get; set; }
        [Hidden]
        public string Content { get; set; }
        [ReadOnlyWhenSet]
        public string Name { get; set; }

        public IsolationMode_ IsolationMode { get; set; }
        [DoNotAllowAdd]
        public IEnumerable<PluginType> PluginTypes { get; set; }

        public enum IsolationMode_
        {
            Sandbox = 2,
            None = 1
        }


    }
    public class PluginType
    {
        [Hidden]
        public string Id { get; set; }

        [GridWidth(300)]
        [ReadOnlyWhenSet]
        [RequiredProperty]
        public string TypeName { get; set; }
        [RequiredProperty]
        public string FriendlyName { get; set; }
        [RequiredProperty]
        public string Name { get; set; }
        [Hidden]
        public bool IsWorkflowActivity { get; set; }
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(IsWorkflowActivity), true)]
        public string GroupName { get; set; }

        [ReadOnlyWhenSet]
        public bool InAssembly { get; set; }
    }
}
