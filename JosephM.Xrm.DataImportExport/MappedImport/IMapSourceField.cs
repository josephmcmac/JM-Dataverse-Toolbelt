namespace JosephM.Xrm.DataImportExport.MappedImport
{
    public interface IMapSourceField
    {
        string SourceField { get; }
        string TargetField { get; }
        bool UseAltMatchField { get; }
        string AltMatchFieldType { get; }
        string AltMatchField { get; }
    }
}
