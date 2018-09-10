#region

using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment.SpreadsheetImport;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Sql;
using JosephM.Record.Xrm;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

#endregion

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
            LogController controller)
        {
            var excelService = new ExcelRecordService(request.ExcelFile);

            var dictionary = new Dictionary<IMapSpreadsheetImport, IEnumerable<IRecord>>();

            foreach (var tabMapping in request.Mappings)
            {
                if(tabMapping.TargetType != null)
                {
                    var queryRows = excelService.RetrieveAll(tabMapping.SourceTab.Key, null);
                    dictionary.Add(tabMapping, queryRows);
                }
            }

            var importService = new SpreadsheetImportService(XrmRecordService);
            var responseItems = importService.DoImport(dictionary, request.MaskEmails, request.MatchRecordsByName, controller);

            foreach (var item in responseItems)
                response.AddResponseItem(new ImportExcelResponseItem(item));
        }
    }
}