using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;

namespace JosephM.InstanceComparer
{
    public class InstanceComparerDialog :
        ServiceRequestDialog<InstanceComparerService, InstanceComparerRequest, InstanceComparerResponse, InstanceComparerResponseItem>
    {
        public InstanceComparerDialog(IDialogController dialogController)
            : base(new InstanceComparerService(), dialogController)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();

            if (Response.AreDifferences)
                CompletionMessage = "Differences Were Found Between The Environments";
            else
                CompletionMessage = "No Difference Were Found";
        }
    }
}