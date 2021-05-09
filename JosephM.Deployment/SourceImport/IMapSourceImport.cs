using System.Collections.Generic;

namespace JosephM.Deployment.SpreadsheetImport
{
    public interface IMapSourceImport
    {
        bool IgnoreDuplicates { get; }
        string SourceType { get; }
        string TargetType { get; }
        string TargetTypeLabel { get; }
        IEnumerable<IMapSourceField> FieldMappings { get; }
        IEnumerable<IMapSourceMatchKey> AltMatchKeys { get; }
    }
}
