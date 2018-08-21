using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;

namespace JosephM.Xrm.Vsix.Module.DeployIntoField
{
    [RequiresConnection]
    public class DeployIntoFieldDialog
        : ServiceRequestDialog<DeployIntoFieldService, DeployIntoFieldRequest, DeployIntoFieldResponse, DeployIntoFieldResponseItem>
    {
        public DeployIntoFieldDialog(DeployIntoFieldService service, IDialogController dialogController)
            : base(service, dialogController)
        {
        }
    }
}
