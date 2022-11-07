using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Attributes;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.CustomisationExporter
{
    [RequiresConnection(escapeConnectionCheckProperty: nameof(LoadedFromConnection))]
    public class CustomisationExporterDialog :
        ServiceRequestDialog
            <CustomisationExporterService, CustomisationExporterRequest, CustomisationExporterResponse,
                CustomisationExporterResponseItem>
    {
        public CustomisationExporterDialog(CustomisationExporterService service, IDialogController dialogController,
            XrmRecordService recordService)
            : base(service, dialogController, recordService)
        {
        }

        protected override bool UseProgressControlUi => true;

        public bool LoadedFromConnection { get; set; }

        protected override IDictionary<string, string> GetPropertiesForCompletedLog()
        {
            var dictionary = base.GetPropertiesForCompletedLog();
            void addProperty(string name, string value)
            {
                if (!dictionary.ContainsKey(name))
                    dictionary.Add(name, value);
            }
            addProperty("Entities", Request.Entities.ToString());
            addProperty("Fields", Request.Fields.ToString());
            addProperty("Relationships", Request.Relationships.ToString());
            addProperty("Field Option Sets", Request.FieldOptionSets.ToString());
            addProperty("Shared Option Sets", Request.SharedOptionSets.ToString());
            return dictionary;
        }
    }
}