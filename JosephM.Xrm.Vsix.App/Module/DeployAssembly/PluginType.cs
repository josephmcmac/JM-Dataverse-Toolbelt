using JosephM.Core.Attributes;

namespace JosephM.Xrm.Vsix.Module.DeployAssembly
{
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
