#region

using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.Application.Dialog;
using JosephM.Record.IService;

#endregion

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    public abstract class RecordExtractSettingsDialog : AppSettingsDialog<IRecordExtractSettings, RecordExtractSettings>
    {
        protected RecordExtractSettingsDialog(IDialogController dialogController, PrismContainer container,
            IRecordService recordService)
            : base(dialogController, container, recordService)
        {
        }
    }
}