using JosephM.Prism.Infrastructure.Module;

namespace JosephM.OrganisationSettings
{
    public class MaintainOrganisationModule : DialogModule<MaintainOrganisationDialog>
    {
        public override void InitialiseModule()
        {
            base.InitialiseModule();
            AddHelp("Update Excel Export Limit", "Organisation Settings Help.htm");
        }

        protected override string MainOperationName
        {
            get { return "Update Excel Export Limit"; }
        }
    }
}