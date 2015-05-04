#region

using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.Application.Dialog;
using JosephM.Record.Xrm.XrmRecord;

#endregion

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    public class XrmRecordExtractSettingsDialog : AppSettingsDialog<IRecordExtractSettings, RecordExtractSettings>
    {
        public XrmRecordExtractSettingsDialog(IDialogController dialogController, PrismContainer container,
            XrmRecordService recordService)
            : base(dialogController, container, recordService)
        {
        }
    }
}