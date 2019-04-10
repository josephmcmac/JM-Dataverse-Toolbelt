using System.Collections.Generic;
using JosephM.Core.Attributes;

namespace JosephM.Xrm.Vsix.Module.DeployAssembly
{
    [Group(Sections.PluginAssembly, true, 10)]
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

        [RequiredProperty]
        [DisplayOrder(10)]
        [Group(Sections.PluginAssembly)]
        [ReadOnlyWhenSet]
        [DisplayName("Assembly Name")]
        public string Name { get; set; }

        [RequiredProperty]
        [DisplayOrder(20)]
        [Group(Sections.PluginAssembly)]
        public IsolationMode_ IsolationMode { get; set; }

        [DoNotAllowAdd]
        public IEnumerable<PluginType> PluginTypes { get; set; }

        public enum IsolationMode_
        {
            Sandbox = 2,
            None = 1
        }

        private static class Sections
        {
            public const string PluginAssembly = "Plugin Assembly";
        }
    }

    [DoNotAllowGridOpen]
    public class PluginType
    {
        [DisplayOrder(10)]
        [Hidden]
        public string Id { get; set; }

        [DisplayOrder(20)]
        [GridWidth(350)]
        [ReadOnlyWhenSet]
        [RequiredProperty]
        public string TypeName { get; set; }

        [GridWidth(300)]
        [DisplayOrder(30)]
        [RequiredProperty]
        public string FriendlyName { get; set; }

        [DisplayOrder(40)]
        [GridWidth(300)]
        [RequiredProperty]
        public string Name { get; set; }

        [DisplayOrder(50)]
        [Hidden]
        public bool IsWorkflowActivity { get; set; }

        [DisplayOrder(60)]
        [GridWidth(300)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(IsWorkflowActivity), true)]
        public string GroupName { get; set; }

        [GridWidth(100)]
        [DisplayOrder(70)]
        [ReadOnlyWhenSet]
        public bool InAssembly { get; set; }
    }
}
