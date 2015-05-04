using JosephM.Core.Constants;

namespace JosephM.Core.FieldType
{
    public class ExcelFile : XrmFile
    {
        public ExcelFile(string filePath)
            : base(filePath)
        {
        }

        public override string FileMask
        {
            get { return FileMasks.ExcelFile; }
        }
    }
}