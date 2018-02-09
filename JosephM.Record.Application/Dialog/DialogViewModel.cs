#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JosephM.Application.ViewModel.Shared;
using JosephM.Application.ViewModel.TabArea;
using JosephM.Core.Extentions;

#endregion

namespace JosephM.Application.ViewModel.Dialog
{
    /// <summary>
    ///     Base Class For Implementing A Process Within The Application Which May Or May Not Have Child Processes
    /// </summary>
    public abstract class DialogViewModel : TabAreaViewModelBase
    {
        public Action OverideCompletionScreenMethod { get; set; }

        //private readonly ObservableCollection<object> _completionItems =
        //    new ObservableCollection<object>();

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

        public override string TabLabel
        {
            get
            {
                var typeName = GetType().Name.SplitCamelCase();
                if (typeName.EndsWith(" Dialog"))
                    return typeName.Substring(0, typeName.IndexOf(" Dialog", StringComparison.Ordinal));
                else
                    return typeName;
            }
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
        public bool IsProcessing { get; protected set; }

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

        protected void ProcessError(Exception ex)
        {
            //note also used in CompleteDialog determining not to continue to next action
            FatalException = ex;
            if (ParentDialog != null)
                ParentDialog.ProcessError(ex);
            else
            {
                CompletionMessage = string.Format("Fatal error:\n{0}", ex.DisplayString());
                if (OverideCompletionScreenMethod != null)
                {
                    ApplicationController.UserMessage(CompletionMessage);
                    OverideCompletionScreenMethod();
                }
                else
                    Controller.ShowCompletionScreen(this);
            }
        }

        protected abstract void LoadDialogExtention();

        public void CompleteDialog()
        {
            ApplicationController.DoOnAsyncThread(
                () =>
                {
                    LoadingViewModel.IsLoading = true;
                    try
                    {
                        CompleteDialogExtention();
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
                        if (DialogCompletionCommit)
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
                OnPropertyChanged("ShowProgressControlViewModel");
            }
        }
    }
}