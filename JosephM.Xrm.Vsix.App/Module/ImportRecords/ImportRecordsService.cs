using JosephM.Core.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.DataImportExport.Import;
using JosephM.Xrm.DataImportExport.XmlExport;

namespace JosephM.Xrm.Vsix.Module.ImportRecords
{
    public class ImportRecordsService :
        ServiceBase<ImportRecordsRequest, ImportRecordsResponse, DataImportResponseItem>
    {
        public ImportRecordsService(IOrganizationConnectionFactory connectionFactory)
        {
            ConnectionFactory = connectionFactory;
        }

        private IOrganizationConnectionFactory ConnectionFactory { get; }

        public override void ExecuteExtention(ImportRecordsRequest request, ImportRecordsResponse response,
            ServiceRequestController controller)
        {
            //just use the method in ImportXmlService to do the import
            var xrmRecordService = new XrmRecordService(request.Connection, ConnectionFactory);
            var service = new ImportXmlService(xrmRecordService);
            var importXmlResponse = new ImportXmlResponse();
            service.ImportXml(request, controller, importXmlResponse, executeMultipleSetSize: 10, targetCacheLimit: 200);
            response.LoadDataImport(importXmlResponse);
        }
    }
}