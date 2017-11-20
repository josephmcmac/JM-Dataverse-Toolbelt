#region

using JosephM.Prism.Infrastructure.Module;

#endregion

namespace JosephM.Deployment.MigrateRecords
{
    public class MigrateRecordsModule
        : ServiceRequestModule<MigrateRecordsDialog, MigrateRecordsService, MigrateRecordsRequest, MigrateRecordsResponse, DataImportResponseItem>
    {
        public override string MenuGroup => "Deployment";
    }
}