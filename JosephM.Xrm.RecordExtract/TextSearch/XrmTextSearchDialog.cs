using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.Extentions;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    public class XrmTextSearchDialog : TextSearchDialogBase<XrmTextSearchService>
    {
        public XrmTextSearchDialog(XrmTextSearchService service, IDialogController dialogController,
            XrmRecordService xrmRecordService)
            : base(service, dialogController, xrmRecordService)
        {
            this.AddRedirectToConnectionEntryWhenNotConnected(xrmRecordService);
        }
    }
}