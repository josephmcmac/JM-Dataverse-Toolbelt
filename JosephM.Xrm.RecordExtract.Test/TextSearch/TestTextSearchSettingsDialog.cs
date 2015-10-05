#region

using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.Application.Fakes;
using JosephM.Xrm.RecordExtract.TextSearch;

#endregion

namespace JosephM.Xrm.RecordExtract.Test.TextSearch
{
    public class TestTextSearchSettingsDialog : TextSearchSettingsDialogBase
    {
        public TestTextSearchSettingsDialog(IDialogController dialogController, 
            FakeRecordService recordService)
            : base(dialogController, recordService)
        {
        }
    }
}