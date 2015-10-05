#region

using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Prism;
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