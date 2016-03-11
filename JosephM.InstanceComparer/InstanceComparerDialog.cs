using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.Infrastructure.Dialog;

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
            if (Response.FileName != null)
                AddCompletionOption("Open CSV", () => ApplicationController.OpenFile(Response.FileName));
            if (Response.Differences)
                CompletionMessage = "Differences Were Found Between The Enviornments. See The Generated CSV";
            else
                CompletionMessage = "No Difference Were Found";
        }
    }
}