#region

using JosephM.Application.Application;
using JosephM.Core.Log;

#endregion

namespace JosephM.Application.ViewModel.Shared
{
    public class ProgressControlViewModel : ViewModelBase, IUserInterface
    {
        private double _fractionCompleted;

        private string _message;

        public ProgressControlViewModel(IApplicationController controller)
            : base(controller)
        {
        }

        #region IUserInterface Members

        public void UpdateProgress(int countCompleted, int countOutOf, string message)
        {
            if (message != null)
                Message = message;
            FractionCompleted = countCompleted/(double) countOutOf;
        }

        public void LogMessage(string message)
        {
            UpdateProgress(message);
        }

        public void LogDetail(string stage)
        {
        }

        #endregion

        public double FractionCompleted
        {
            get { return _fractionCompleted; }
            set
            {
                _fractionCompleted = value;
                OnPropertyChanged(nameof(FractionCompleted));
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
                if (!UiActive)
                    UiActive = true;
            }
        }

        public void UpdateProgress(string message)
        {
            Message = message;
        }

        private bool _uiActive;

        public bool UiActive
        {
            get { return _uiActive; }
            set
            {
                _uiActive = value;
                OnPropertyChanged(nameof(UiActive));
            }
        }
    }
}