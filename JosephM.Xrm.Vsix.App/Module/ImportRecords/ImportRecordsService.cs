using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Deployment.DataImport;
using JosephM.Deployment.ImportXml;
using JosephM.Record.Xrm.XrmRecord;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.ImportRecords
{
    public class ImportRecordsService :
        ServiceBase<ImportRecordsRequest, ImportRecordsResponse, DataImportResponseItem>
    {
        public override void ExecuteExtention(ImportRecordsRequest request, ImportRecordsResponse response,
            LogController controller)
        {
            //just use the method in ImportXmlService to do the import
            var xrmRecordService = new XrmRecordService(request.Connection);
            var service = new ImportXmlService(xrmRecordService);
            var entities = service.LoadEntitiesFromXmlFiles(request.XmlFiles.Select(fr => fr.FileName).ToArray());

            var importResponses = service.DataImportService.DoImport(entities, controller, false);
            response.AddResponseItems(importResponses);
        }
    }
}