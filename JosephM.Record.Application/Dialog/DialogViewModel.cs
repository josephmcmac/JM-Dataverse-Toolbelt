using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.Shared;
using JosephM.Application.ViewModel.TabArea;
using JosephM.Core.Extentions;

namespace JosephM.Application.ViewModel.Dialog
{
    /// <summary>
    ///     Base Class For Implementing A Process Within The Application Which May Or May Not Have Child Processes
    /// </summary>
    public abstract class DialogViewModel : TabAreaViewModelBase
    {
        public Action OverideCompletionScreenMethod { get; set; }

        private readonly ObservableCollection<XrmButtonViewModel> _completionOptions =
            new ObservableCollection<XrmButtonViewModel>();

        private int _currentSubDialogIndex;
        protected bool DialogCompletionCommit { get; set; }

        private IEnumerable<DialogViewModel> _subDialogs = new DialogViewModel[0];

        public IEnumerable<DialogViewModel> SubDialogs
        {
            get { return _subDialogs; }
            set { _subDialogs = value; }
        }

        protected DialogViewModel(DialogViewModel parentDialog)
            : base(parentDialog.ApplicationController)
        {
            LoadingViewModel = parentDialog.LoadingViewModel;
            ParentDialog = parentDialog;
            Controller = parentDialog.Controller;
            ProgressControlViewModel = new ProgressControlViewModel(ApplicationController);
            OnCancel = Controller.Close;
        }

        protected DialogViewModel(IDialogController controller)
            : base(controller.ApplicationController)
        {
            Controller = controller;
            Controller.MainDialog = this;
            ProgressControlViewModel = new ProgressControlViewModel(ApplicationController);
            OnCancel = Controller.Close;
        }

        private string _tabLabel;
        public override string TabLabel
        {
            get
            {
                if(_tabLabel == null)
                {
                    _tabLabel = GetType().Name.SplitCamelCase();
                    if (_tabLabel.EndsWith(" Dialog"))
                        _tabLabel = _tabLabel.Substring(0, _tabLabel.IndexOf(" Dialog", StringComparison.Ordinal));
                }
                return _tabLabel;
            }
        }

        public virtual void SetTabLabel(string newLabel)
        {
            _tabLabel = newLabel;
        }

        public string CompletionMessage { get; set; }

        public object CompletionItem
        {
            get; set;
        }

        public ObservableCollection<XrmButtonViewModel> CompletionOptions
        {
            get { return _completionOptions; }
        }

        protected void AddCompletionOption(string label, Action action)
        {
            CompletionOptions.Add(new XrmButtonViewModel(label, action, ApplicationController));
        }

        public Exception FatalException { get; private set; }

        public Action OnCancel { get; set; }

        public IDialogController Controller { get; private set; }

        private DialogViewModel ParentDialog { get; set; }

        protected bool HasParentDialog
        {
            get { return ParentDialog != null; }
        }

        public bool IsProcessing { get; protected set; }

        protected void MoveBackToPrevious()
        {
            if(ParentDialog == null)
            {
                throw new Exception("Cannot Move Back. Parent Dialog Is Null");
            }
            if(ParentDialog._currentSubDialogIndex < 2)
            {
                throw new NotImplementedException("Cannot Move Back. Not Implemented For The First Child Dialog Of The Parent");
            }
            var previousDialog = ParentDialog.SubDialogs.ElementAt(ParentDialog._currentSubDialogIndex - 2);
            previousDialog.DialogCompletionCommit = false;
            ParentDialog._currentSubDialogIndex = ParentDialog._currentSubDialogIndex - 2;
            ParentDialog.StartNextAction();
        }

        protected void StartNextAction()
        {
            if (SubDialogs.Any() && _currentSubDialogIndex < SubDialogs.Count())
            {
                _currentSubDialogIndex++;
                SubDialogs.ElementAt(_currentSubDialogIndex - 1).LoadDialog();
            }
            else if (!DialogCompletionCommit)
            {
                DialogCompletionCommit = true;
                CompleteDialog();
            }
            else
            {
                if (ParentDialog != null)
                    ParentDialog.StartNextAction();
                else
                {
                    ApplicationController.LogEvent(DialogEventName + " Completed");
                    if (OverideCompletionScreenMethod != null)
                        OverideCompletionScreenMethod();
                    else
                        Controller.ShowCompletionScreen(this);
                }
            }
        }

        public virtual string GetCompletionHeading()
        {
            return "Finished";
        }

        public void LoadDialog()
        {
            ApplicationController.DoOnAsyncThread(
                () =>
                {
                    LoadingViewModel.IsLoading = true;
                    try
                    {
                        if (ParentDialog == null)
                            ApplicationController.LogEvent(DialogEventName + " Loaded");
                        LoadDialogExtention();
                    }
                    catch (Exception ex)
                    {
                        //if we have an application which does not spawn async threads
                        //and a fatal error has been thrown at completion processing
                        //then allow that error to find its way up the stack
                        if (DialogCompletionCommit)
                            throw;
                        else
                            ProcessError(ex);
                    }
                    finally
                    {
                        LoadingViewModel.IsLoading = false;
                    }
                });
        }

        private string DialogEventName => GetType().GetDisplayName();

        protected void ProcessError(Exception ex)
        {
            //note also used in CompleteDialog determining not to continue to next action
            FatalException = ex;
            if (ParentDialog != null)
                ParentDialog.ProcessError(ex);
            else
            {
                ApplicationController.LogEvent(DialogEventName + " Fatal Error", new Dictionary<string, string> { { "Error", ex.Message }, { "Error Trace", ex.DisplayString() } });
                CompletionMessage = string.Format("Fatal error:\n{0}", ex.DisplayString());
                Controller.ShowCompletionScreen(this);
            }
        }

        protected abstract void LoadDialogExtention();

        private List<Action> _onCompletedEvents = new List<Action>();

        public void AddOnCompletedEvent(Action action)
        {
            _onCompletedEvents.Add(action);
        }

        public void CompleteDialog()
        {
            ApplicationController.DoOnAsyncThread(
                () =>
                {
                    LoadingViewModel.IsLoading = true;
                    try
                    {
                        CompleteDialogExtention();
                        foreach (var action in _onCompletedEvents)
                            action();
                        if (DialogCompletionCommit)
                        {
                            if (FatalException == null)
                                StartNextAction();
                        }
                    }
                    catch (Exception ex)
                    {
                        //if we have an application which does not spawn async threads
                        //and a fatal error has been thrown at completion processing
                        //then allow that error to find its way up the stack
                        if (DialogCompletionCommit && !Thread.CurrentThread.IsBackground)
                            throw;
                        else
                            ProcessError(ex);
                    }
                    LoadingViewModel.IsLoading = false;
                }
                );
        }

        protected abstract void CompleteDialogExtention();

        protected override bool ConfirmTabClose()
        {
            if (!IsProcessing)
                return true;

            return false;
        }

        public ProgressControlViewModel ProgressControlViewModel { get; set; }

        private bool _showProgressControlViewModel;

        public bool ShowProgressControlViewModel
        {
            get { return _showProgressControlViewModel; }
            set
            {
                _showProgressControlViewModel = value;
                OnPropertyChanged(nameof(ShowProgressControlViewModel));
            }
        }

        protected void AddObjectToUi(object objectToDisplay, Action backAction = null, Action nextAction = null)
        {
            var vm = new ObjectDisplayViewModel(objectToDisplay, FormController.CreateForObject(objectToDisplay, ApplicationController, null)
                , backAction: backAction, nextAction: nextAction);
            Controller.LoadToUi(vm);
        }

        protected void RemoveObjectFromUi(object objectToDisplay)
        {
            foreach (var item in Controller.UiItems.ToArray())
            {
                if (item is ObjectDisplayViewModel && ((ObjectDisplayViewModel)item).GetObject() == objectToDisplay)
                    Controller.RemoveFromUi(item);
            }
        }
    }
}