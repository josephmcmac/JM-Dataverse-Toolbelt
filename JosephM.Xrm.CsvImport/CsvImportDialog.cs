using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.CsvImport
{
    [RequiresConnection]
    public class CsvImportDialog :
        ServiceRequestDialog
            <CsvImportService, CsvImportRequest,
                CsvImportResponse, CsvImportResponseItem>
    {
        public CsvImportDialog(CsvImportService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService, nextButtonLabel: "Import")
        {
            var validationDialog = new CsvImportValidationDialog(this, Request);
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
            addProperty("Match By Name", Request.MatchByName.ToString());
            addProperty("Update Only", Request.UpdateOnly.ToString());
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