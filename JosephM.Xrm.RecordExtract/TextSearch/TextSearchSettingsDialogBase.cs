#region

using JosephM.Application.Prism.Module.Settings;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.IService;

#endregion

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    public abstract class TextSearchSettingsDialogBase : AppSettingsDialog<ITextSearchSettings, TextSearchSettings>
    {
        protected TextSearchSettingsDialogBase(IDialogController dialogController, IRecordService recordService)
            : base(dialogController, recordService)
        {
        }
    }
}