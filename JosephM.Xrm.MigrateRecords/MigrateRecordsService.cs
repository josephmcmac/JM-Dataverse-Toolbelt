using JosephM.Core.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.DataImportExport.Import;
using JosephM.Xrm.DataImportExport.XmlImport;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.MigrateRecords
{
    public class MigrateRecordsService :
        ServiceBase<MigrateRecordsRequest, MigrateRecordsResponse, DataImportResponseItem>
    {
        public MigrateRecordsService(IOrganizationConnectionFactory connectionFactory)
        {
            ConnectionFactory = connectionFactory;
        }

        private IOrganizationConnectionFactory ConnectionFactory { get; }

        public override void ExecuteExtention(MigrateRecordsRequest request, MigrateRecordsResponse response,
            ServiceRequestController controller)
        {
            var exportService = new ExportXmlService(new XrmRecordService(request.SourceConnection, ConnectionFactory));

            var exportedEntities = new List<Entity>();

            exportService.ProcessExport(request.RecordTypesToMigrate, request.IncludeNotes, request.IncludeFileAndImageFields, request.IncludeNNRelationshipsBetweenEntities, controller.Controller
                , (entity) => exportedEntities.Add(entity)
                , (entity) => exportedEntities.Add(entity));

            var removeDuplicates = new List<Entity>();
            foreach(var entity in exportedEntities)
            {
                if(!removeDuplicates.Any(e => e.Id == entity.Id && e.LogicalName == entity.LogicalName))
                {
                    removeDuplicates.Add(entity);
                }
            }

            var importService = new DataImportService(new XrmRecordService(request.TargetConnection, ConnectionFactory));
            var matchOption = request.MatchByName ? MatchOption.PrimaryKeyThenName : MatchOption.PrimaryKeyOnly;
            var dataImportResponse = importService.DoImport(removeDuplicates, controller, request.MaskEmails, matchOption: matchOption, includeOwner: request.IncludeOwner, executeMultipleSetSize: request.ExecuteMultipleSetSize, targetCacheLimit: request.TargetCacheLimit);
            response.ConnectionMigratedInto = request.TargetConnection;
            response.LoadDataImport(dataImportResponse);
            response.Message = "The Record Migration Has Completed";
        }
    }
}