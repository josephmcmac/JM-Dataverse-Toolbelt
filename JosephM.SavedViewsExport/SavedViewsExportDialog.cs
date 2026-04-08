using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.SavedViewsExport
{
    [RequiresConnection]
    public class SavedViewsExportDialog :
        ServiceRequestDialog
            <SavedViewsExportService, SavedViewsExportRequest, SavedViewsExportResponse,
                SavedViewsExportResponseItem>
    {
        public SavedViewsExportDialog(SavedViewsExportService service, IDialogController dialogController,
            XrmRecordService recordService)
            : base(service, dialogController, recordService)
        {
        }

        protected override bool UseProgressControlUi => true;
    }
}