namespace JosephM.Core.FieldType
{
    public abstract class XrmFile
    {
        protected XrmFile(string fileName)
        {
            FileName = fileName;
        }

        protected XrmFile()
        {
            
        }

        public string FileName { get; set; }

        public abstract string FileMask { get; }
    }
}