namespace JosephM.Deployment.SpreadsheetImport
{
    public interface IMapSourceMatchKey
    {
        string TargetField { get; }
        string TargetFieldLabel { get; }
        bool CaseSensitive { get; }
    }
}
