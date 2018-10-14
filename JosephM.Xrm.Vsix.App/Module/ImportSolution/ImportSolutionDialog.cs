using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.Xrm.Vsix.Module.ImportSolution
{
    public class ImportSolutionDialog
        : ServiceRequestDialog<ImportSolutionService, ImportSolutionRequest, ImportSolutionResponse, ImportSolutionResponseItem>
    {
        public ImportSolutionDialog(ImportSolutionService service, IDialogController dialogController)
            : base(service, dialogController)
        {

        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            CompletionMessage = $"The Solution Has Been Deployed Into {Request.Connection}";
            AddCompletionOption($"Open {Request.Connection}", () =>
            {
                try
                {
                    ApplicationController.StartProcess(new XrmRecordService(Request.Connection).WebUrl);
                }
                catch (Exception ex)
                {
                    ApplicationController.ThrowException(ex);
                }
            });
        }
    }
}
