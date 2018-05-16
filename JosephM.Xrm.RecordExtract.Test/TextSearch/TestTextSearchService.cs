using JosephM.Application.ViewModel.Fakes;
using JosephM.Xrm.RecordExtract.Test.RecordExtract;
using JosephM.Xrm.RecordExtract.TextSearch;

namespace JosephM.Xrm.RecordExtract.Test.TextSearch
{
    public class TestTextSearchService : TextSearchService
    {
        public TestTextSearchService(FakeRecordService service, 
            DocumentWriter.DocumentWriter documentWriter)
            : base(service, documentWriter)
        {
        }
    }
}