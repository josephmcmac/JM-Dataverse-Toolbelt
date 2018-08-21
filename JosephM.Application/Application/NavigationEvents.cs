using System;
using System.Collections.Generic;

namespace JosephM.Application.Application
{
    /// <summary>
    /// Holds all actions registered to run when an object is navigated to
    /// </summary>
    public class NavigationEvents
    {
        private List<Action<object>> _actions;

        public NavigationEvents()
        {
            _actions = new List<Action<object>>();
        }

        public void AddEvent(Action<object> action)
        {
            _actions.Add(action);
        }

        public IEnumerable<Action<object>> EventActions
        {
            get { return _actions; }
        }
    }
}