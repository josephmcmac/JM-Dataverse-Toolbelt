#region

using JosephM.Core.Attributes;
using JosephM.Prism.Infrastructure.Module;

#endregion

namespace JosephM.Deployment.ImportCsvs
{
    [MyDescription("Import Records Defined In CSV Files Into A CRM Instance")]
    public class ImportCsvsModule
        : ServiceRequestModule<ImportCsvsDialog, ImportCsvsService, ImportCsvsRequest, ImportCsvsResponse, ImportCsvsResponseItem>
    {
        public override string MenuGroup => "Deployment";
    }
}