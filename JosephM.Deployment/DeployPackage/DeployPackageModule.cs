using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Deployment.DataImport;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.Deployment.DeployPackage
{
    [MyDescription("Deploy A Solution Package Into A Target CRM Instance")]
    public class DeployPackageModule
        : ServiceRequestModule<DeployPackageDialog, DeployPackageService, DeployPackageRequest, DeployPackageResponse, DataImportResponseItem>
    {
        public override string MenuGroup => "Deployment";

        public override void RegisterTypes()
        {
            base.RegisterTypes();

            AddDialogCompletionLinks();
        }

        private void AddDialogCompletionLinks()
        {
            this.AddCustomFormFunction(new CustomFormFunction("OPENINSTANCE"
                , (r) => $"Open {r.GetRecord().GetField(nameof(DeployPackageResponse.Connection))}"
                , (r) =>
                {
                    try
                    {
                        ApplicationController.StartProcess(new XrmRecordService(r.GetRecord().GetField(nameof(DeployPackageResponse.Connection)) as IXrmRecordConfiguration).WebUrl);
                    }
                    catch (Exception ex)
                    {
                        ApplicationController.ThrowException(ex);
                    }
                }
                , (r) => r.GetRecord().GetField(nameof(DeployPackageResponse.Connection)) != null)
                , typeof(DeployPackageResponse));
        }
    }
}