using System.Collections.ObjectModel;
using JosephM.Record.Application.Controller;

namespace JosephM.Record.Application.Dialog
{
    public interface IDialogController
    {
        DialogViewModel MainDialog { get; set; }
        ObservableCollection<ViewModelBase> UiItems { get; }
        bool IsStarted { get; set; }
        IApplicationController ApplicationController { get; }
        void Close();
        void ShowCompletionScreen(DialogViewModel dialog);
        void LoadToUi(ViewModelBase viewModel);
        void RemoveFromUi(ViewModelBase viewModel);
        void BeginDialog();
    }
}