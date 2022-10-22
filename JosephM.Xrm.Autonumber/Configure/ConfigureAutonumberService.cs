using JosephM.Core.Service;
using JosephM.Record.Xrm.XrmRecord;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Threading;

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
            controller.UpdateProgress(1, 5, "Loading Field metadata");
            var stringFieldMetadata = xrmService.GetFieldMetadata(fieldName, recordType) as StringAttributeMetadata;
            if (stringFieldMetadata == null)
                throw new Exception($"Field {fieldName} In {recordType} Is Not Of Type {nameof(StringAttributeMetadata)}");

            //apparently if null doesnt apply the clear so lets set as empty string if null
            request.Format = request.Format ?? string.Empty;

            if (stringFieldMetadata.AutoNumberFormat != request.Format)
            {
                //okay I have noted that the autonumber does not seem to always apply immediatelly
                //that is when querying the metadata after setting the autonumber config
                //sometimes it does not return the applied value
                //so lets do wait/retry

                var numberOfSecondsToCheckUpdated = 4;
                var retryAfterNumberOfChecks = 3;
                var numberOfRetriesLeft = 3;

                var isUpdated = false;
                while (true)
                {
                    controller.UpdateProgress(2, 5, "Setting Format");
                    stringFieldMetadata.AutoNumberFormat = request.Format;
                    xrmService.CreateOrUpdateAttribute(fieldName, recordType, stringFieldMetadata);
                    controller.UpdateProgress(3, 5, "Publishing");
                    var publishXml = $"<importexportxml><entities><entity>{recordType}</entity></entities></importexportxml>";
                    xrmService.Publish(publishXml);

                    for(var i = 0; i < retryAfterNumberOfChecks; i++)
                    {
                        var getAttributeResponse = (RetrieveAttributeResponse)xrmService.Execute(new RetrieveAttributeRequest()
                        {
                            EntityLogicalName = request.RecordType.Key,
                            LogicalName = request.Field.Key
                        });
                        var stringAttributeMetadata = getAttributeResponse.AttributeMetadata as StringAttributeMetadata;
                        if (stringAttributeMetadata.AutoNumberFormat == request.Format)
                        {
                            isUpdated = true;
                            response.FormatUpdated = true;
                            xrmService.SetFieldMetadataCache(request.RecordType.Key, request.Field.Key, stringAttributeMetadata);
                            break;
                        }

                        Thread.Sleep(numberOfSecondsToCheckUpdated * 1000);
                    }
                    if (isUpdated)
                        break;

                    numberOfRetriesLeft--;
                    if(numberOfRetriesLeft == 0)
                    {
                        throw new Exception("The Autonumber Configuration May Not be Applied. The New Format Was Sent To The Web Service Several Times But Dynamics Has Not Applied The Settings According To The Metadata Provided By The Web Service. Perhaps Retry");
                    }
                }
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
                response.SeedUpdated = true;
            }

            controller.UpdateProgress(5, 5, "Finishing");
        }
    }
}