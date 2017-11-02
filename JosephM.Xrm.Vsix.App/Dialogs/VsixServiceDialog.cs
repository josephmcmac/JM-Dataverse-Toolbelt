using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.IService;

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

        public VsixServiceDialog(TService service, TRequest request, IDialogController dialogController, bool showRequestEntryForm = false, IRecordService lookupService = null)
            : base(dialogController)
        {
            Service = service;
            Request = request;
            if(showRequestEntryForm)
            {
                var configEntryDialog = new ObjectEntryDialog(Request, this, ApplicationController, lookupService, null);
                SubDialogs = new DialogViewModel[] { configEntryDialog };
            }
        }

        protected override void LoadDialogExtention()
        {
            StartNextAction();
        }

        protected override void CompleteDialogExtention()
        {
            //todo refactor this to use the same dialog as app
            //so runs same dialog completion

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

            CompletionItem = Response;

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