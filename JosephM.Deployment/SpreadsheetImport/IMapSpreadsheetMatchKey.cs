namespace JosephM.Deployment.SpreadsheetImport
{
    public interface IMapSpreadsheetMatchKey
    {
        string TargetField { get; }
        string TargetFieldLabel { get; }
        bool CaseSensitive { get; }
    }
}
