using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.Infrastructure.Module.Crud;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Prism.XrmModule.Crud
{
    public class XrmCrudDialog : CrudDialog
    {
        public XrmCrudDialog(XrmRecordService recordService, IDialogController dialogController)
            : base(dialogController, recordService)
        {

        }
    }
}
