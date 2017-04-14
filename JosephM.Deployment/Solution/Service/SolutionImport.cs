using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;

namespace JosephM.Xrm.ImportExporter.Service
{
    public class SolutionImport : ISolutionImport
    {
        public SolutionImport()
        {
            OverwriteCustomisations = true;
            PublishWorkflows = true;
        }

        [RequiredProperty]
        [GridWidth(400)]
        [FileMask(FileMasks.ZipFile)]
        public FileReference SolutionFile { get; set; }

        public bool OverwriteCustomisations { get; set; }

        public bool PublishWorkflows { get; set; }

        public int? ImportOrder { get; set; }


        public override string ToString()
        {
            return SolutionFile != null ? SolutionFile.FileName : base.ToString();
        }
    }
}
