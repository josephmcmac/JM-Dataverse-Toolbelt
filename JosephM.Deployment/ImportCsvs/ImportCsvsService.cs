using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Deployment.DataImport;
using JosephM.Deployment.SpreadsheetImport;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Sql;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;

namespace JosephM.Deployment.ImportCsvs
{
    public class ImportCsvsService :
        ServiceBase<ImportCsvsRequest, ImportCsvsResponse, ImportCsvsResponseItem>
    {
        public XrmRecordService XrmRecordService { get; }

        public ImportCsvsService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public override void ExecuteExtention(ImportCsvsRequest request, ImportCsvsResponse response, ServiceRequestController controller)
        {
            controller.Controller.UpdateProgress(0, 1, "Loading Records For Import");
            var dictionary = LoadMappingDictionary(request);

            var importService = new SpreadsheetImportService(XrmRecordService);
            var responseItem = importService.DoImport(dictionary, request.MaskEmails, request.MatchByName, request.UpdateOnly, controller, useAmericanDates: request.DateFormat == DateFormat.American);
            response.Connection = XrmRecordService.XrmRecordConfiguration;
            response.LoadSpreadsheetImport(responseItem);
            response.Message = "The Import Process Has Completed";
        }

        public Dictionary<IMapSpreadsheetImport, IEnumerable<IRecord>> LoadMappingDictionary(ImportCsvsRequest request)
        {
            var dictionary = new Dictionary<IMapSpreadsheetImport, IEnumerable<IRecord>>();

            foreach (var csvMapping in request.CsvsToImport)
            {
                if (csvMapping.TargetType != null)
                {
                    var excelService = new CsvRecordService(csvMapping.SourceCsv);
                    var queryRows = excelService.RetrieveAll(csvMapping.SourceType.Key, null);
                    dictionary.Add(csvMapping, queryRows);
                }
            }

            return dictionary;
        }
    }
}