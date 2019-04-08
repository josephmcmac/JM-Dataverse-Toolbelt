using System;
using System.Collections.ObjectModel;
using JosephM.Application.Application;

namespace JosephM.Application.ViewModel.Dialog
{
    public interface IDialogController
    {
        DialogViewModel MainDialog { get; set; }
        ObservableCollection<ViewModelBase> UiItems { get; }
        bool IsStarted { get; set; }
        IApplicationController ApplicationController { get; }
        Action RemoveMethod { get; set; }
        void Close();
        void ShowCompletionScreen(DialogViewModel dialog);
        void LoadToUi(ViewModelBase viewModel);
        void RemoveFromUi(ViewModelBase viewModel);
        void BeginDialog();
    }
}