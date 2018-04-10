using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Xrm.RecordExtract.RecordExtract;

namespace JosephM.Xrm.RecordExtract.Test.RecordExtract
{
    public class TestRecordExtractDialog : RecordExtractDialogBase<TestRecordExtractService>
    {
        public TestRecordExtractDialog(TestRecordExtractService service, IDialogController dialogController,
            FakeRecordService recordService)
            : base(service, dialogController, recordService)
        {
        }
    }
}