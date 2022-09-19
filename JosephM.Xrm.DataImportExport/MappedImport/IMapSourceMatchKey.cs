namespace JosephM.Xrm.DataImportExport.MappedImport
{
    public interface IMapSourceMatchKey
    {
        string TargetField { get; }
        string TargetFieldLabel { get; }
        bool CaseSensitive { get; }
    }
}
