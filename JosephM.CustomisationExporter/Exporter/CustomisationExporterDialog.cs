using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.CustomisationExporter.Exporter
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

        public bool LoadedFromConnection { get; set; }

        protected override IDictionary<string, string> GetPropertiesForCompletedLog()
        {
            var dictionary = base.GetPropertiesForCompletedLog();
            void addProperty(string name, string value)
            {
                if (!dictionary.ContainsKey(name))
                    dictionary.Add(name, value);
            }
            addProperty("All Types", Request.IncludeAllRecordTypes.ToString());
            if(!Request.IncludeAllRecordTypes && Request.RecordTypes != null)
                addProperty("Selected Type Count", Request.RecordTypes.Count().ToString());
            addProperty("Entities", Request.Entities.ToString());
            addProperty("Fields", Request.Fields.ToString());
            addProperty("Relationships", Request.Relationships.ToString());
            addProperty("Field Option Sets", Request.FieldOptionSets.ToString());
            addProperty("Shared Option Sets", Request.SharedOptionSets.ToString());
            return dictionary;
        }
    }
}