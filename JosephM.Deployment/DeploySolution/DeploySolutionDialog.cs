using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Deployment.DataImport;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.Deployment.DeploySolution
{
    public class DeploySolutionDialog :
        ServiceRequestDialog
            <DeploySolutionService, DeploySolutionRequest,
                DeploySolutionResponse, DataImportResponseItem>
    {
        public DeploySolutionDialog(DeploySolutionService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            CompletionMessage = $"The Solution Has Been Deployed Into {Request.TargetConnection}";
            AddCompletionOption($"Open {Request.TargetConnection}", () =>
            {
                try
                {
                    ApplicationController.StartProcess(new XrmRecordService(Request.TargetConnection).WebUrl);
                }
                catch (Exception ex)
                {
                    ApplicationController.ThrowException(ex);
                }
            });
        }
    }
}