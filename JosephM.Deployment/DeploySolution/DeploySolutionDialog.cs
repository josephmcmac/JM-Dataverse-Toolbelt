using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;

namespace JosephM.Deployment.DeploySolution
{
    public class DeploySolutionDialog :
        ServiceRequestDialog
            <DeploySolutionService, DeploySolutionRequest,
                DeploySolutionResponse, DeploySolutionResponseItem>
    {
        public DeploySolutionDialog(DeploySolutionService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
        }
    }
}