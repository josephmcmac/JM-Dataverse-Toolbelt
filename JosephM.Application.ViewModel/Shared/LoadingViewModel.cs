#region

using JosephM.Application.Application;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Core.Log;
using System.Windows.Navigation;

#endregion

namespace JosephM.Application.ViewModel.Shared
{
    public class LoadingViewModel : ViewModelBase, IUserInterface
    {
        private string StandardLoadingString
        {
            get { return "Please Wait While Loading..."; }
        }

        public LoadingViewModel(IApplicationController controller)
            : base(controller)
        {
            LoadingMessage = StandardLoadingString;
        }

        private string _loadingMessage;

        public string LoadingMessage
        {
            get { return _loadingMessage; }
            set
            {
                _loadingMessage = value;
                OnPropertyChanged(nameof(LoadingMessage));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public bool UiActive { get => IsLoading; set => IsLoading = value; }

        public void LogMessage(string message)
        {
            LoadingMessage = message;
        }

        public void LogDetail(string stage)
        {
            LoadingMessage = stage;
        }

        public void UpdateProgress(int countCompleted, int countOutOf, string message)
        {
            LoadingMessage = message;
        }

        public ObjectDisplayViewModel DetailObjectViewModel { get; set; }

        public void SetDetailObject(object detailObject)
        {
            ApplicationController.DoOnAsyncThread(() =>
            {
                DetailObjectViewModel = new ObjectDisplayViewModel(detailObject, FormController.CreateForObject(detailObject, ApplicationController, null));
                OnPropertyChanged(nameof(DetailObjectViewModel));
            });
        }

        public void ClearDetailObject()
        {
            DetailObjectViewModel = null;
            OnPropertyChanged(nameof(DetailObjectViewModel));
        }
    }
}