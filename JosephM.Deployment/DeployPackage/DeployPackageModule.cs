using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Core.AppConfig;
using JosephM.Deployment.DataImport;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
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
                        var connection = r.GetRecord().GetField(nameof(DeployPackageResponse.Connection)) as IXrmRecordConfiguration;
                        var serviceFactory = ApplicationController.ResolveType<IOrganizationConnectionFactory>();
                        ApplicationController.StartProcess(new XrmRecordService(connection, serviceFactory).WebUrl);
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