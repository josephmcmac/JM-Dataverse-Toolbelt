using System.Collections.Generic;
using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Record.IService;

namespace JosephM.Xrm.Vsix.Module.DeployAssembly
{
    [Group(Sections.PluginAssembly, Group.DisplayLayoutEnum.HorizontalCenteredInputOnly, order: 10)]
    [Group(Sections.DeployOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20, displayLabel: false)]
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
        [DisplayName("Refresh Workflow Arguments")]
        [MyDescription("If set the deploy process sets a field on each custom workflow activity intended to refresh it's input & output arguments")]
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
}
