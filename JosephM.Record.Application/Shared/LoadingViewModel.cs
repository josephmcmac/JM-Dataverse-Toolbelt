#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JosephM.Record.Application.Controller;

#endregion

namespace JosephM.Record.Application.Shared
{
    public class LoadingViewModel : ViewModelBase
    {
        private object _lockLoading = new Object();
        protected Dictionary<object, string> LoadingObjects { get; set; }

        private string StandardLoadingString
        {
            get { return "Please Wait While Loading..."; }
        }

        public LoadingViewModel(IApplicationController controller)
            : base(controller)
        {
            LoadingMessage = StandardLoadingString;
            LoadingObjects = new Dictionary<object, string>();
        }

        private List<Action> _triggerActions = new List<Action>();
        public void AddTriggerMethod(Action action)
        {
            _triggerActions.Add(action);
        }

        private string _loadingMessage;

        public string LoadingMessage
        {
            get { return _loadingMessage; }
            set
            {
                _loadingMessage = value;
                OnPropertyChanged("LoadingMessage");
            }
        }

        public bool IsLoading
        {
            get
            {
                lock (_lockLoading)
                {
                    return LoadingObjects.Any();
                }
            }
        }

        internal void DoWhileLoading(string message, Action action)
        {
            var loadingObject = new object();
            try
            {
                var isLoadingBefore = false;
                lock (_lockLoading)
                {
                    isLoadingBefore = LoadingObjects.Any();
                    LoadingObjects.Add(loadingObject, message);
                }
                if (!isLoadingBefore)
                {
                    OnPropertyChanged("IsLoading");
                    foreach (var triggerAction in _triggerActions)
                    {
                        triggerAction();
                    }
                }
                if (message != null)
                    LoadingMessage = message;
                action();
            }
            finally
            {
                lock (_lockLoading)
                {
                    LoadingObjects.Remove(loadingObject);
                    if (!LoadingObjects.Any())
                    {
                        OnPropertyChanged("IsLoading");
                        foreach (var triggerAction in _triggerActions)
                        {
                            triggerAction();
                        }
                    }
                }
                //LoadingMessage = StandardLoadingString;
            }
        }
    }
}