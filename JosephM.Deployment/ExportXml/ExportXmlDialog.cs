using System;
using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Deployment.ExportXml
{
    [RequiresConnection]
    public class ExportXmlDialog :
        ServiceRequestDialog
            <ExportXmlService, ExportXmlRequest,
                ExportXmlResponse, ExportXmlResponseItem>
    {
        public ExportXmlDialog(ExportXmlService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
        }

        private string _tabLabel = "Export XML";
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

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            CompletionMessage = "The XML Files Have Been Created";
            AddCompletionOption("Open Folder", () => ApplicationController.StartProcess(Request.Folder.FolderPath));
        }
    }
}