using JosephM.Application.Application;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Deployment.SpreadsheetImport;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Sql;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;

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
            var dictionary = LoadMappingDictionary(request);
            var importService = new SpreadsheetImportService(XrmRecordService);
            var responseItems = importService.DoImport(dictionary, request.MaskEmails, request.MatchRecordsByName, request.UpdateOnly, controller, executeMultipleSetSize: request.ExecuteMultipleSetSize, targetCacheLimit: request.TargetCacheLimit);
            response.Connection = XrmRecordService.XrmRecordConfiguration;
            response.LoadSpreadsheetImport(responseItems);
            response.Message = "The Import Process Has Completed";
        }

        public Dictionary<IMapSpreadsheetImport, IEnumerable<IRecord>> LoadMappingDictionary(ImportExcelRequest request)
        {
            var excelService = new ExcelRecordService(request.ExcelFile);

            var dictionary = new Dictionary<IMapSpreadsheetImport, IEnumerable<IRecord>>();

            foreach (var tabMapping in request.Mappings)
            {
                if (tabMapping.TargetType != null)
                {
                    var queryRows = excelService.RetrieveAll(tabMapping.SourceTab.Key, null);
                    dictionary.Add(tabMapping, queryRows);
                }
            }

            return dictionary;
        }
    }
}