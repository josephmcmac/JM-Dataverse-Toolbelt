using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Deployment.DataImport;
using JosephM.Record.Xrm.XrmRecord;
using System;
using System.Collections.Generic;

namespace JosephM.Deployment.MigrateRecords
{
    public class MigrateRecordsDialog :
        ServiceRequestDialog
            <MigrateRecordsService, MigrateRecordsRequest,
                MigrateRecordsResponse, DataImportResponseItem>
    {
        public MigrateRecordsDialog(MigrateRecordsService service,
            IDialogController dialogController)
            : base(service, dialogController)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            CompletionMessage = "The Record Migration Has Completed";
            AddCompletionOption($"Open {Request.TargetConnection}", () =>
            {
                try
                {
                    ApplicationController.StartProcess(new XrmRecordService(Request.TargetConnection).WebUrl);
                }
                catch (Exception ex)
                {
                    ApplicationController.ThrowException(ex);
                }
            });
        }

        protected override IDictionary<string, string> GetPropertiesForCompletedLog()
        {
            var dictionary = base.GetPropertiesForCompletedLog();
            void addProperty(string name, string value)
            {
                if (!dictionary.ContainsKey(name))
                    dictionary.Add(name, value);
            }
            addProperty("Include NN", Request.IncludeNNRelationshipsBetweenEntities.ToString());
            addProperty("Include Notes", Request.IncludeNotes.ToString());
            addProperty("Include Owner", Request.IncludeOwner.ToString());
            addProperty("Mask Emails", Request.MaskEmails.ToString());
            addProperty("Match By Name", Request.MatchByName.ToString());
            if (Response.IsImportSummary)
            {
                foreach (var typeGroup in Response.ImportSummary)
                {
                    addProperty($"Import {typeGroup.Type} Count", typeGroup.Total.ToString());
                    addProperty($"Import {typeGroup.Type} Errors", typeGroup.Errors.ToString());
                }
            }
            return dictionary;
        }
    }
}