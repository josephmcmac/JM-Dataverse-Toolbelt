using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Service;
using JosephM.Record.Xrm.XrmRecord;
using System.Windows.Forms;

namespace JosephM.CodeGenerator.JavaScriptOptions
{
    [RequiresConnection]
    public class JavaScriptOptionsDialog :
        ServiceRequestDialog<JavaScriptOptionsService, JavaScriptOptionsRequest, JavaScriptOptionsResponse, ServiceResponseItem>
    {
        public JavaScriptOptionsDialog(JavaScriptOptionsService service, IDialogController dialogController, XrmRecordService xrmRecordService)
            : base(service, dialogController, xrmRecordService)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            if (!string.IsNullOrWhiteSpace(Response.Javascript))
            {
                AddCompletionOption("Copy To Clipboard", () =>
                {
                    Clipboard.SetText(Response.Javascript);
                });
            }
        }
    }
}