using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.UserSavedObjectsUtility.Charts
{
    [RequiresConnection]
    public class UserSavedChartsUtilityDialog :
        ServiceRequestDialog
            <UserSavedChartsUtilityService, UserSavedChartsUtilityRequest, UserSavedChartsUtilityResponse,
                UserSavedChartsUtilityResponseItem>
    {
        public UserSavedChartsUtilityDialog(UserSavedChartsUtilityService service, IDialogController dialogController,
            XrmRecordService recordService)
            : base(service, dialogController, recordService)
        {
        }

        protected override bool UseProgressControlUi => true;
    }
}