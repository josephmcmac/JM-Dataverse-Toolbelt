using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;

namespace JosephM.Xrm.ImportExporter.Service
{
    public interface ISolutionImport
    {
        FileReference SolutionFile { get; }
        bool OverwriteCustomisations { get; }
        bool PublishWorkflows { get; }
        int? ImportOrder { get; }
    }
}