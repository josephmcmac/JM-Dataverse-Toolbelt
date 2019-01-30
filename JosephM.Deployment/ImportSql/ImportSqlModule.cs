using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;

namespace JosephM.Deployment.ImportSql
{
    [MyDescription("Import Records Defined In An Excel Sheet Into A CRM Instance")]
    public class ImportSqlModule
        : ServiceRequestModule<ImportSqlDialog, ImportSqlService, ImportSqlRequest, ImportSqlResponse, ImportSqlResponseItem>
    {
        public override string MenuGroup => "Data Import/Export";
    }
}