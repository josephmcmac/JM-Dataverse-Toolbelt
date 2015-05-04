using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    public class XrmRecordExtractService : RecordExtractService
    {
        public XrmRecordExtractService(XrmRecordService service, IRecordExtractSettings recordExtractSettings,
            DocumentWriter.DocumentWriter documentWriter)
            : base(service, recordExtractSettings, documentWriter)
        {
        }
    }
}