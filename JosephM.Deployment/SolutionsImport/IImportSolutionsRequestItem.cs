namespace JosephM.Deployment.SolutionsImport
{
    public interface IImportSolutionsRequestItem
    {
        byte[] GetSolutionZipContent();
        bool InstallAsUpgrade { get; }
        bool OverwriteCustomisations { get; }
    }
}
