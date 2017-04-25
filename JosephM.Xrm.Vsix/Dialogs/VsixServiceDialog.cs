using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.Log;
using JosephM.Core.Service;

namespace JosephM.XRM.VSIX.Dialogs
{
    public class VsixServiceDialog<TService, TRequest, TResponse, TResponseItem>
        : DialogViewModel
        where TService : ServiceBase<TRequest, TResponse, TResponseItem>
        where TRequest : ServiceRequestBase
        where TResponse : ServiceResponseBase<TResponseItem>, new()
        where TResponseItem : ServiceResponseItem
    {
        public TService Service { get; set; }
        public TRequest Request { get; set; }
        public TResponse Response { get; set; }

        public VsixServiceDialog(TService service, TRequest request, IDialogController dialogController)
            : base(dialogController)
        {
            Service = service;
            Request = request;
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            LoadingViewModel.IsLoading = false;
            IsProcessing = true;

            var progressControlViewModel = new ProgressControlViewModel(ApplicationController);
            Controller.LoadToUi(progressControlViewModel);
            var progressControlViewModelLevel2 = new ProgressControlViewModel(ApplicationController);
            Controller.LoadToUi(progressControlViewModelLevel2);
            var controller = new LogController(progressControlViewModel);
            controller.AddLevel2Ui(progressControlViewModelLevel2);

            Response = Service.Execute(Request, controller);

            PostExecute();

            Controller.RemoveFromUi(progressControlViewModel);
            Controller.RemoveFromUi(progressControlViewModelLevel2);

            foreach (var responseItem in Response.ResponseItems)
            {
                CompletionItems.Add(responseItem);
            }

            IsProcessing = false;

            if (!Response.Success)
                ProcessError(Response.Exception);
            else if (string.IsNullOrWhiteSpace(CompletionMessage))
                CompletionMessage = "Process Finished";
        }

        public virtual void PostExecute()
        {

        }
    }
}