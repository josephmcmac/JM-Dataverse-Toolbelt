using JosephM.Application.Application;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.OrganisationSettings
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