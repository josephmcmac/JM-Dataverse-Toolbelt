using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.Application.Dialog;
using JosephM.Record.Application.Fakes;
using JosephM.Xrm.RecordExtract.RecordExtract;

namespace JosephM.Xrm.RecordExtract.Test.RecordExtract
{
    public class TestRecordExtractSettingsDialog : AppSettingsDialog<IRecordExtractSettings, RecordExtractSettings>
    {
        public TestRecordExtractSettingsDialog(IDialogController dialogController, PrismContainer container,
            FakeRecordService recordService)
            : base(dialogController, container, recordService)
        {
        }
    }
}