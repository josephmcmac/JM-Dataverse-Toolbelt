using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment.ImportSql
{
    [RequiresConnection]
    public class ImportSqlDialog :
        ServiceRequestDialog
            <ImportSqlService, ImportSqlRequest,
                ImportSqlResponse, ImportSqlResponseItem>
    {
        public ImportSqlDialog(ImportSqlService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService, nextButtonLabel: "Import")
        {
            SetTabLabel("Import SQL");
            var validationDialog = new ImportSqlValidationDialog(this, Request);
            SubDialogs = SubDialogs.Union(new[] { validationDialog }).ToArray();
        }

        protected override IDictionary<string, string> GetPropertiesForCompletedLog()
        {
            var dictionary = base.GetPropertiesForCompletedLog();
            void addProperty(string name, string value)
            {
                if (!dictionary.ContainsKey(name))
                    dictionary.Add(name, value);
            }
            addProperty("Mask Emails", Request.MaskEmails.ToString());
            addProperty("Match By Name", Request.MatchRecordsByName.ToString());
            addProperty("Update Only", Request.UpdateOnly.ToString());
            addProperty("Send Notification At Completion", Request.SendNotificationAtCompletion.ToString());
            addProperty("Only Send Notification If Error", Request.OnlySendNotificationIfError.ToString());
            if (Response.IsImportSummary)
            {
                foreach (var typeGroup in Response.ImportSummary)
                {
                    addProperty($"Import Count {typeGroup.Type}", typeGroup.Total.ToString());
                    addProperty($"Import Errors {typeGroup.Type}", typeGroup.Errors.ToString());
                }
            }
            return dictionary;
        }
    }
}