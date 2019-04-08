using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.Shared;

namespace JosephM.TestModule.AllPropertyTypesModule
{
    public class AllPropertyTypesDialog :
        ServiceRequestDialog<AllPropertyTypesService, AllPropertyTypesRequest, AllPropertyTypesResponse, AllPropertyTypesResponseItem>
    {
        public AllPropertyTypesDialog(IDialogController dialogController)
            : base(new AllPropertyTypesService(), dialogController, FakeRecordService.Get())
        {
            
        }
    }
}