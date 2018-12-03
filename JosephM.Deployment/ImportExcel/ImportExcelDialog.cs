using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System;
using System.Linq;

namespace JosephM.Deployment.ImportExcel
{
    [RequiresConnection]
    public class ImportExcelDialog :
        ServiceRequestDialog
            <ImportExcelService, ImportExcelRequest,
                ImportExcelResponse, ImportExcelResponseItem>
    {
        public ImportExcelDialog(ImportExcelService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
            var validationDialog = new ImportExcelValidationDialog(this, Request);
            SubDialogs = SubDialogs.Union(new[] { validationDialog }).ToArray();
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

        private string _tabLabel = "Import Excel";
        public override string TabLabel
        {
            get
            {
                return _tabLabel;
            }
        }

        public void SetTabLabel(string newLabel)
        {
            _tabLabel = newLabel;
        }
    }
}