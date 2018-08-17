using JosephM.Application.Desktop.Module.Crud;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Extentions;
using System.Collections.Generic;

namespace JosephM.XrmModule.Crud
{
    public class XrmCrudDialog : CrudDialog
    {
        public XrmCrudDialog(XrmRecordService recordService, IDialogController dialogController)
            : base(dialogController, recordService)
        {
            this.AddRedirectToConnectionEntryWhenNotConnected(recordService);
        }

        public override IEnumerable<string> AdditionalExplicitTypes => new[] { Entities.uom, Entities.savedquery, Entities.calendar, Entities.role, Entities.sdkmessageprocessingstep, Entities.activitymimeattachment, Entities.organization, Entities.usersettings, Entities.productpricelevel };
    }
}
