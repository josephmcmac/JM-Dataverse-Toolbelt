using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;

namespace JosephM.TestModule.AllPropertyTypesCentered
{
    public class AllPropertyTypesCenteredDialog :
        ServiceRequestDialog<AllPropertyTypesCenteredService, AllPropertyTypesCenteredRequest, AllPropertyTypesCenteredResponse, AllPropertyTypesCenteredResponseItem>
    {
        public AllPropertyTypesCenteredDialog(IDialogController dialogController)
            : base(new AllPropertyTypesCenteredService(), dialogController, FakeRecordService.Get())
        {
            
        }
    }
}