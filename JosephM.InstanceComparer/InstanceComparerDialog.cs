using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.Infrastructure.Dialog;
using System.Linq;

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

            //if (Response.Differences.Any())
            //    CompletionItems.Add(Response.Differences);

            //todo finish off changing this to displaying results with csv download option

            if (Response.FileName != null)
                AddCompletionOption("Open CSV", () => ApplicationController.OpenFile(Response.FileName));
            if (Response.AreDifferences)
                CompletionMessage = "Differences Were Found Between The Environments. See The Generated CSV";
            else
                CompletionMessage = "No Difference Were Found";
        }
    }
}