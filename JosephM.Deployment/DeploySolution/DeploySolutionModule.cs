using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Deployment.DataImport;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.Deployment.DeploySolution
{
    [MyDescription("Deploy A Solution From One Instance into Another")]
    public class DeploySolutionModule
        : ServiceRequestModule<DeploySolutionDialog, DeploySolutionService, DeploySolutionRequest, DeploySolutionResponse, DeploySolutionResponseItem>
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
                , (r) => $"Open {r.GetRecord().GetField(nameof(DeploySolutionResponse.ConnectionDeployedInto))}"
                , (r) =>
                {
                    try
                    {
                        ApplicationController.StartProcess(new XrmRecordService(r.GetRecord().GetField(nameof(DeploySolutionResponse.ConnectionDeployedInto)) as IXrmRecordConfiguration).WebUrl);
                    }
                    catch (Exception ex)
                    {
                        ApplicationController.ThrowException(ex);
                    }
                }
                , (r) => r.GetRecord().GetField(nameof(DeploySolutionResponse.ConnectionDeployedInto)) != null)
                , typeof(DeploySolutionResponse));
        }
    }
}