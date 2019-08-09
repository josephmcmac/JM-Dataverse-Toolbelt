using JosephM.Application.Application;
using JosephM.Application.ViewModel.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.Application.ViewModel.Notification
{
    public class NotificationViewModel : ViewModelBase
    {
        public NotificationViewModel(string key, string value, bool isLoading, Dictionary<string, Action> actions, IApplicationController applicationController)
            : base(applicationController)
        {
            Key = key;
            Value = value;
            IsLoading = isLoading;
            OpenActionsCommand = new MyCommand(() => OpenActions = true);
            if (actions != null)
            {
                Actions = actions.Select(a => new XrmButtonViewModel(a.Key,
                    () => { a.Value(); OpenActions = false; },
                    applicationController)).ToArray();
            }
        }

        public string Key { get; set; }
        public string Value { get; set; }
        public bool IsLoading { get; set; }
        public MyCommand OpenActionsCommand { get; }

        private bool _openActions;
        public bool OpenActions
        {
            get { return _openActions; }
            set
            {
                _openActions = value;
                OnPropertyChanged(nameof(OpenActions));
            }
        }

        public IEnumerable<XrmButtonViewModel> Actions { get; }
    }
}
