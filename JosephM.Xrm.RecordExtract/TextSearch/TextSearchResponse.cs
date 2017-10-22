using JosephM.Core.Attributes;
using JosephM.Core.Service;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    public class TextSearchResponse : ServiceResponseBase<TextSearchResponseItem>
    {
        [Hidden]
        public string Folder { get; set; }
        [Hidden]
        public string FileName { get; set; }
        [Hidden]
        public string FileNameQualified
        {
            get { return string.Format("{0}/{1}", Folder, FileName); }
        }
    }
}