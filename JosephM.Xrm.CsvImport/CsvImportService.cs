using JosephM.Core.Service;
using JosephM.Record.Csv;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.DataImportExport.MappedImport;
using System.Collections.Generic;

namespace JosephM.Xrm.CsvImport
{
    public class CsvImportService :
        ServiceBase<CsvImportRequest, CsvImportResponse, CsvImportResponseItem>
    {
        public XrmRecordService XrmRecordService { get; }

        public CsvImportService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public override void ExecuteExtention(CsvImportRequest request, CsvImportResponse response, ServiceRequestController controller)
        {
            controller.Controller.UpdateProgress(0, 1, "Loading Records For Import");
            var dictionary = LoadMappingDictionary(request);

            var importService = new MappedImportService(XrmRecordService);
            var responseItem = importService.DoImport(dictionary, request.MaskEmails, request.MatchByName, request.UpdateOnly, controller, useAmericanDates: request.DateFormat == DateFormat.American, executeMultipleSetSize: request.ExecuteMultipleSetSize, targetCacheLimit: request.TargetCacheLimit);
            response.Connection = XrmRecordService.XrmRecordConfiguration;
            response.LoadSpreadsheetImport(responseItem);
            response.Message = "The Import Process Has Completed";
        }

        public Dictionary<IMapSourceImport, IEnumerable<IRecord>> LoadMappingDictionary(CsvImportRequest request)
        {
            var dictionary = new Dictionary<IMapSourceImport, IEnumerable<IRecord>>();

            foreach (var csvMapping in request.CsvsToImport)
            {
                if (csvMapping.TargetType != null)
                {
                    var csvService = new CsvRecordService(csvMapping.SourceCsv);
                    var queryRows = csvService.RetrieveAll(csvMapping.SourceType.Key, null);
                    dictionary.Add(csvMapping, queryRows);
                }
            }

            return dictionary;
        }
    }
}