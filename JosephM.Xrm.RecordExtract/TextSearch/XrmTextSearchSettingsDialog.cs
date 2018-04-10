#region

using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;

#endregion

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    public class XrmTextSearchSettingsDialog : TextSearchSettingsDialogBase
    {
        public XrmTextSearchSettingsDialog(IDialogController dialogController, 
            XrmRecordService recordService)
            : base(dialogController, recordService)
        {
        }
    }
}