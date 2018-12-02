#region

using System;
using System.Collections.ObjectModel;
using JosephM.Application.Application;

#endregion

namespace JosephM.Application.ViewModel.Dialog
{
    /// <summary>
    ///  Interfaces Between The Dialogs And The UI And Application Controller
    /// </summary>
    public class DialogController : IDialogController
    {
        public DialogController(IApplicationController applicationController)
        {
            ApplicationController = applicationController;
            UiItems = new ObservableCollection<ViewModelBase>();
            RemoveMethod = () => ApplicationController.Remove(MainDialog);
        }

        public DialogViewModel MainDialog { get; set; }
        public ObservableCollection<ViewModelBase> UiItems { get; private set; }

        public bool IsStarted { get; set; }
        public IApplicationController ApplicationController { get; private set; }
        public Action RemoveMethod { get; set; }

        public void Close()
        {
            RemoveMethod();
            if (MainDialog.OverideCompletionScreenMethod != null)
                MainDialog.OverideCompletionScreenMethod();
        }

        public virtual void ShowCompletionScreen(DialogViewModel dialog)
        {
            var completionScreenViewModel = new CompletionScreenViewModel(dialog.OnCancel ?? Close, dialog.CompletionMessage,
                dialog.CompletionOptions, dialog.CompletionItem,
                ApplicationController);
            LoadToUi(completionScreenViewModel);
        }

        public virtual void LoadToUi(ViewModelBase viewModel)
        {
            ApplicationController.DoOnMainThread(() => UiItems.Add(viewModel));
        }

        public virtual void RemoveFromUi(ViewModelBase viewModel)
        {
            ApplicationController.DoOnMainThread(() => UiItems.Remove(viewModel));
        }

        public virtual void BeginDialog()
        {
            MainDialog.LoadDialog();
            IsStarted = true;
        }
    }
}