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

        protected override void CompleteDialogExtention()
        {

            base.CompleteDialogExtention();

            CompletionOptions.Add(new XrmButtonViewModel("Option 1", () => ApplicationController.UserMessage("Dummy"), ApplicationController));
            CompletionOptions.Add(new XrmButtonViewModel("option 2", () => ApplicationController.UserMessage("Dummy 2"), ApplicationController));
        }
    }
}