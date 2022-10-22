using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Xrm.TextSearch
{
    [RequiresConnection(escapeConnectionCheckProperty: nameof(LoadedFromConnection))]
    public class XrmTextSearchDialog : TextSearchDialogBase<XrmTextSearchService>
    {
        public XrmTextSearchDialog(XrmTextSearchService service, IDialogController dialogController,
            XrmRecordService xrmRecordService)
            : base(service, dialogController, xrmRecordService)
        {
        }

        protected override bool UseProgressControlUi => true;

        public bool LoadedFromConnection { get; set; }
    }
}