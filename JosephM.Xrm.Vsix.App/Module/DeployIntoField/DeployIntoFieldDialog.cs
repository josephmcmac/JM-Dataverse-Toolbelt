using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;

namespace JosephM.Xrm.Vsix.Module.DeployIntoField
{
    public class DeployIntoFieldDialog
        : ServiceRequestDialog<DeployIntoFieldService, DeployIntoFieldRequest, DeployIntoFieldResponse, DeployIntoFieldResponseItem>
    {
        public DeployIntoFieldDialog(DeployIntoFieldService service, IDialogController dialogController)
            : base(service, dialogController)
        {

        }
    }
}
