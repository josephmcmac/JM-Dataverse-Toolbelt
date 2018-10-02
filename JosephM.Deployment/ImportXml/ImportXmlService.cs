using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Deployment.DataImport;
using JosephM.Record.Xrm.XrmRecord;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace JosephM.Deployment.ImportXml
{
    public class ImportXmlService :
        ServiceBase<ImportXmlRequest, ImportXmlResponse, DataImportResponseItem>
    {
        public ImportXmlService(XrmRecordService xrmRecordService)
        {
            DataImportService = new DataImportService(xrmRecordService);
            XrmRecordService = xrmRecordService;
        }

        public DataImportService DataImportService { get; }
        public XrmRecordService XrmRecordService { get; }

        public override void ExecuteExtention(ImportXmlRequest request, ImportXmlResponse response,
            ServiceRequestController controller)
        {
            ImportXml(request.Folder.FolderPath, controller, response, request.MaskEmails);
        }


        public void ImportXml(string folder, ServiceRequestController controller,
            ImportXmlResponse response, bool maskEmails = false)
        {
            controller.UpdateProgress(0, 1, "Loading XML Files");
            var entities = LoadEntitiesFromXmlFiles(folder);

            var importResponse = DataImportService.DoImport(entities, controller, maskEmails);
            response.LoadDataImport(importResponse);
        }

        public IEnumerable<Entity> LoadEntitiesFromXmlFiles(string folder)
        {
            var filesToImport = Directory.GetFiles(folder, "*.xml");
            return LoadEntitiesFromXmlFiles(filesToImport);
        }

        public IEnumerable<Entity> LoadEntitiesFromXmlFiles(IEnumerable<string> filesToImport)
        {
            var lateBoundSerializer = new DataContractSerializer(typeof(Entity));
            var entities = new List<Entity>();
            foreach (var file in filesToImport)
            {
                using (var fileStream = new FileStream(file, FileMode.Open))
                {
                    entities.Add((Entity)lateBoundSerializer.ReadObject(fileStream));
                }
            }
            return entities;
        }
    }
}