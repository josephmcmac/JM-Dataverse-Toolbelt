using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment.MigrateInternal
{
    [RequiresConnection(escapeConnectionCheckProperty: nameof(LoadedFromConnection))]
    public class MigrateInternalDialog :
        ServiceRequestDialog
            <MigrateInternalService, MigrateInternalRequest,
                MigrateInternalResponse, MigrateInternalResponseItem>
    {
        public MigrateInternalDialog(MigrateInternalService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService, nextButtonLabel: "Migrate")
        {
            SetTabLabel("Migrate Internal");
            var validationDialog = new MigrateInternalValidationDialog(this, Request);
            SubDialogs = SubDialogs.Union(new[] { validationDialog }).ToArray();
        }

        public bool LoadedFromConnection { get; set; }

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