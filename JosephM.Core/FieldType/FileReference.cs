namespace JosephM.Core.FieldType
{
    public class FileReference
    {
        public FileReference(string fileName)
        {
            FileName = fileName;
        }

        public FileReference()
        {
            
        }

        public string FileName { get; set; }

        public override string ToString()
        {
            return FileName;
        }
    }
}