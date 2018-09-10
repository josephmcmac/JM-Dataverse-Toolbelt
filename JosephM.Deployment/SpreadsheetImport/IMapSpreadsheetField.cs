namespace JosephM.Deployment.SpreadsheetImport
{
    public interface IMapSpreadsheetColumn
    {
        string SourceField { get; }
        string TargetField { get; }
    }
}
