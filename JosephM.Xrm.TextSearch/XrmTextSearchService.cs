using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Xrm.TextSearch
{
    public class XrmTextSearchService : TextSearchService
    {
        public XrmTextSearchService(XrmRecordService service)
            : base(service)
        {
        }
    }
}