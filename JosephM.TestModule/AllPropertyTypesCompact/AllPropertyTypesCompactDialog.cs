using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.Shared;

namespace JosephM.TestModule.AllPropertyTypesCompact
{
    public class AllPropertyTypesCompactDialog :
        ServiceRequestDialog<AllPropertyTypesCompactService, AllPropertyTypesCompactRequest, AllPropertyTypesCompactResponse, AllPropertyTypesCompactResponseItem>
    {
        public AllPropertyTypesCompactDialog(IDialogController dialogController)
            : base(new AllPropertyTypesCompactService(), dialogController, FakeRecordService.Get())
        {
            
        }
    }
}