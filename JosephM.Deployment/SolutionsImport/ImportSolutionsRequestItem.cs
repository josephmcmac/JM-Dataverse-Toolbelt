namespace JosephM.Deployment.SolutionsImport
{
    public class ImportSolutionsRequestItem : IImportSolutionsRequestItem
    {
        public ImportSolutionsRequestItem(byte[] solutionZipContent)
        {
            OverwriteCustomisations = true;
            this.solutionZipContent = solutionZipContent;
        }

        private byte[] solutionZipContent;

        public byte[] GetSolutionZipContent()
        {
            return solutionZipContent;
        }

        public void SetSolutionZipContent(byte[] value)
        {
            solutionZipContent = value;
        }

        public bool InstallAsUpgrade { get; set; }
        public bool OverwriteCustomisations { get; set; }
        public bool ConvertToManaged { get; }
    }
}
