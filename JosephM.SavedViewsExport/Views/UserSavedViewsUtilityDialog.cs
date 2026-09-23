using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.UserSavedObjectsUtility.Views
{
    [RequiresConnection]
    public class UserSavedViewsUtilityDialog :
        ServiceRequestDialog
            <UserSavedViewsUtilityService, UserSavedViewsUtilityRequest, UserSavedViewsUtilityResponse,
                UserSavedViewsUtilityResponseItem>
    {
        public UserSavedViewsUtilityDialog(UserSavedViewsUtilityService service, IDialogController dialogController,
            XrmRecordService recordService)
            : base(service, dialogController, recordService)
        {
        }

        protected override bool UseProgressControlUi => true;
    }
}