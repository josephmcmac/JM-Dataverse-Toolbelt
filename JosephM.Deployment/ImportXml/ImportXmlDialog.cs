using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Deployment.DataImport;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.Deployment.ImportXml
{
    [RequiresConnection]
    public class ImportXmlDialog :
        ServiceRequestDialog
            <ImportXmlService, ImportXmlRequest,
                ImportXmlResponse, DataImportResponseItem>
    {
        public ImportXmlDialog(ImportXmlService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            CompletionMessage = "The Import Process Has Completed";
            AddCompletionOption($"Open {Service?.XrmRecordService?.XrmRecordConfiguration?.ToString()}", () =>
            {
                try
                {
                    ApplicationController.StartProcess(Service?.XrmRecordService?.WebUrl);
                }
                catch (Exception ex)
                {
                    ApplicationController.ThrowException(ex);
                }
            });
        }
    }
}