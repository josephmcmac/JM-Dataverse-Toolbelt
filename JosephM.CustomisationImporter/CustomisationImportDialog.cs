using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.CustomisationImporter.Service;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.CustomisationImporter
{
    [RequiresConnection]
    public class CustomisationImportDialog :
        ServiceRequestDialog
            <XrmCustomisationImportService, CustomisationImportRequest, CustomisationImportResponse,
                CustomisationImportResponseItem>
    {
        public CustomisationImportDialog(XrmCustomisationImportService service, IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
            var validationDialog = new CustomisationImportValidationDialog(this, Request);
            SubDialogs = SubDialogs.Union(new[] { validationDialog }).ToArray();
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            if (Response.HasResponseItems)
                CompletionMessage = "The Import Completed But The Following Errors Were Encountered During The Import";
            else
                CompletionMessage = "The Customisations Have Been Imported And Published";
        }

        protected override IDictionary<string, string> GetPropertiesForCompletedLog()
        {
            var dictionary = base.GetPropertiesForCompletedLog();
            void addProperty(string name, string value)
            {
                if (!dictionary.ContainsKey(name))
                    dictionary.Add(name, value);
            }
            if (Response.ImportedItems != null)
            {
                foreach(var typeGroup in Response.ImportedItems.GroupBy(it => it.Type))
                {
                    addProperty(($"Imported {typeGroup.Key} Count", typeGroup.Count().ToString());
                }
            }
            return dictionary;
        }
    }
}