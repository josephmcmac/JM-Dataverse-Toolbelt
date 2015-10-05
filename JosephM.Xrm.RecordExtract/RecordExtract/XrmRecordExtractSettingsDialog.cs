#region

using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.Xrm.XrmRecord;

#endregion

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    public class XrmRecordExtractSettingsDialog : AppSettingsDialog<IRecordExtractSettings, RecordExtractSettings>
    {
        public XrmRecordExtractSettingsDialog(IDialogController dialogController, XrmRecordService recordService)
            : base(dialogController, recordService)
        {
        }
    }
}