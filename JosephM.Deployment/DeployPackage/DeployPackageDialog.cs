using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Deployment.DataImport;
using JosephM.Record.Xrm.XrmRecord;
using System;
using System.Collections.Generic;

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
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            CompletionMessage = $"The Package Has Been Deployed Into {Request.Connection}";
            AddCompletionOption($"Open {Request.Connection}", () =>
            {
                try
                {
                    ApplicationController.StartProcess(new XrmRecordService(Request.Connection).WebUrl);
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