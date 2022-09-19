using JosephM.Application.Application;
using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.DataImportExport.Import;
using System;

namespace JosephM.Xrm.MigrateRecords
{
    [MyDescription("Migrate A Set Of Records From One CRM Instance Into Another")]
    public class MigrateRecordsModule
        : ServiceRequestModule<MigrateRecordsDialog, MigrateRecordsService, MigrateRecordsRequest, MigrateRecordsResponse, DataImportResponseItem>
    {
        public override string MenuGroup => "Import & Migrate Data";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
            var configManager = Resolve<ISettingsManager>();
            configManager.ProcessNamespaceChange(GetType().Namespace, "JosephM.Deployment.MigrateRecords");
            AddDialogCompletionLinks();
        }

        private void AddDialogCompletionLinks()
        {
            this.AddCustomFormFunction(new CustomFormFunction("OPENINSTANCE"
                , (r) => $"Open {r.GetRecord().GetField(nameof(MigrateRecordsResponse.ConnectionMigratedInto))}"
                , (r) =>
                {
                    try
                    {
                        var connection = r.GetRecord().GetField(nameof(MigrateRecordsResponse.ConnectionMigratedInto)) as IXrmRecordConfiguration;
                        var serviceFactory = ApplicationController.ResolveType<IOrganizationConnectionFactory>();
                        ApplicationController.StartProcess(new XrmRecordService(connection, serviceFactory).WebUrl);
                    }
                    catch (Exception ex)
                    {
                        ApplicationController.ThrowException(ex);
                    }
                }
                , (r) => r.GetRecord().GetField(nameof(MigrateRecordsResponse.ConnectionMigratedInto)) != null)
                , typeof(MigrateRecordsResponse));
        }
    }
}