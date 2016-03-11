using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JosephM.Prism.Infrastructure.Dialog
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
            : this(service, dialogController, null)
        {
        }

        public ServiceRequestDialog(TService service, IDialogController dialogController, IRecordService lookupService)
            : base(dialogController)
        {
            Service = service;
            Request = new TRequest();

            var configEntryDialog = new ObjectEntryDialog(Request, this, ApplicationController, lookupService,
                LookupAllowedValues);
            SubDialogs = new DialogViewModel[] { configEntryDialog };
        }

        protected virtual IDictionary<string, IEnumerable<string>> LookupAllowedValues
        {
            get { return new Dictionary<string, IEnumerable<string>>(); }
        }

        protected TService Service { get; set; }

        protected TRequest Request { get; set; }

        protected TResponse Response { get; set; }

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
            Controller.RemoveFromUi(progressControlViewModel);
            Controller.RemoveFromUi(progressControlViewModelLevel2);

            foreach (var responseItem in Response.ResponseItems)
            {
                CompletionItems.Add(responseItem);
            }
            if (Request.GetType().GetCustomAttributes(typeof(AllowSaveAndLoad), false).Any())
            {
                AddCompletionOption("Save Request", SaveRequest);
            }

            if (Response.Success)
                ProcessCompletionExtention();

            IsProcessing = false;

            if (!Response.Success)
                ProcessError(Response.Exception);
            else
                CompletionMessage = "Process Finished";
        }

        private void SaveRequest()
        {
            var fileName = ApplicationController.GetSaveFileName("*", ".xml");
            if (!fileName.IsNullOrWhiteSpace())
                ApplicationController.SeralializeObjectToFile(Request, fileName);
        }

        protected virtual void ProcessCompletionExtention()
        {
        }

        public override string TabLabel
        {
            get { return typeof(TRequest).GetDisplayName(); }
        }

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