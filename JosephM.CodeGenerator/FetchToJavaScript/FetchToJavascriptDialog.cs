using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Service;
using System.Windows.Forms;

namespace JosephM.CodeGenerator.FetchToJavascript
{
    public class FetchToJavascriptDialog :
        ServiceRequestDialog<FetchToJavascriptService, FetchToJavascriptRequest, FetchToJavascriptResponse, ServiceResponseItem>
    {
        public FetchToJavascriptDialog(FetchToJavascriptService service, IDialogController dialogController)
            : base(service, dialogController)
        {
            
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            if(!string.IsNullOrWhiteSpace(Response.Javascript))
            {
                AddCompletionOption("Copy To Clipboard", () =>
                {
                    Clipboard.SetText(Response.Javascript);
                });
            }
        }
    }
}