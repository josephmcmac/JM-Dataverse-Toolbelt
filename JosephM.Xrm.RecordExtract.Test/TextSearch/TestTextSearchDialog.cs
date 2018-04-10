using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
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