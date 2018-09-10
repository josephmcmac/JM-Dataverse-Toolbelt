using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;
using JosephM.Deployment.DataImport;

namespace JosephM.Deployment.MigrateRecords
{
    [MyDescription("Migrate A Set Of Records From One CRM Instance Into Another")]
    public class MigrateRecordsModule
        : ServiceRequestModule<MigrateRecordsDialog, MigrateRecordsService, MigrateRecordsRequest, MigrateRecordsResponse, DataImportResponseItem>
    {
        public override string MenuGroup => "Data Import/Export";
    }
}