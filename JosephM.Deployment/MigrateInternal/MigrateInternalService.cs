using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Deployment.SpreadsheetImport;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Sql;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment.MigrateInternal
{
    public class MigrateInternalService :
        ServiceBase<MigrateInternalRequest, MigrateInternalResponse, MigrateInternalResponseItem>
    {
        public MigrateInternalService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public XrmRecordService XrmRecordService { get; }

        public override void ExecuteExtention(MigrateInternalRequest request, MigrateInternalResponse response,
            ServiceRequestController controller)
        {
            controller.Controller.UpdateProgress(0, 1, "Loading Records For Import");
            var dictionary = LoadMappingDictionary(request, controller.Controller);
            var importService = new SourceImportService(XrmRecordService);
            var responseItems = importService.DoImport(dictionary, false, request.MatchRecordsByName, false, controller, executeMultipleSetSize: request.ExecuteMultipleSetSize, targetCacheLimit: request.TargetCacheLimit);
            response.LoadSpreadsheetImport(responseItems);
            response.Message = "The Import Process Has Completed";
        }

        public Dictionary<IMapSourceImport, IEnumerable<IRecord>> LoadMappingDictionary(MigrateInternalRequest request, LogController logController)
        {
            var dictionary = new Dictionary<IMapSourceImport, IEnumerable<IRecord>>();

            var toIterate = request.Mappings.Where(tm => tm.TargetType != null).ToArray();
            var countToDo = toIterate.Count();
            var countDone = 0;
            foreach (var sourceMapping in toIterate)
            {
                logController.LogLiteral($"Reading Sheet {++countDone}/{countToDo} {sourceMapping.SourceType.Key}");
                var queryRows = XrmRecordService.RetrieveAll(sourceMapping.SourceType.Key, null);
                dictionary.Add(sourceMapping, queryRows);
            }

            return dictionary;
        }
    }
}