using System;
using System.Collections.Generic;
using System.Diagnostics;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Record.IService;

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

        protected override IDictionary<string, IEnumerable<string>> LookupAllowedValues
        {
            get { return new Dictionary<string, IEnumerable<string>>() {{"RecordType", Service.AllowedRecordTypes}}; }
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