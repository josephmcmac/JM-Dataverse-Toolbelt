using JosephM.Application.Modules;
using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.XrmModule.XrmConnection;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using System.Windows.Forms;

namespace JosephM.CodeGenerator.JavaScriptOptions
{
    [MyDescription("Generate JavaScript Code Constants For A Picklist Field")]
    [DependantModule(typeof(XrmConnectionModule))]
    public class JavaScriptOptionsModule :
        ServiceRequestModule
            <JavaScriptOptionsDialog, JavaScriptOptionsService, JavaScriptOptionsRequest, JavaScriptOptionsResponse, ServiceResponseItem>
    {
        public override string MenuGroup => "Code Generation";

        public override string MainOperationName => "JavaScript Options";

        public override void RegisterTypes()
        {
            base.RegisterTypes();

            AddDialogCompletionLinks();
        }

        private void AddDialogCompletionLinks()
        {
            this.AddCustomFormFunction(new CustomFormFunction("COPYTOCLIPBOARD", "Copy To Clipboard"
                , (r) => Clipboard.SetText(r.GetRecord().GetStringField(nameof(JavaScriptOptionsResponse.Javascript)))
                , (r) => !string.IsNullOrWhiteSpace(r.GetRecord().GetStringField(nameof(JavaScriptOptionsResponse.Javascript))))
                , typeof(JavaScriptOptionsResponse));
        }
    }
}