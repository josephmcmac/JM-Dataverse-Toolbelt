using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.UserSavedObjectsUtility.Dashboards
{
    [RequiresConnection]
    public class UserSavedDashboardsUtilityDialog :
        ServiceRequestDialog
            <UserSavedDashboardsUtilityService, UserSavedDashboardsUtilityRequest, UserSavedDashboardsUtilityResponse,
                UserSavedDashboardsUtilityResponseItem>
    {
        public UserSavedDashboardsUtilityDialog(UserSavedDashboardsUtilityService service, IDialogController dialogController,
            XrmRecordService recordService)
            : base(service, dialogController, recordService)
        {
        }

        protected override bool UseProgressControlUi => true;
    }
}