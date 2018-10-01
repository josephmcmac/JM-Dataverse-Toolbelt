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

            var importService = new SpreadsheetImportService(XrmRecordService);
            var responseItem = importService.DoImport(dictionary, request.MaskEmails, request.MatchByName, controller, useAmericanDates: request.DateFormat == DateFormat.American);
            response.LoadSpreadsheetImport(responseItem);
        }
    }
}