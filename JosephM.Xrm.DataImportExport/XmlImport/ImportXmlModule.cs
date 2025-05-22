using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.DataImportExport.Import;
using System;

namespace JosephM.Xrm.DataImportExport.XmlExport
{
    [MyDescription("Import Records Which Have Been Exported As XML Files Into A CRM Instance")]
    public class ImportXmlModule
        : ServiceRequestModule<ImportXmlDialog, ImportXmlService, ImportXmlRequest, ImportXmlResponse, DataImportResponseItem>
    {
        public override string MenuGroup => "Import & Migrate Data";

        public override string MainOperationName => "XML Import";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddDialogCompletionLinks();
        }

        private void AddDialogCompletionLinks()
        {
            this.AddCustomFormFunction(new CustomFormFunction("OPENINSTANCE"
                , (r) => $"Open {r.GetRecord().GetField(nameof(ImportXmlResponse.Connection))}"
                , (r) =>
                {
                    try
                    {
                        var connection = r.GetRecord().GetField(nameof(ImportXmlResponse.Connection)) as IXrmRecordConfiguration;
                        var serviceFactory = ApplicationController.ResolveType<IOrganizationConnectionFactory>();
                        ApplicationController.StartProcess(new XrmRecordService(connection, serviceFactory).WebUrl);
                    }
                    catch (Exception ex)
                    {
                        ApplicationController.ThrowException(ex);
                    }
                }
                , (r) => r.GetRecord().GetField(nameof(ImportXmlResponse.Connection)) != null)
                , typeof(ImportXmlResponse));
        }
    }
}