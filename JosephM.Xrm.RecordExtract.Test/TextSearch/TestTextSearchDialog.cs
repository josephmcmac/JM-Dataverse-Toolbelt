using JosephM.Record.Application.Dialog;
using JosephM.Record.Application.Fakes;
using JosephM.Xrm.RecordExtract.TextSearch;

namespace JosephM.Xrm.RecordExtract.Test.TextSearch
{
    public class TestTextSearchDialog : TextSearchDialogBase<TestTextSearchService>
    {
        public TestTextSearchDialog(TestTextSearchService service, IDialogController dialogController,
            FakeRecordService recordService)
            : base(service, dialogController, recordService)
        {
        }
    }
}