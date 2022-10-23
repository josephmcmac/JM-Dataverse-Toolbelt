using JosephM.Application;
using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.Core.FieldType;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.XrmModule.SavedXrmConnections;
using System;
using System.Linq;

namespace JosephM.Deployment.ImportSolution
{
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class ImportSolutionModule : ServiceRequestModule<ImportSolutionDialog, ImportSolutionService, ImportSolutionRequest, ImportSolutionResponse, ImportSolutionResponseItem>
    {
        public override void RegisterTypes()
        {
            base.RegisterTypes();

            AddDialogCompletionLinks();
        }

        public override string MenuGroup => "Solution Deployment";

        private void AddDialogCompletionLinks()
        {
            this.AddCustomFormFunction(new CustomFormFunction("OPENINSTANCE"
                , (r) => $"Open {r.GetRecord().GetField(nameof(ImportSolutionResponse.Connection))}"
                , (r) =>
                {
                    try
                    {
                        var connection = r.GetRecord().GetField(nameof(ImportSolutionResponse.Connection)) as IXrmRecordConfiguration;
                        var serviceFactory = ApplicationController.ResolveType<IOrganizationConnectionFactory>();
                        ApplicationController.StartProcess(new XrmRecordService(connection, serviceFactory).WebUrl);
                    }
                    catch (Exception ex)
                    {
                        ApplicationController.ThrowException(ex);
                    }
                }
                , (r) => r.GetRecord().GetField(nameof(ImportSolutionResponse.Connection)) != null)
                , typeof(ImportSolutionResponse));
        }
    }
}
