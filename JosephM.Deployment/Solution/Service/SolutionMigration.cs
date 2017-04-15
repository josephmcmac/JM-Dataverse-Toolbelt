using JosephM.Core.Attributes;

namespace JosephM.Xrm.ImportExporter.Service
{
    [Group(Sections.ImportOptions, true, 20)]
    public class SolutionMigration : SolutionExport
    {
        public SolutionMigration()
        {
            OverwriteCustomisations = true;
            PublishWorkflows = true;
        }

        [Group(Sections.ImportOptions)]
        [DisplayOrder(120)]
        public bool OverwriteCustomisations { get; set; }

        [Group(Sections.ImportOptions)]
        [DisplayOrder(130)]
        public bool PublishWorkflows { get; set; }

        [Group(Sections.ImportOptions)]
        [DisplayOrder(140)]
        public int? ImportOrder { get; set; }

        private static class Sections
        {
            public const string ImportOptions = "Import Options";
        }
    }
}
