using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.Extentions;

namespace JosephM.CodeGenerator.CSharp
{
    public class CSharpDialog :
        ServiceRequestDialog<CSharpService, CSharpRequest, CSharpResponse, ServiceResponseItem>
    {
        public CSharpDialog(CSharpService service, IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
            this.AddRedirectToConnectionEntryWhenNotConnected(lookupService);
        }

        protected override void ProcessCompletionExtention()
        {
            if (!string.IsNullOrWhiteSpace(Response.FileName))
                AddCompletionOption("Open File", OpenFile);
            if (!string.IsNullOrWhiteSpace(Response.Folder))
                AddCompletionOption("Open Folder", OpenFolder);
            CompletionMessage = "Document Successfully Generated";
        }

        public void OpenFile()
        {
            ApplicationController.StartProcess("notepad.exe", Response.FileName);
        }

        public void OpenFolder()
        {
            ApplicationController.StartProcess("explorer", Response.Folder);
        }
    }
}