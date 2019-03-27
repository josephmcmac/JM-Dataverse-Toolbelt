using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Deployment.DataImport;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.Deployment.MigrateRecords
{
    [MyDescription("Migrate A Set Of Records From One CRM Instance Into Another")]
    public class MigrateRecordsModule
        : ServiceRequestModule<MigrateRecordsDialog, MigrateRecordsService, MigrateRecordsRequest, MigrateRecordsResponse, DataImportResponseItem>
    {
        public override string MenuGroup => "Data Import/Export";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
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
                        ApplicationController.StartProcess(new XrmRecordService(r.GetRecord().GetField(nameof(MigrateRecordsResponse.ConnectionMigratedInto)) as IXrmRecordConfiguration).WebUrl);
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