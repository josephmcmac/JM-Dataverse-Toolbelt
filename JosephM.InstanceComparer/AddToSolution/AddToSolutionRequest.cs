using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Attributes;
using JosephM.Record.Query;
using JosephM.Xrm.Schema;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.InstanceComparer.AddToSolution
{
    [Group(Sections.Solution, true, 10)]
    [Group(Sections.Types, true, order: 20, selectAll: true)]
    public class AddToSolutionRequest : ServiceRequestBase
    {
        [Group(Sections.Solution)]
        [RequiredProperty]
        [ReferencedType(Entities.solution)]
        [UsePicklist(Fields.solution_.uniquename)]
        [LookupCondition(Fields.solution_.ismanaged, false)]
        [LookupCondition(Fields.solution_.isvisible, true)]
        [LookupCondition(Fields.solution_.uniquename, ConditionType.NotEqual, "default")]
        public Lookup SolutionAddTo { get; set; }

        [Hidden]
        public bool AreDashboards
        {
            get
            {
                return Items.Any(i => i.ComponentType == OptionSets.SolutionComponent.ObjectTypeCode.SystemForm);
            }
        }
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(AreDashboards), true)]
        [DisplayOrder(100)]
        public bool Dashboards { get; set; }

        [Hidden]
        public bool AreEmailTemplates
        {
            get
            {
                return Items.Any(i => i.ComponentType == OptionSets.SolutionComponent.ObjectTypeCode.EmailTemplate);
            }
        }
        [DisplayOrder(200)]
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(AreEmailTemplates), true)]
        public bool EmailTemplates { get; set; }

        [Hidden]
        public bool AreEntities
        {
            get
            {
                return Items.Any(i => i.ComponentType == OptionSets.SolutionComponent.ObjectTypeCode.Entity);
            }
        }
        [DisplayOrder(300)]
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(AreEntities), true)]
        public bool Entity { get; set; }

        [Hidden]
        public bool ArePluginTriggers
        {
            get
            {
                return Items.Any(i => i.ComponentType == OptionSets.SolutionComponent.ObjectTypeCode.SDKMessageProcessingStep);
            }
        }
        [DisplayOrder(400)]
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(ArePluginTriggers), true)]
        public bool PluginTriggers { get; set; }

        [Hidden]
        public bool ArePluginAssemblies
        {
            get
            {
                return Items.Any(i => i.ComponentType == OptionSets.SolutionComponent.ObjectTypeCode.PluginAssembly);
            }
        }
        [DisplayOrder(350)]
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(ArePluginAssemblies), true)]
        public bool PluginAssemblies { get; set; }

        [Hidden]
        public bool AreReports
        {
            get
            {
                return Items.Any(i => i.ComponentType == OptionSets.SolutionComponent.ObjectTypeCode.Report);
            }
        }
        [DisplayOrder(500)]
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(AreReports), true)]
        public bool Reports { get; set; }

        [Hidden]
        public bool AreSecurityRoles
        {
            get
            {
                return Items.Any(i => i.ComponentType == OptionSets.SolutionComponent.ObjectTypeCode.Role);
            }
        }
        [DisplayOrder(600)]
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(AreSecurityRoles), true)]
        public bool SecurityRoles { get; set; }

        [Hidden]
        public bool AreSharedPicklists
        {
            get
            {
                return Items.Any(i => i.ComponentType == OptionSets.SolutionComponent.ObjectTypeCode.OptionSet);
            }
        }
        [DisplayOrder(700)]
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(AreSharedPicklists), true)]
        public bool SharedPicklists { get; set; }

        [Hidden]
        public bool AreWebResources
        {
            get
            {
                return Items.Any(i => i.ComponentType == OptionSets.SolutionComponent.ObjectTypeCode.WebResource);
            }
        }
        [DisplayOrder(800)]
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(AreWebResources), true)]
        public bool WebResources { get; set; }

        [Hidden]
        public bool AreWorkflows
        {
            get
            {
                return Items.Any(i => i.ComponentType == OptionSets.SolutionComponent.ObjectTypeCode.Workflow);
            }
        }
        [DisplayOrder(900)]
        [Group(Sections.Types)]
        [PropertyInContextByPropertyValue(nameof(AreWorkflows), true)]
        public bool Workflows { get; set; }

        [Hidden]
        public IEnumerable<AddToSolutionItem> Items { get; set; }

        private static class Sections
        {
            public const string Solution = "Solution";
            public const string Types = "Types";
        }
    }
}