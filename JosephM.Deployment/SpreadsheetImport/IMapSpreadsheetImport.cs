using System.Collections.Generic;

namespace JosephM.Deployment.SpreadsheetImport
{
    public interface IMapSpreadsheetImport
    {
        string SourceType { get; }
        string TargetType { get; }
        string TargetTypeLabel { get; }
        IEnumerable<IMapSpreadsheetColumn> FieldMappings { get; }
        IEnumerable<IMapSpreadsheetMatchKey> AltMatchKeys { get; }
    }
}
