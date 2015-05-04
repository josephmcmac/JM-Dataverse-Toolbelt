#region

using System.Collections.ObjectModel;
using JosephM.Record.Application.Constants;
using JosephM.Record.Application.Controller;

#endregion

namespace JosephM.Record.Application.Dialog
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
        }

        public DialogViewModel MainDialog { get; set; }
        public ObservableCollection<ViewModelBase> UiItems { get; private set; }

        public bool IsStarted { get; set; }
        public IApplicationController ApplicationController { get; private set; }

        public void Close()
        {
            ApplicationController.Remove(RegionNames.MainTabRegion, MainDialog);
        }

        public virtual void ShowCompletionScreen(DialogViewModel dialog)
        {
            var completionScreenViewModel = new CompletionScreenViewModel(Close, dialog.CompletionMessage,
                dialog.CompletionOptions, dialog.CompletionItems,
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

        public void BeginDialog()
        {
            MainDialog.LoadDialog();
            IsStarted = true;
        }
    }
}