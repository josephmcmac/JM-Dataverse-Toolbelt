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
            : base(service, dialogController, lookupService, nextButtonLabel: "Import")
        {
            var validationDialog = new CustomisationImportValidationDialog(this, Request);
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
            if (Response.ImportedItems != null)
            {
                foreach(var typeGroup in Response.ImportedItems.GroupBy(it => it.Type))
                {
                    addProperty($"Imported Count {typeGroup.Key}", typeGroup.Count().ToString());
                }
            }
            return dictionary;
        }
    }
}