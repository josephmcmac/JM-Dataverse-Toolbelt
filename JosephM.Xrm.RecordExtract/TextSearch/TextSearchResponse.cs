using JosephM.Core.Service;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    public class TextSearchResponse : ServiceResponseBase<TextSearchResponseItem>
    {
        public string Folder { get; set; }
        public string FileName { get; set; }

        public string FileNameQualified
        {
            get { return string.Format("{0}/{1}", Folder, FileName); }
        }
    }
}