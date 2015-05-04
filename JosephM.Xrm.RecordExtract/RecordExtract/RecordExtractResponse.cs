using JosephM.Core.Service;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    public class RecordExtractResponse : ServiceResponseBase<RecordExtractResponseItem>
    {
        public string Folder { get; set; }
        public string FileName { get; set; }

        public string FileNameQualified
        {
            get { return string.Format("{0}/{1}", Folder, FileName); }
        }
    }
}