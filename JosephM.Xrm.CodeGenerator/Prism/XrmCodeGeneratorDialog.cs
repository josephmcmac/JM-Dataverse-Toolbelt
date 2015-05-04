using System;
using System.Diagnostics;
using JosephM.Core.Extentions;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Record.Application.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.CodeGenerator.Service;

namespace JosephM.Xrm.CodeGenerator.Prism
{
    public class XrmCodeGeneratorDialog :
        ServiceRequestDialog
            <XrmCodeGeneratorService, CodeGeneratorRequest, CodeGeneratorResponse, CodeGeneratorResponseItem>
    {
        public XrmCodeGeneratorDialog(XrmCodeGeneratorService service, IDialogController dialogController, XrmRecordService xrmRecordService)
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
            try
            {
                Process.Start("notepad.exe", Response.FileName);
            }
            catch (Exception ex)
            {
                ApplicationController.UserMessage(ex.DisplayString());
            }
        }

        public void OpenFolder()
        {
            try
            {
                Process.Start(Response.Folder);
            }
            catch (Exception ex)
            {
                ApplicationController.UserMessage(ex.DisplayString());
            }
        }
    }
}