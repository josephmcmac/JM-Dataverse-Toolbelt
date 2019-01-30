using JosephM.Core.Service;
using JosephM.Deployment.SpreadsheetImport;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Sql;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;

namespace JosephM.Deployment.ImportSql
{
    public class ImportSqlService :
        ServiceBase<ImportSqlRequest, ImportSqlResponse, ImportSqlResponseItem>
    {
        public ImportSqlService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public XrmRecordService XrmRecordService { get; }

        public override void ExecuteExtention(ImportSqlRequest request, ImportSqlResponse response,
            ServiceRequestController controller)
        {
            controller.Controller.UpdateProgress(0, 1, "Loading Records For Import");
            var dictionary = LoadMappingDictionary(request);
            var importService = new SpreadsheetImportService(XrmRecordService);
            var responseItems = importService.DoImport(dictionary, request.MaskEmails, request.MatchRecordsByName, request.UpdateOnly, controller);
            response.LoadSpreadsheetImport(responseItems);
        }

        public Dictionary<IMapSpreadsheetImport, IEnumerable<IRecord>> LoadMappingDictionary(ImportSqlRequest request)
        {
            var excelService = new SqlRecordService(new SqlConnectionString(request.ConnectionString));

            var dictionary = new Dictionary<IMapSpreadsheetImport, IEnumerable<IRecord>>();

            foreach (var tabMapping in request.Mappings)
            {
                if (tabMapping.TargetType != null)
                {
                    var queryRows = excelService.RetrieveAll(tabMapping.SourceTable.Key, null);
                    dictionary.Add(tabMapping, queryRows);
                }
            }

            return dictionary;
        }
    }
}