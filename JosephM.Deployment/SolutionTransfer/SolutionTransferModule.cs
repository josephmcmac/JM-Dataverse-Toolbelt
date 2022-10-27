using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Core.AppConfig;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using System;
using JosephM.Deployment.SolutionsImport;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Xrm.Schema;

namespace JosephM.Deployment.SolutionTransfer
{
    [MyDescription("Deploy a solution from one instance into another")]
    public class SolutionTransferModule
        : ServiceRequestModule<SolutionTransferDialog, SolutionTransferService, SolutionTransferRequest, SolutionTransferResponse, SolutionTransferResponseItem>
    {
        public override string MenuGroup => "Solution Deployment";

        public override void RegisterTypes()
        {
            base.RegisterTypes();

            AddDialogCompletionLinks();

            this.AddSolutionDetailsFormEvent(typeof(SolutionTransferRequest), nameof(SolutionTransferRequest.Solution), nameof(SolutionTransferRequest.InstallAsManaged), nameof(SolutionTransferRequest.SourceVersionForRelease));
        }

        private void AddDialogCompletionLinks()
        {
            this.AddCustomFormFunction(new CustomFormFunction("OPENINSTANCE"
                , (r) => $"Open {r.GetRecord().GetField(nameof(SolutionTransferResponse.ConnectionDeployedInto))}"
                , (r) =>
                {
                    try
                    {
                        var connection = r.GetRecord().GetField(nameof(SolutionTransferResponse.ConnectionDeployedInto)) as IXrmRecordConfiguration;
                        var serviceFactory = ApplicationController.ResolveType<IOrganizationConnectionFactory>();
                        ApplicationController.StartProcess(new XrmRecordService(connection, serviceFactory).WebUrl);
                    }
                    catch (Exception ex)
                    {
                        ApplicationController.ThrowException(ex);
                    }
                }
                , (r) => r.GetRecord().GetField(nameof(SolutionTransferResponse.ConnectionDeployedInto)) != null)
                , typeof(SolutionTransferResponse));
        }
    }
}