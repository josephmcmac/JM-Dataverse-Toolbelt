#region

using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.Application.Dialog;
using JosephM.Record.Application.Fakes;
using JosephM.Xrm.RecordExtract.TextSearch;

#endregion

namespace JosephM.Xrm.RecordExtract.Test.TextSearch
{
    public class TestTextSearchSettingsDialog : TextSearchSettingsDialogBase
    {
        public TestTextSearchSettingsDialog(IDialogController dialogController, PrismContainer container,
            FakeRecordService recordService)
            : base(dialogController, container, recordService)
        {
        }
    }
}