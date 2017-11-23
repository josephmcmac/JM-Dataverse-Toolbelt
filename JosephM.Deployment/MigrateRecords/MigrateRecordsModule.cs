#region

using JosephM.Core.Attributes;
using JosephM.Prism.Infrastructure.Module;

#endregion

namespace JosephM.Deployment.MigrateRecords
{
    [MyDescription("Migrate A Set Of Records From One CRM Instance Into Another")]
    public class MigrateRecordsModule
        : ServiceRequestModule<MigrateRecordsDialog, MigrateRecordsService, MigrateRecordsRequest, MigrateRecordsResponse, DataImportResponseItem>
    {
        public override string MenuGroup => "Deployment";
    }
}