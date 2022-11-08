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
using System.Linq;

namespace JosephM.Application.Desktop.Module.ServiceRequest
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

        public virtual bool DisplayResponseDuringServiceRequestExecution
        {
            get
            {
                return false;
            }
        }

        public ServiceRequestDialog(TService service, IDialogController dialogController, IRecordService lookupService, TRequest request = null, Action onClose = null, string nextButtonLabel = null, string initialValidationMessage = null, string initialLoadingMessage = null)
            : base(dialogController)
        {
            if (onClose != null)
                OnCancel = onClose;

            Service = service;
            
            if (request != null)
                Request = request;
            else
                Request = ApplicationController.ResolveType<TRequest>();

            ConfigEntryDialog = new ObjectEntryDialog(Request, this, ApplicationController, lookupService, null, null, onClose, saveButtonLabel: nextButtonLabel ?? "Next", initialValidationMessage: initialValidationMessage, initialLoadingMessage: initialLoadingMessage);
            SubDialogs = new DialogViewModel[] { ConfigEntryDialog };
        }

        public bool SkipObjectEntry { get; set; }

        public TService Service { get; set; }

        public TRequest Request { get; set; }

        protected TResponse Response { get; set; }

        protected override void LoadDialogExtention()
        {
            //todo - this should be part of the saved request module
            var savedRequests = ApplicationController
                .ResolveType<ISettingsManager>()
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
            if (SkipObjectEntry)
            {
                var subDialogs = new List<DialogViewModel>(SubDialogs);
                subDialogs.Remove(ConfigEntryDialog);
                SubDialogs = subDialogs;
            }
            StartNextAction();
        }

        protected virtual bool UseProgressControlUi => false;


        protected override void CompleteDialogExtention()
        {
            LoadingViewModel.IsLoading = true;
            IsProcessing = true;
            Response = new TResponse();
            Response.HideResponseItems = true;

            ProgressControlViewModel progressControlViewModel = null;
            ServiceRequestController serviceRequestController = null;
            if (UseProgressControlUi)
            {
                LoadingViewModel.IsLoading = false;
                progressControlViewModel = new ProgressControlViewModel(ApplicationController);
                if (DisplayResponseDuringServiceRequestExecution)
                {
                    progressControlViewModel.SetDetailObject(Response);
                }
                Controller.LoadToUi(progressControlViewModel);
                LogController = progressControlViewModel.CreateLogControllerFor();
                
                serviceRequestController = new ServiceRequestController(LogController, (o) => progressControlViewModel.SetDetailObject(o), (o) => progressControlViewModel.ClearDetailObject());
            }
            else
            {
                if (DisplayResponseDuringServiceRequestExecution)
                {
                    LoadingViewModel.SetDetailObject(Response);
                }
                LogController = new LogController(LoadingViewModel);
                serviceRequestController = new ServiceRequestController(LogController, (o) => LoadingViewModel.SetDetailObject(o), (o) => LoadingViewModel.ClearDetailObject());
            }
            Response = Service.Execute(Request, serviceRequestController, response: Response);
            CompletionItem = Response;

            if (Response.Success)
            {
                ProcessCompletionExtention();
            }
            if (progressControlViewModel != null)
            {
                Controller.RemoveFromUi(progressControlViewModel);
            }
            Response.HideResponseItems = false;

            IsProcessing = false;
        }

        protected virtual void ProcessCompletionExtention()
        {
        }

        private string _tabLabel;
        public override string TabLabel
        {
            get
            {
                if (_tabLabel == null)
                {
                    _tabLabel = typeof(TRequest).GetDisplayName();
                    if (_tabLabel.EndsWith(" Request"))
                        _tabLabel = _tabLabel.Substring(0, _tabLabel.IndexOf(" Request", StringComparison.Ordinal));
                }
                return _tabLabel;
            }
        }

        public override void SetTabLabel(string newLabel)
        {
            _tabLabel = newLabel;
        }

        public ObjectEntryDialog ConfigEntryDialog { get; private set; }
        public LogController LogController { get; private set; }

        public void OpenFolder(string folder)
        {
            try
            {
                ApplicationController.StartProcess(folder);
            }
            catch (Exception ex)
            {
                ApplicationController.UserMessage(ex.DisplayString());
            }
        }

        protected override IDictionary<string, string> GetPropertiesForCompletedLog()
        {
            var dictionary = base.GetPropertiesForCompletedLog();
            void addProperty(string name, string value)
            {
                if (!dictionary.ContainsKey(name))
                    dictionary.Add(name, value);
            }
            var responseErrors = Response.GetResponseItemsWithError();
            if (responseErrors.Any())
            {
                addProperty("Response Error Count", responseErrors.Count().ToString());
                addProperty("Response Error Count Distinct", responseErrors.Select(re => re.Exception.Message).Distinct().Count().ToString());
                addProperty("Response Error Messages", string.Join(Environment.NewLine, responseErrors.Select(re => re.Exception.Message).Distinct().Take(20)));
                addProperty("First Error Detail", responseErrors.First().ErrorDetails);
            }
            return dictionary;
        }
    }
}