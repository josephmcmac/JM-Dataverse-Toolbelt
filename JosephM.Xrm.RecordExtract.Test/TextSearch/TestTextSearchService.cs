using JosephM.Application.ViewModel.Fakes;
using JosephM.Record.Application.Fakes;
using JosephM.Xrm.RecordExtract.Test.RecordExtract;
using JosephM.Xrm.RecordExtract.TextSearch;

namespace JosephM.Xrm.RecordExtract.Test.TextSearch
{
    public class TestTextSearchService : TextSearchService
    {
        public TestTextSearchService(FakeRecordService service, ITextSearchSettings settings,
            DocumentWriter.DocumentWriter documentWriter, TestRecordExtractService recordExtractService)
            : base(service, settings, documentWriter, recordExtractService)
        {
        }
    }
}