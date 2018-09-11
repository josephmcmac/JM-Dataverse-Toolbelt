#region

using JosephM.Application.Application;

#endregion

namespace JosephM.Application.ViewModel.Shared
{
    public class LoadingViewModel : ViewModelBase
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
    }
}