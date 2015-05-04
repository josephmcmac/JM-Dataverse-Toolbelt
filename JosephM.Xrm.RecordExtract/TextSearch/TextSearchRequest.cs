using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.RecordExtract.DocumentWriter;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    [DisplayName("Text Search")]
    public class TextSearchRequest : ServiceRequestBase
    {
        [RequiredProperty]
        public Folder SaveToFolder { get; set; }

        [RequiredProperty]
        public DocumentType DocumentFormat { get; set; }

        [RequiredProperty]
        public string SearchText { get; set; }

        [RequiredProperty]
        public DetailLevel DetailOfRecordsRelatedToMatches { get; set; }
    }
}