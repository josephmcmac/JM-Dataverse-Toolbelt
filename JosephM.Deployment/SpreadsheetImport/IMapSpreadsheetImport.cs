using System.Collections.Generic;

namespace JosephM.Deployment.SpreadsheetImport
{
    public interface IMapSpreadsheetImport
    {
        string SourceType { get; }
        string TargetType { get; }
        IEnumerable<IMapSpreadsheetColumn> FieldMappings { get; }
        IEnumerable<IMapSpreadsheetMatchKey> AltMatchKeys { get; }
    }
}
