#region

using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.IService;

#endregion

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    public abstract class RecordExtractSettingsDialog : AppSettingsDialog<IRecordExtractSettings, RecordExtractSettings>
    {
        protected RecordExtractSettingsDialog(IDialogController dialogController,
            IRecordService recordService)
            : base(dialogController, recordService)
        {
        }
    }
}