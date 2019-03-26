using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Deployment.DataImport;
using JosephM.Deployment.ExportXml;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment.CreatePackage
{
    [RequiresConnection]
    public class CreatePackageDialog :
        ServiceRequestDialog
            <CreatePackageService, CreatePackageRequest,
                CreatePackageResponse, DataImportResponseItem>
    {
        public CreatePackageDialog(CreatePackageService service, IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            CompletionMessage = "The Deployment Package Has Been Generated" + (Request.DeployPackageInto == null ? "" : " And Deployed");
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
            addProperty("Managed Solution", Request.ExportAsManaged.ToString());
            addProperty("Include Deploy", (Request.DeployPackageInto != null).ToString());
            if (Request.DataToInclude != null)
            {
                foreach (var data in Request.DataToInclude)
                {
                    addProperty($"Include Data {data.RecordType?.Key}", data.Type.ToString());
                    addProperty($"Include Data All Fields {data.RecordType?.Key}", data.IncludeAllFields.ToString());
                    addProperty($"Include Data Include Inactive {data.RecordType?.Key}", data.IncludeInactive.ToString());
                    if (data.Type == ExportType.SpecificRecords)
                    {
                        addProperty($"Include Data Specific Record Count {data.RecordType?.Key}", data.SpecificRecordsToExport.Count().ToString());
                    }
                    if (!data.IncludeAllFields)
                    {
                        addProperty($"Include Data Specific Field Count {data.RecordType?.Key}", data.IncludeOnlyTheseFields.Count().ToString());
                    }
                }
            }
            return dictionary;
        }
    }
}