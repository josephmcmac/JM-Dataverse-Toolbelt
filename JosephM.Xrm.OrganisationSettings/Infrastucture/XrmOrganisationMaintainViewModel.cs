using System;
using JosephM.Core.Attributes;
using JosephM.Record.Application.RecordEntry.Form;

namespace JosephM.Xrm.OrganisationSettings.Infrastucture
{
    [DisplayName("Update Excel Export Limit")]
    public class XrmOrganisationMaintainViewModel : MaintainViewModel
    {
        public XrmOrganisationMaintainViewModel(XrmOrganisationFormController formController)
            : base(formController)
        {
        }

        protected override void LoadRecord()
        {
            //gets and sets the only organisation record from the crm connection
            var organisationRecord = RecordService.GetFirst(RecordType);
            if (organisationRecord == null)
                throw new Exception("Error Retreiving Organisation Record");
            SetRecord(organisationRecord);
        }
    }
}