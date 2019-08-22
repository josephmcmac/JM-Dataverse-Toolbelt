using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Deployment.DataImport;
using JosephM.Deployment.ImportXml;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment.DeployPackage
{
    public class DeployPackageDialog :
        ServiceRequestDialog
            <DeployPackageService, DeployPackageRequest,
                DeployPackageResponse, DataImportResponseItem>
    {
        public DeployPackageDialog(DeployPackageService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
            var validationDialog = new ImportXmlValidationDialog(this, Request);
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