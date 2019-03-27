using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Core.Service;
using System.Windows.Forms;

namespace JosephM.CodeGenerator.FetchToJavascript
{
    [MyDescription("Generate C# Or JavaScript Code For A Multiline String Value")]
    public class FetchToJavascriptModule :
        ServiceRequestModule
            <FetchToJavascriptDialog, FetchToJavascriptService, FetchToJavascriptRequest, FetchToJavascriptResponse, ServiceResponseItem>
    {
        public override string MenuGroup => "Code Generation";

        public override string MainOperationName => "Convert Fetch To Javascript";

        public override void RegisterTypes()
        {
            base.RegisterTypes();

            AddDialogCompletionLinks();
        }

        private void AddDialogCompletionLinks()
        {
            this.AddCustomFormFunction(new CustomFormFunction("COPYTOCLIPBOARD", "Copy To Clipboard"
                , (r) => Clipboard.SetText(r.GetRecord().GetStringField(nameof(FetchToJavascriptResponse.Javascript)))
                , (r) => !string.IsNullOrWhiteSpace(r.GetRecord().GetStringField(nameof(FetchToJavascriptResponse.Javascript))))
                , typeof(FetchToJavascriptResponse));
        }
    }
}