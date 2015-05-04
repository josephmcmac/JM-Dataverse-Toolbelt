using JosephM.Record.Application.Controller;
using JosephM.Record.Application.RecordEntry;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Xrm.OrganisationSettings.Infrastucture
{
    public class XrmOrganisationFormController : FormController
    {
        public XrmOrganisationFormController(XrmRecordService recordService, XrmOrganisationFormService formService,
            IApplicationController applicationController)
            : base(recordService, formService, applicationController)
        {
        }
    }
}