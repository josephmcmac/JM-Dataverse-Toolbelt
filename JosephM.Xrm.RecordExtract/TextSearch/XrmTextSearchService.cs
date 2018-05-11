using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.RecordExtract.RecordExtract;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    public class XrmTextSearchService : TextSearchService
    {
        public XrmTextSearchService(XrmRecordService service, DocumentWriter.DocumentWriter documentWriter, XrmRecordExtractService recordExtractService)
            : base(service, documentWriter, recordExtractService)
        {
        }
    }
}