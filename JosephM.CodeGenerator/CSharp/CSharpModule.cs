using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.CodeGenerator.CSharp
{
    [MyDescription("Generate C# Code Constants For The Customisations In A CRM Instance")]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class CSharpModule :
        ServiceRequestModule
            <CSharpDialog, CSharpService, CSharpRequest, CSharpResponse, ServiceResponseItem>
    {
        public override string MenuGroup => "Code Generation";

        public override string MainOperationName => "C# Constants";

        public override void RegisterTypes()
        {
            base.RegisterTypes();

            AddDialogCompletionLinks();
        }

        private void AddDialogCompletionLinks()
        {
            this.AddCustomFormFunction(new CustomFormFunction("OPENFILE", "Open File"
                , (r) => r.ApplicationController.StartProcess("notepad.exe", r.GetRecord().GetStringField(nameof(CSharpResponse.FileName)))
                , (r) => !string.IsNullOrWhiteSpace(r.GetRecord().GetStringField(nameof(CSharpResponse.FileName))))
                , typeof(CSharpResponse));

            this.AddCustomFormFunction(new CustomFormFunction("OPENFOLDER", "Open Folder"
                , (r) => r.ApplicationController.StartProcess("explorer", r.GetRecord().GetStringField(nameof(CSharpResponse.Folder)))
                , (r) => !string.IsNullOrWhiteSpace(r.GetRecord().GetStringField(nameof(CSharpResponse.Folder))))
                , typeof(CSharpResponse));
        }
    }
}