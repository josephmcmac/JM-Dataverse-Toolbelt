#region

using JosephM.Application.Application;
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
            
        }
    }
}