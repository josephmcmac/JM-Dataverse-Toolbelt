#region

using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Record.Application.Dialog;
using JosephM.Record.IService;

#endregion

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    public abstract class TextSearchSettingsDialogBase : AppSettingsDialog<ITextSearchSettings, TextSearchSettings>
    {
        protected TextSearchSettingsDialogBase(IDialogController dialogController, PrismContainer container,
            IRecordService recordService)
            : base(dialogController, container, recordService)
        {
        }
    }
}