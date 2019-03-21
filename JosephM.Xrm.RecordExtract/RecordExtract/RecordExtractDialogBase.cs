using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Linq;

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
            CompletionMessage = "The Document Has Been Generated";
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

        protected override IDictionary<string, string> GetPropertiesForCompletedLog()
        {
            var dictionary = base.GetPropertiesForCompletedLog();
            void addProperty(string name, string value)
            {
                if (!dictionary.ContainsKey(name))
                    dictionary.Add(name, value);
            }
            addProperty("Record Type", Request.RecordType?.Key);
            addProperty("Related Detail", Request.DetailOfRelatedRecords.ToString());
            addProperty("Related Created", Request.IncludeCreatedByAndOn.ToString());
            addProperty("Related Owner", Request.IncludeCrmOwner.ToString());
            addProperty("Related Modified", Request.IncludeModifiedByAndOn.ToString());
            addProperty("Related State", Request.IncludeState.ToString());
            addProperty("Related Status", Request.IncludeStatus.ToString());
            addProperty("Custom HTML Field Count", (Request.CustomHtmlFields?.Count() ?? 0).ToString());
            addProperty("Exclude Field Count", (Request.FieldsToExclude?.Count() ?? 0).ToString());
            addProperty("Exclude Type Count", (Request.RecordTypesToExclude?.Count() ?? 0).ToString());
            addProperty("Exclude Type Only Name Count", (Request.RecordTypesOnlyDisplayName?.Count() ?? 0).ToString());
            addProperty("Strip HTML Tags", Request.StripHtmlTags.ToString());
            return dictionary;
        }
    }
}