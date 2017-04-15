using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;

namespace JosephM.Xrm.ImportExporter.Service
{
    [Group(Sections.ImportOptions, true)]
    [Group(Sections.Solution, true)]
    public class SolutionImport : ISolutionImport
    {
        public SolutionImport()
        {
            OverwriteCustomisations = true;
            PublishWorkflows = true;
        }

        [Group(Sections.Solution)]
        [RequiredProperty]
        [GridWidth(400)]
        [FileMask(FileMasks.ZipFile)]
        public FileReference SolutionFile { get; set; }

        [Group(Sections.ImportOptions)]
        public bool OverwriteCustomisations { get; set; }

        [Group(Sections.ImportOptions)]
        public bool PublishWorkflows { get; set; }

        [Group(Sections.ImportOptions)]
        public int? ImportOrder { get; set; }


        public override string ToString()
        {
            return SolutionFile != null ? SolutionFile.FileName : base.ToString();
        }

        private static class Sections
        {
            public const string Solution = "Solution";
            public const string ImportOptions = "Import Options";
        }
    }
}
