using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Xrm.RecordExtract.DocumentWriter;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    public class RecordExtractToDocumentRequest
    {
        public RecordExtractToDocumentRequest(Lookup lookup, Section section, LogController controller, DetailLevel relatedDetail)
        {
            RecordLookup = lookup;
            Section = section;
            Controller = controller;
            RelatedDetail = relatedDetail;
        }

        public Lookup RecordLookup { get; set; }
        public Section Section { get; set; }
        public LogController Controller { get; set; }
        public DetailLevel RelatedDetail { get; set; }
    }
}