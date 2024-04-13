using JosephM.Xrm.DataImportExport.Import;
using System.Collections.Generic;

namespace JosephM.Xrm.DataImportExport.MappedImport
{
    public interface IMapSourceImport
    {
        bool IgnoreDuplicates { get; }
        string SourceType { get; }
        string TargetType { get; }
        string TargetTypeLabel { get; }
        IEnumerable<IMapSourceField> FieldMappings { get; }
        IEnumerable<IMapSourceMatchKey> AltMatchKeys { get; }
        IEnumerable<ExplicitFieldValues> ExplicitValuesToSet { get; }
    }
}
