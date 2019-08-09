using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    [MyDescription("Generate A Document Detailing The Field Values And Related Records For A Specific Record In The Dynamics Instance")]
    public class XrmRecordExtractModule :
        ServiceRequestModule
            <XrmRecordExtractDialog, XrmRecordExtractService, RecordExtractRequest, RecordExtractResponse, RecordExtractResponseItem>
    {
        public override string MainOperationName
        {
            get { return "Record Report"; }
        }

        public override string MenuGroup => "Reports";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddDialogCompletionLinks();
        }

        private void AddDialogCompletionLinks()
        {
            this.AddCustomFormFunction(new CustomFormFunction("OPENDOCUMENT", "Open Document"
                , (r) => r.ApplicationController.StartProcess(r.GetRecord().GetStringField(nameof(RecordExtractResponse.FileNameQualified)))
                , (r) => !string.IsNullOrWhiteSpace(r.GetRecord().GetStringField(nameof(RecordExtractResponse.FileNameQualified))))
                , typeof(RecordExtractResponse));
            this.AddCustomFormFunction(new CustomFormFunction("OPENFOLDER", "Open Folder"
                , (r) => r.ApplicationController.StartProcess(r.GetRecord().GetStringField(nameof(RecordExtractResponse.Folder)))
                , (r) => !string.IsNullOrWhiteSpace(r.GetRecord().GetStringField(nameof(RecordExtractResponse.Folder))))
                , typeof(RecordExtractResponse));
        }
    }
}