using System;
using System.Diagnostics;
using JosephM.Core.Extentions;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Record.Application.Dialog;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Xrm.CustomisationExporter
{
    public class CustomisationExporterDialog :
        ServiceRequestDialog
            <CustomisationExporterService, CustomisationExporterRequest, CustomisationExporterResponse, CustomisationExporterResponseItem>
    {
        public CustomisationExporterDialog(CustomisationExporterService service, IDialogController dialogController,
            XrmRecordService recordService)
            : base(service, dialogController, recordService)
        {
        }

        protected override void ProcessCompletionExtention()
        {
            if(!Response.TypesFileName.IsNullOrWhiteSpace())
                AddCompletionOption("Open Types", OpenTypesFile);
            if (!Response.FieldsFileName.IsNullOrWhiteSpace())
                AddCompletionOption("Open Fields", OpenFieldsFile);
            if (!Response.RelationshipsFileName.IsNullOrWhiteSpace())
                AddCompletionOption("Open Relationships", OpenRelationshipsFile);
            if (!Response.OptionSetsFileName.IsNullOrWhiteSpace())
                AddCompletionOption("Open Options", OpenOptionsFile);
            AddCompletionOption("Open Folder", () => OpenFolder(Response.Folder));
            CompletionMessage = "Document Successfully Generated";
        }

        public void OpenOptionsFile()
        {
            try
            {
                Process.Start(Response.OptionSetsFileNameQualified);
            }
            catch (Exception ex)
            {
                ApplicationController.UserMessage(ex.DisplayString());
            }
        }

        public void OpenTypesFile()
        {
            try
            {
                Process.Start(Response.TypesFileNameQualified);
            }
            catch (Exception ex)
            {
                ApplicationController.UserMessage(ex.DisplayString());
            }
        }

        public void OpenFieldsFile()
        {
            try
            {
                Process.Start(Response.FieldsFileNameQualified);
            }
            catch (Exception ex)
            {
                ApplicationController.UserMessage(ex.DisplayString());
            }
        }

        public void OpenRelationshipsFile()
        {
            try
            {
                Process.Start(Response.RelationshipsFileNameQualified);
            }
            catch (Exception ex)
            {
                ApplicationController.UserMessage(ex.DisplayString());
            }
        }
    }
}