using JosephM.Prism.Infrastructure.Module;

namespace JosephM.OrganisationSettings
{
    public class MaintainOrganisationModule : DialogModule<MaintainOrganisationDialog>
    {
        protected override string MainOperationName
        {
            get { return "Update Excel Export Limit"; }
        }
    }
}