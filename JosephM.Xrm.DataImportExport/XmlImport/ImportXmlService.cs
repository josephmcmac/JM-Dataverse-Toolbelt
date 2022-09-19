using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.DataImportExport.Import;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace JosephM.Xrm.DataImportExport.XmlExport
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
            ImportXml(request, controller, response, maskEmails: request.MaskEmails, includeOwner: request.IncludeOwner, matchByName: request.MatchByName, executeMultipleSetSize: request.ExecuteMultipleSetSize, targetCacheLimit: request.TargetCacheLimit);
        }


        public void ImportXml(IImportXmlRequest request, ServiceRequestController controller,
            ImportXmlResponse response, bool maskEmails = false, bool includeOwner = false, bool matchByName = true, int? executeMultipleSetSize = null, int? targetCacheLimit = null)
        {
            controller.UpdateProgress(0, 1, "Loading XML Files");
            var entities = request.GetOrLoadEntitiesForImport(controller.Controller).Values.ToArray();
            var matchOption = matchByName ? MatchOption.PrimaryKeyThenName : MatchOption.PrimaryKeyOnly;
            var importResponse = DataImportService.DoImport(entities, controller, maskEmails, matchOption: matchOption, includeOwner: includeOwner, executeMultipleSetSize: executeMultipleSetSize, targetCacheLimit: targetCacheLimit);
            response.Connection = XrmRecordService.XrmRecordConfiguration;
            response.LoadDataImport(importResponse);
            response.Message = "The Import Process Has Completed";
        }

        public static IDictionary<string, Entity> LoadEntitiesFromXmlFiles(string folder, LogController controller = null)
        {
            var filesToImport = Directory.GetFiles(folder, "*.xml");
            return LoadEntitiesFromXmlFiles(filesToImport, controller);
        }

        public static IDictionary<string, Entity> LoadEntitiesFromXmlFiles(IEnumerable<string> filesToImport, LogController controller = null)
        {
            var lateBoundSerializer = new DataContractSerializer(typeof(Entity));
            var entities = new Dictionary<string, Entity>();
            var done = 0;
            var toDo = filesToImport.Count();
            foreach (var file in filesToImport)
            {
                using (var fileStream = new FileStream(file, FileMode.Open))
                {
                    entities.Add(file, (Entity)lateBoundSerializer.ReadObject(fileStream));
                }
                done++;
                if (controller != null)
                    controller.LogLiteral($"Loading Xml Files {done}/{toDo}");
            }
            return entities;
        }
    }
}