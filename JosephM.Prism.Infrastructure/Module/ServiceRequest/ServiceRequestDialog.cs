using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.ObjectMapping;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JosephM.Application.Prism.Module.ServiceRequest
{
    /// <summary>
    ///     Base Class implementing A Dialog Which Executes The Main Method Provided By A Service
    ///     First Has A Dialog To Populate The Request Object Required By The Service
    ///     Then Executes The Service Method And Displays The Completion Page With A Summary
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TResponseItem"></typeparam>
    public class ServiceRequestDialog<TService, TRequest, TResponse, TResponseItem> : DialogViewModel
        where TRequest : ServiceRequestBase, new()
        where TResponseItem : ServiceResponseItem
        where TResponse : ServiceResponseBase<TResponseItem>, new()
        where TService : ServiceBase<TRequest, TResponse, TResponseItem>
    {
        public ServiceRequestDialog(TService service, IDialogController dialogController)
            : this(service, dialogController, null, null)
        {
        }

        public ServiceRequestDialog(TService service, IDialogController dialogController, IRecordService lookupService, TRequest request = null, Action onClose = null)
            : base(dialogController)
        {
            if (onClose != null)
                OnCancel = onClose;

            Service = service;
            
            if (request != null)
                Request = request;
            else
                Request = ApplicationController.ResolveType<TRequest>();

            ConfigEntryDialog = new ObjectEntryDialog(Request, this, ApplicationController, lookupService, null, null, onClose);
            SubDialogs = new DialogViewModel[] { ConfigEntryDialog };
        }

        public bool SkipObjectEntry { get; set; }

        protected TService Service { get; set; }

        public TRequest Request { get; set; }

        protected TResponse Response { get; set; }

        protected override void LoadDialogExtention()
        {
            var savedRequests = ApplicationController
                .ResolveType<PrismSettingsManager>()
                .Resolve<SavedSettings>(typeof(TRequest));

            if (savedRequests != null && savedRequests.SavedRequests != null)
            {
                var autoLoads = savedRequests.SavedRequests
                    .Where(r => r is TRequest)
                    .Cast<TRequest>()
                    .Where(r => r.Autoload)
                    .ToArray();
                if (autoLoads.Any())
                {
                    var mapper = new ClassSelfMapper();
                    mapper.Map(autoLoads.First(), Request);
                }
            }
            if(SkipObjectEntry)
            {
                var subDialogs = new List<DialogViewModel>(SubDialogs);
                subDialogs.Remove(ConfigEntryDialog);
                SubDialogs = subDialogs;
            }
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
            LogController = new LogController(progressControlViewModel);
            LogController.AddLevel2Ui(progressControlViewModelLevel2);

            Response = Service.Execute(Request, LogController);

            CompletionItem = Response;

            if (Response.Success)
                ProcessCompletionExtention();

            Controller.RemoveFromUi(progressControlViewModel);
            Controller.RemoveFromUi(progressControlViewModelLevel2);

            IsProcessing = false;

            if (!Response.Success)
                ProcessError(Response.Exception);
            else if (CompletionMessage.IsNullOrWhiteSpace())
                CompletionMessage = "Process Finished";
        }

        protected virtual void ProcessCompletionExtention()
        {
        }

        public override string TabLabel
        {
            get { return typeof(TRequest).GetDisplayName(); }
        }

        public ObjectEntryDialog ConfigEntryDialog { get; private set; }
        public LogController LogController { get; private set; }

        public void OpenFolder(string folder)
        {
            try
            {
                Process.Start(folder);
            }
            catch (Exception ex)
            {
                ApplicationController.UserMessage(ex.DisplayString());
            }
        }
    }
}