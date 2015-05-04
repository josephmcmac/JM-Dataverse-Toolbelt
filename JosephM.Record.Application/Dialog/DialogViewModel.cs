#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Record.Application.Shared;
using JosephM.Record.Application.TabArea;

#endregion

namespace JosephM.Record.Application.Dialog
{
    /// <summary>
    ///     Base Class For Implementing A Process Within The Application Which May Or May Not Have Child Processes
    /// </summary>
    public abstract class DialogViewModel : TabAreaViewModelBase
    {
        private readonly ObservableCollection<object> _completionItems =
            new ObservableCollection<object>();

        private readonly ObservableCollection<XrmButtonViewModel> _completionOptions =
            new ObservableCollection<XrmButtonViewModel>();

        private int _currentSubDialogIndex;
        private bool _dialogCompletionStarted;

        private IEnumerable<DialogViewModel> _subDialogs = new DialogViewModel[0];

        protected IEnumerable<DialogViewModel> SubDialogs
        {
            get { return _subDialogs; }
            set { _subDialogs = value; }
        }

        protected DialogViewModel(DialogViewModel parentDialog)
            : base(parentDialog.ApplicationController)
        {
            ParentDialog = parentDialog;
            Controller = parentDialog.Controller;
        }

        protected DialogViewModel(IDialogController controller)
            : base(controller.ApplicationController)
        {
            Controller = controller;
            Controller.MainDialog = this;
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

        public ObservableCollection<object> CompletionItems
        {
            get { return _completionItems; }
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

        public Action OnCancel
        {
            get { return Controller.Close; }
        }

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
            else if (!_dialogCompletionStarted)
            {
                _dialogCompletionStarted = true;
                CompleteDialog();
            }
            else
            {
                if (ParentDialog != null)
                    ParentDialog.StartNextAction();
                else
                    Controller.ShowCompletionScreen(this);
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
                    try
                    {
                        LoadDialogExtention();
                    }
                    catch (Exception ex)
                    {
                        ProcessError(ex);
                    }
                });
        }

        protected void ProcessError(Exception ex)
        {
            FatalException = ex;
            if (ParentDialog != null)
                ParentDialog.ProcessError(ex);
            else
            {
                CompletionMessage = string.Format("Fatal error during import\n{0}", ex.DisplayString());
                Controller.ShowCompletionScreen(this);
            }
        }

        protected abstract void LoadDialogExtention();

        public void CompleteDialog()
        {
            ApplicationController.DoOnAsyncThread(
                () =>
                {
                    try
                    {
                        CompleteDialogExtention();
                        StartNextAction();
                    }
                    catch (Exception ex)
                    {
                        ProcessError(ex);
                    }
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
    }
}