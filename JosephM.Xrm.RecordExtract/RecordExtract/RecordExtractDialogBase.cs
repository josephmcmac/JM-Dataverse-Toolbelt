using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Record.IService;
using System;
using System.Diagnostics;

namespace JosephM.Xrm.RecordExtract.RecordExtract
{
    public abstract class RecordExtractDialogBase<TRecordExtractService> :
        ServiceRequestDialog
            <TRecordExtractService, RecordExtractRequest, RecordExtractResponse, RecordExtractResponseItem>
        where TRecordExtractService : RecordExtractService
    {
        protected RecordExtractDialogBase(TRecordExtractService service, IDialogController dialogController,
            IRecordService recordService)
            : base(service, dialogController, recordService)
        {
        }

        protected override void ProcessCompletionExtention()
        {
            AddCompletionOption("Open Document", OpenFile);
            AddCompletionOption("Open Folder", OpenFolder);
            CompletionMessage = "Document Successfully Generated";
        }

        public void OpenFile()
        {
            try
            {
                Process.Start(Response.FileNameQualified);
            }
            catch (Exception ex)
            {
                ApplicationController.UserMessage(ex.DisplayString());
            }
        }

        public void OpenFolder()
        {
            try
            {
                Process.Start(Response.Folder);
            }
            catch (Exception ex)
            {
                ApplicationController.UserMessage(ex.DisplayString());
            }
        }
    }
}