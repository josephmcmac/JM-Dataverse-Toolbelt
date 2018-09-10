using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Service;
using JosephM.Deployment.DataImport;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.Deployment.DeployPackage
{
    public class DeployPackageDialog :
        ServiceRequestDialog
            <DeployPackageService, DeployPackageRequest,
                ServiceResponseBase<DataImportResponseItem>, DataImportResponseItem>
    {
        public DeployPackageDialog(DeployPackageService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            CompletionMessage = $"The Package Has Been Deployed Into {Request.Connection}";
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