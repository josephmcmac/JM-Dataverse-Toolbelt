#region

using JosephM.Prism.Infrastructure.Module;

#endregion

namespace JosephM.Deployment.ImportCsvs
{
    public class ImportCsvsModule
        : ServiceRequestModule<ImportCsvsDialog, ImportCsvsService, ImportCsvsRequest, ImportCsvsResponse, ImportCsvsResponseItem>
    {
        public override string MenuGroup => "Deployment";
    }
}