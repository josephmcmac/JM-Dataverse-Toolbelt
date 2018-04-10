using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Service;

namespace JosephM.CodeGenerator.FetchToJavascript
{
    public class FetchToJavascriptDialog :
        ServiceRequestDialog<FetchToJavascriptService, FetchToJavascriptRequest, FetchToJavascriptResponse, ServiceResponseItem>
    {
        public FetchToJavascriptDialog(FetchToJavascriptService service, IDialogController dialogController)
            : base(service, dialogController)
        {
        }
    }
}