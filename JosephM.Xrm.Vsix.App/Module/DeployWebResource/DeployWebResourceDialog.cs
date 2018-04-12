using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;

namespace JosephM.Xrm.Vsix.Module.DeployWebResource
{
    public class DeployWebResourceDialog
        : ServiceRequestDialog<DeployWebResourceService, DeployWebResourceRequest, DeployWebResourceResponse, DeployWebResourceResponseItem>
    {
        public DeployWebResourceDialog(DeployWebResourceService service, IDialogController dialogController)
            : base(service, dialogController)
        {

        }
    }
}
