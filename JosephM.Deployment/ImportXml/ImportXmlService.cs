using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Deployment.DataImport;
using JosephM.Record.Xrm.XrmRecord;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            ImportXml(request.Folder.FolderPath, controller, response, maskEmails: request.MaskEmails, includeOwner: request.IncludeOwner, matchByName: request.MatchByName);
        }


        public void ImportXml(string folder, ServiceRequestController controller,
            ImportXmlResponse response, bool maskEmails = false, bool includeOwner = false, bool matchByName = true)
        {
            controller.UpdateProgress(0, 1, "Loading XML Files");
            var entities = LoadEntitiesFromXmlFiles(folder, controller.Controller);
            var matchOption = matchByName ? DataImportService.MatchOption.PrimaryKeyThenName : DataImportService.MatchOption.PrimaryKeyOnly;
            var importResponse = DataImportService.DoImport(entities, controller, maskEmails, matchOption: matchOption, includeOwner: includeOwner);
            response.LoadDataImport(importResponse);
        }

        public IEnumerable<Entity> LoadEntitiesFromXmlFiles(string folder, LogController controller = null)
        {
            var filesToImport = Directory.GetFiles(folder, "*.xml");
            return LoadEntitiesFromXmlFiles(filesToImport, controller);
        }

        public IEnumerable<Entity> LoadEntitiesFromXmlFiles(IEnumerable<string> filesToImport, LogController controller = null)
        {
            var lateBoundSerializer = new DataContractSerializer(typeof(Entity));
            var entities = new List<Entity>();
            var done = 0;
            var toDo = filesToImport.Count();
            foreach (var file in filesToImport)
            {
                using (var fileStream = new FileStream(file, FileMode.Open))
                {
                    entities.Add((Entity)lateBoundSerializer.ReadObject(fileStream));
                }
                done++;
                if (controller != null)
                    controller.LogLiteral($"Loading Xml Files {done}/{toDo}");
            }
            return entities;
        }
    }
}