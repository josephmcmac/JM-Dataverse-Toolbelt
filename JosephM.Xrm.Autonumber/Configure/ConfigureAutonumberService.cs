using JosephM.Core.Service;
using JosephM.Record.Xrm.XrmRecord;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;

namespace JosephM.Application.Desktop.Module.Crud.ConfigureAutonumber
{
    public class ConfigureAutonumberService :
        ServiceBase<ConfigureAutonumberRequest, ConfigureAutonumberResponse, ConfigureAutonumberResponseItem>
    {
        public XrmRecordService XrmRecordService { get; set; }
        public ConfigureAutonumberService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public override void ExecuteExtention(ConfigureAutonumberRequest request, ConfigureAutonumberResponse response,
            ServiceRequestController controller)
        {
            controller.UpdateProgress(0, 5, "Processing Field Update");
            //okay we need to update the autonumber
            var fieldName = request.Field?.Key;
            var recordType = request.RecordType?.Key;
            var xrmService = XrmRecordService.XrmService;
            xrmService.ClearFieldMetadataCache(recordType);
            controller.UpdateProgress(1, 5, "Loading Field metadata");
            var stringFieldMetadata = xrmService.GetFieldMetadata(fieldName, recordType) as StringAttributeMetadata;
            if (stringFieldMetadata == null)
                throw new Exception($"Field {fieldName} In {recordType} Is Not Of Type {nameof(StringAttributeMetadata)}");

            if (stringFieldMetadata.AutoNumberFormat != request.AutonumberFormat)
            {
                controller.UpdateProgress(2, 5, "Setting Format");
                stringFieldMetadata.AutoNumberFormat = request.AutonumberFormat;
                xrmService.CreateOrUpdateAttribute(fieldName, recordType, stringFieldMetadata);
                controller.UpdateProgress(3, 5, "Publishing");
                var publishXml = $"<importexportxml><entities><entity>{recordType}</entity></entities></importexportxml>";
                xrmService.Publish(publishXml);
                xrmService.ClearFieldMetadataCache(recordType);
            }

            if (request.SetSeed.HasValue)
            {
                controller.UpdateProgress(4, 5, "Updating Seed Field Update");
                var req = new SetAutoNumberSeedRequest
                {
                    AttributeName = fieldName,
                    EntityName = recordType,
                    Value = request.SetSeed.Value
                };
                xrmService.Execute(req);
            }
            controller.UpdateProgress(5, 5, "Finishing");
        }
    }
}