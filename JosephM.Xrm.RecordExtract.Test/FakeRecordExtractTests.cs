using JosephM.Core.Test;
using JosephM.Record.Application.Fakes;
using JosephM.Xrm.RecordExtract.RecordExtract;
using JosephM.Xrm.RecordExtract.Test.RecordExtract;
using JosephM.Xrm.RecordExtract.Test.TextSearch;
using JosephM.Xrm.RecordExtract.TextSearch;

namespace JosephM.Xrm.RecordExtract.Test
{
    public class FakeRecordExtractTests : CoreTest
    {
        private DocumentWriter.DocumentWriter DocumentWriter
        {
            get { return new DocumentWriter.DocumentWriter(); }
        }

        protected TestRecordExtractService TestRecordExtractService
        {
            get
            {
                return new TestRecordExtractService(FakeRecordService.Get(), new RecordExtractSettings(), DocumentWriter);
            }
        }

        protected TestTextSearchService TestTextSearchService
        {
            get
            {
                return new TestTextSearchService(FakeRecordService.Get(), new TextSearchSettings(), DocumentWriter,
                    TestRecordExtractService);
            }
        }
    }
}