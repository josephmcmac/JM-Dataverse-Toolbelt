using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Record.IService;
using System;

namespace JosephM.Xrm.RecordExtract.TextSearch
{
    public abstract class TextSearchDialogBase<TTextSearchService> :
        ServiceRequestDialog<TTextSearchService, TextSearchRequest, TextSearchResponse, TextSearchResponseItem>
        where TTextSearchService : TextSearchService
    {
        protected TextSearchDialogBase(TTextSearchService service, IDialogController dialogController,
            IRecordService recordService)
            : base(service, dialogController, recordService)
        {
        }

        private string _tabLabel = "Text Search";
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

        protected override void ProcessCompletionExtention()
        {
            if (Request.GenerateDocument)
            {
                AddCompletionOption("Open Document", OpenFile);
                AddCompletionOption("Open Folder", OpenFolder);
            }
        }

        public void OpenFile()
        {
            try
            {
                ApplicationController.StartProcess(Response.FileNameQualified);
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
                ApplicationController.StartProcess(Response.Folder);
            }
            catch (Exception ex)
            {
                ApplicationController.UserMessage(ex.DisplayString());
            }
        }
    }
}