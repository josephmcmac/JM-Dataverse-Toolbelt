namespace JosephM.Deployment.SpreadsheetImport
{
    public interface IMapSpreadsheetColumn
    {
        string SourceField { get; }
        string TargetField { get; }
        bool UseAltMatchField { get; }
        string AltMatchFieldType { get; }
        string AltMatchField { get; }
    }
}
