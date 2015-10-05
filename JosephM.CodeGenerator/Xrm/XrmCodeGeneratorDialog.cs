using JosephM.Application.ViewModel.Dialog;
using JosephM.CodeGenerator.Service;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.CodeGenerator.Xrm
{
    public class XrmCodeGeneratorDialog :
        ServiceRequestDialog
            <XrmCodeGeneratorService, CodeGeneratorRequest, CodeGeneratorResponse, CodeGeneratorResponseItem>
    {
        public XrmCodeGeneratorDialog(XrmCodeGeneratorService service, IDialogController dialogController,
            XrmRecordService xrmRecordService)
            : base(service, dialogController, xrmRecordService)
        {
        }

        protected override void ProcessCompletionExtention()
        {
            if (!string.IsNullOrWhiteSpace(Response.FileName))
                AddCompletionOption("Open File", OpenFile);
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