#region

using JosephM.Core.Log;
using JosephM.Record.Application.Controller;

#endregion

namespace JosephM.Record.Application.Shared
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
            protected set
            {
                _fractionCompleted = value;
                OnPropertyChanged("FractionCompleted");
            }
        }

        public string Message
        {
            get { return _message; }
            protected set
            {
                _message = value;
                OnPropertyChanged("Message");
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
                OnPropertyChanged("UiActive");
            }
        }
    }
}