using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.Excel;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.DataImportExport.MappedImport;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.ExcelImport
{
    public class ExcelImportService :
        ServiceBase<ExcelImportRequest, ExcelImportResponse, ExcelImportResponseItem>
    {
        public ExcelImportService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public XrmRecordService XrmRecordService { get; }

        public override void ExecuteExtention(ExcelImportRequest request, ExcelImportResponse response,
            ServiceRequestController controller)
        {
            controller.Controller.UpdateProgress(0, 1, "Loading Records For Import");
            var dictionary = LoadMappingDictionary(request, controller.Controller);
            var importService = new MappedImportService(XrmRecordService);
            var responseItems = importService.DoImport(dictionary, request.MaskEmails, request.MatchRecordsByName, request.UpdateOnly, controller, executeMultipleSetSize: request.ExecuteMultipleSetSize, targetCacheLimit: request.TargetCacheLimit, ignoreNullValues: request.IgnoreEmptyCells, onlyFieldMatchActive: request.OnlyFieldMatchActive, parallelImportProcessCount: request.ParallelImportProcessCount ?? 1, bypassWorkflowsAndPlugins: request.BypassFlowsPluginsAndWorkflows);
            response.Connection = XrmRecordService.XrmRecordConfiguration;
            response.LoadSpreadsheetImport(responseItems);
            response.Message = "The Import Process Has Completed";
        }

        public Dictionary<IMapSourceImport, IEnumerable<IRecord>> LoadMappingDictionary(ExcelImportRequest request, LogController logController)
        {
            var excelService = new ExcelRecordService(request.ExcelFile);

            var dictionary = new Dictionary<IMapSourceImport, IEnumerable<IRecord>>();

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