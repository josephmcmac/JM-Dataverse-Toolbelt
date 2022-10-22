using JosephM.Application.Application;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Log;

namespace JosephM.Application.ViewModel.Shared
{
    public class ProgressControlViewModel : ViewModelBase, IUserInterface
    {
        private double _fractionCompleted;

        private string _message;

        public ProgressControlViewModel(IApplicationController controller, bool createLevel2 = true)
            : base(controller)
        {
            if (createLevel2)
                Level2ProgressControlViewModel = new ProgressControlViewModel(controller, createLevel2: false);
        }

        public ProgressControlViewModel Level2ProgressControlViewModel { get; set; }

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

        public LogController CreateLogControllerFor()
        {
            var logController = new LogController(this);
            logController.AddLevel2Ui(Level2ProgressControlViewModel);
            return logController;
        }
    }
}