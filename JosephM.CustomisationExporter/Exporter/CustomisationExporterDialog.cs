using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.CustomisationExporter.Exporter
{
    [RequiresConnection]
    public class CustomisationExporterDialog :
        ServiceRequestDialog
            <CustomisationExporterService, CustomisationExporterRequest, CustomisationExporterResponse,
                CustomisationExporterResponseItem>
    {
        public CustomisationExporterDialog(CustomisationExporterService service, IDialogController dialogController,
            XrmRecordService recordService)
            : base(service, dialogController, recordService)
        {
        }
    }
}