using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Attributes;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.SolutionComponentExporter
{
    [RequiresConnection(escapeConnectionCheckProperty: nameof(LoadedFromConnection))]
    public class SolutionComponentExporterDialog :
        ServiceRequestDialog
            <SolutionComponentExporterService, SolutionComponentExporterRequest, SolutionComponentExporterResponse,
                SolutionComponentExporterResponseItem>
    {
        public SolutionComponentExporterDialog(SolutionComponentExporterService service, IDialogController dialogController,
            XrmRecordService recordService)
            : base(service, dialogController, recordService)
        {
        }

        protected override bool UseProgressControlUi => true;

        public bool LoadedFromConnection { get; set; }
    }
}