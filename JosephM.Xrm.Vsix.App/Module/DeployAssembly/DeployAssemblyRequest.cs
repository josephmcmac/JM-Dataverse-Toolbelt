using System.Collections.Generic;
using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Record.IService;

namespace JosephM.Xrm.Vsix.Module.DeployAssembly
{
    [Group(Sections.PluginAssembly, true, 10)]
    [Group(Sections.DeployOptions, true, order: 20, displayLabel: false)]
    public class DeployAssemblyRequest : ServiceRequestBase
    {
        public DeployAssemblyRequest()
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
        public string AssemblyName { get; set; }

        [RequiredProperty]
        [DisplayOrder(20)]
        [Group(Sections.DeployOptions)]
        public IsolationMode_ IsolationMode { get; set; }

        [RequiredProperty]
        [DisplayOrder(30)]
        [Group(Sections.DeployOptions)]
        [DisplayName("Refresh Workflow Activity Input & Output Arguments")]
        [MyDescription("If Set The Deploy Process Sets A Field On Each Custom Workflow Activity Intended To Refresh It's Input & Output Arguments")]
        public bool TriggerWorkflowActivityRefreshes { get; set; }

        [DoNotAllowAdd]
        public IEnumerable<PluginType> PluginTypes { get; set; }

        private IEnumerable<IRecord> _preTypeRecords;

        public IEnumerable<IRecord> GetPreTypeRecords()
        {
            return _preTypeRecords;
        }

        public void SetPreTypeRecords(IEnumerable<IRecord> value)
        {
            _preTypeRecords = value;
        }

        public enum IsolationMode_
        {
            Sandbox = 2,
            None = 1
        }

        private static class Sections
        {
            public const string PluginAssembly = "Deploy Plugin Assembly";
            public const string DeployOptions = "Deploy Options";
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
        [DisplayOrder(1)]
        [ReadOnlyWhenSet]
        public bool InAssembly { get; set; }

        [GridWidth(100)]
        [DisplayOrder(2)]
        [ReadOnlyWhenSet]
        public bool IsDeployed { get; set; }
    }
}
