using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.Shared;

namespace JosephM.AllPropertyTypesModule.AllPropertyTypesDialog
{
    public class AllPropertyTypesDialog :
        ServiceRequestDialog<AllPropertyTypesService, AllPropertyTypesRequest, AllPropertyTypesResponse, AllPropertyTypesResponseItem>
    {
        public AllPropertyTypesDialog(IDialogController dialogController)
            : base(new AllPropertyTypesService(), dialogController, FakeRecordService.Get())
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