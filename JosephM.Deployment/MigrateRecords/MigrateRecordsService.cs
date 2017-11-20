#region

using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment.ExportXml;
using JosephM.Deployment.ImportXml;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
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

namespace JosephM.Deployment.MigrateRecords
{
    public class MigrateRecordsService :
        ServiceBase<MigrateRecordsRequest, MigrateRecordsResponse, DataImportResponseItem>
    {
        public MigrateRecordsService()
        {
        }

        public override void ExecuteExtention(MigrateRecordsRequest request, MigrateRecordsResponse response,
            LogController controller)
        {
            var exportService = new ExportXmlService(new XrmRecordService(request.SourceConnection));

            var exportedEntities = new List<Entity>();

            exportService.ProcessExport(request.RecordTypesToMigrate, request.IncludeNotes, request.IncludeNNRelationshipsBetweenEntities, controller
                , (entity) => exportedEntities.Add(entity)
                , (entity) => exportedEntities.Add(entity));

            var importService = new ImportXmlService(new XrmRecordService(request.TargetConnection));
            importService.DoImport(exportedEntities, controller, request.MaskEmails);
        }
    }
}