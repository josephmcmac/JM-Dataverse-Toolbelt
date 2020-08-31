using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Deployment.SpreadsheetImport;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Sql;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment.ImportExcel
{
    public class ImportExcelService :
        ServiceBase<ImportExcelRequest, ImportExcelResponse, ImportExcelResponseItem>
    {
        public ImportExcelService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public XrmRecordService XrmRecordService { get; }

        public override void ExecuteExtention(ImportExcelRequest request, ImportExcelResponse response,
            ServiceRequestController controller)
        {
            controller.Controller.UpdateProgress(0, 1, "Loading Records For Import");
            var dictionary = LoadMappingDictionary(request, controller.Controller);
            var importService = new SpreadsheetImportService(XrmRecordService);
            var responseItems = importService.DoImport(dictionary, request.MaskEmails, request.MatchRecordsByName, request.UpdateOnly, controller, executeMultipleSetSize: request.ExecuteMultipleSetSize, targetCacheLimit: request.TargetCacheLimit);
            response.Connection = XrmRecordService.XrmRecordConfiguration;
            response.LoadSpreadsheetImport(responseItems);
            response.Message = "The Import Process Has Completed";
        }

        public Dictionary<IMapSpreadsheetImport, IEnumerable<IRecord>> LoadMappingDictionary(ImportExcelRequest request, LogController logController)
        {
            var excelService = new ExcelRecordService(request.ExcelFile);

            var dictionary = new Dictionary<IMapSpreadsheetImport, IEnumerable<IRecord>>();

            var toIterate = request.Mappings.Where(tm => tm.TargetType != null).ToArray();
            var countToDo = toIterate.Count();
            var countDone = 0;
            foreach (var tabMapping in toIterate)
            {
                logController.LogLiteral($"Reading Sheet {++countDone}/{countToDo} {tabMapping.SourceTab.Key}");
                var queryRows = excelService.RetrieveAll(tabMapping.SourceTab.Key, null);
                dictionary.Add(tabMapping, queryRows);
            }

            return dictionary;
        }
    }
}