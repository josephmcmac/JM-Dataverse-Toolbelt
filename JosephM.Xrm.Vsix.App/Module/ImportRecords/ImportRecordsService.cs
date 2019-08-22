﻿using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Deployment.DataImport;
using JosephM.Deployment.ImportXml;
using JosephM.Record.Xrm.XrmRecord;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.ImportRecords
{
    public class ImportRecordsService :
        ServiceBase<ImportRecordsRequest, ImportRecordsResponse, DataImportResponseItem>
    {
        public override void ExecuteExtention(ImportRecordsRequest request, ImportRecordsResponse response,
            ServiceRequestController controller)
        {
            //just use the method in ImportXmlService to do the import
            var xrmRecordService = new XrmRecordService(request.Connection);
            var service = new ImportXmlService(xrmRecordService);
            var importXmlResponse = new ImportXmlResponse();
            service.ImportXml(request, controller, importXmlResponse);
            response.LoadDataImport(importXmlResponse);
        }
    }
}