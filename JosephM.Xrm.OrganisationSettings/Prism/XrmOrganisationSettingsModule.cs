using JosephM.Prism.Infrastructure.Module;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.OrganisationSettings.Infrastucture;

namespace JosephM.Xrm.OrganisationSettings.Prism
{
    public class XrmOrganisationSettingsModule :
        MaintainRecordModule<XrmOrganisationFormService, XrmOrganisationMaintainViewModel>
    {
        protected override string Type
        {
            get { return "organization"; }
        }

        protected override string IdName
        {
            get { return "name"; }
        }

        protected override string Id
        {
            get
            {
                var config = Container.Resolve<IXrmRecordConfiguration>();
                return config.OrganizationUniqueName;
            }
        }

        public override void InitialiseModule()
        {
            base.InitialiseModule();
            ApplicationOptions.AddHelp("Update Excel Export Limit", "Organisation Settings Help.htm");
        }
    }
}