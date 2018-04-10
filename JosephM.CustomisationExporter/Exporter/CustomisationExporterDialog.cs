using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.CustomisationExporter.Exporter
{
    public class CustomisationExporterDialog :
        ServiceRequestDialog
            <CustomisationExporterService, CustomisationExporterRequest, CustomisationExporterResponse,
                CustomisationExporterResponseItem>
    {
        public CustomisationExporterDialog(CustomisationExporterService service, IDialogController dialogController,
            XrmRecordService recordService)
            : base(service, dialogController, recordService)
        {
        }

        protected override void ProcessCompletionExtention()
        {
            if (!Response.TypesFileName.IsNullOrWhiteSpace())
                AddCompletionOption("Open Types", OpenTypesFile);
            if (!Response.FieldsFileName.IsNullOrWhiteSpace())
                AddCompletionOption("Open Fields", OpenFieldsFile);
            if (!Response.RelationshipsFileName.IsNullOrWhiteSpace())
                AddCompletionOption("Open Relationships", OpenRelationshipsFile);
            if (!Response.OptionSetsFileName.IsNullOrWhiteSpace())
                AddCompletionOption("Open Options", OpenOptionsFile);
            AddCompletionOption("Open Folder", OpenFolder);
            CompletionMessage = "Document Successfully Generated";
        }

        public void OpenFolder()
        {
            ApplicationController.StartProcess("explorer", Response.Folder);
        }

        public void OpenOptionsFile()
        {
            ApplicationController.StartProcess(Response.OptionSetsFileNameQualified);
        }

        public void OpenTypesFile()
        {
            ApplicationController.StartProcess(Response.TypesFileNameQualified);
        }

        public void OpenFieldsFile()
        {
            ApplicationController.StartProcess(Response.FieldsFileNameQualified);
        }

        public void OpenRelationshipsFile()
        {
            ApplicationController.StartProcess(Response.RelationshipsFileNameQualified);
        }
    }
}