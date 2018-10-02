using JosephM.Core.Log;
using System;

namespace JosephM.Core.Service
{
    public class ServiceRequestController
    {
        public ServiceRequestController(LogController controller, Action<object> addToUi = null, Action<object> removeToUi = null)
        {
            Controller = controller;
            AddToUi = addToUi;
            RemoveToUi = removeToUi;
        }

        public LogController Controller { get; }
        private Action<object> AddToUi { get; }
        private Action<object> RemoveToUi { get; }

        public void UpdateProgress(int done, int toDo, string message)
        {
            Controller.UpdateProgress(done, toDo, message);
        }

        public void AddObjectToUi(object item)
        {
            AddToUi?.Invoke(item);
        }

        public void RemoveObjectFromUi(object item)
        {
            RemoveToUi?.Invoke(item);
        }

        public void LogLiteral(string message)
        {
            Controller.LogLiteral(message);
        }

        public void UpdateLevel2Progress(int done, int toDo, string message)
        {
            Controller.UpdateLevel2Progress(done, toDo, message);
        }

        public void TurnOffLevel2()
        {
            Controller.TurnOffLevel2();
        }
    }
}
