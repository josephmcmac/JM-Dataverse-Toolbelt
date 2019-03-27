using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Deployment.DataImport;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.Deployment.ImportXml
{
    [MyDescription("Import Records Which Have Been Exported As XML Files Into A CRM Instance")]
    public class ImportXmlModule
        : ServiceRequestModule<ImportXmlDialog, ImportXmlService, ImportXmlRequest, ImportXmlResponse, DataImportResponseItem>
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
                , (r) => $"Open {r.GetRecord().GetField(nameof(ImportXmlResponse.Connection))}"
                , (r) =>
                {
                    try
                    {
                        ApplicationController.StartProcess(new XrmRecordService(r.GetRecord().GetField(nameof(ImportXmlResponse.Connection)) as IXrmRecordConfiguration).WebUrl);
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