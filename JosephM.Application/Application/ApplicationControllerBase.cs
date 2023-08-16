using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace JosephM.Application.Application
{
    /// <summary>
    ///     Base Class For Implementing IApplicationController
    /// </summary>
    public abstract class ApplicationControllerBase : IApplicationController
    {
        public virtual bool RunThreadsAsynch
        {
            get
            {
                return true;
            }
        }

        protected ApplicationControllerBase(string applicationName, IDependencyResolver container)
        {
            Container = container;
            Dispatcher = Dispatcher.CurrentDispatcher;
            ApplicationName = applicationName;
        }

        public void LogEvent(string eventName, IDictionary<string, string> properties = null)
        {
            foreach(var item in _loggers.ToArray())
            {
                item.LogEvent(eventName, properties);
            }
        }

        public void AddLogger(IApplicationLogger applicationLogger)
        {
            _loggers.Add(applicationLogger);
        }

        public virtual void AddNotification(string id, string notification, bool isLoading = false, Dictionary<string, Action> actions = null)
        {
        }

        public virtual void OpenFile(string fileName)
        {
            Process.Start(fileName);
        }

        private List<IApplicationLogger> _loggers = new List<IApplicationLogger>();

        public abstract void Remove(object item);

        public abstract IEnumerable<object> GetObjects();

        public Dispatcher Dispatcher { get; private set; }

        public abstract void UserMessage(string message);

        public abstract bool UserConfirmation(string message);

        public bool CurrentThreadIsDispatcher()
        {
            return Dispatcher.Thread == Thread.CurrentThread;
        }

        public void DoOnMainThread(Action action)
        {
            if (!RunThreadsAsynch)
            {
                action();
            }
            else
            {
                Dispatcher.BeginInvoke(
                    DispatcherPriority.Send,
                    (SendOrPostCallback)
                        delegate
                        {
                            try
                            {
                                action();
                            }
                            catch (Exception ex)
                            {
                                ThrowException(ex);
                            }
                        },
                    null);
            }
        }

        public void DoOnAsyncThread(Action action)
        {
            if (!RunThreadsAsynch)
            {
                action();
            }
            else
            {
                var thread = new Thread(
                () =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        ThrowException(ex);
                    }
                });
                thread.IsBackground = true;
                thread.Start();
            }
        }

        public string SettingsPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "JM Software", ApplicationName);
            }
        }

        public string ApplicationName { get; set; }

        public string LogPath
        {
            get; set;
        }

        public virtual void ThrowException(Exception ex)
        {
            foreach (var item in _loggers.ToArray())
            {
                item.LogException(ex);
            }
            UserMessage(ex.DisplayString());
        }

        public abstract void NavigateTo(Type type, UriQuery uriQuery = null);
        public abstract void NavigateTo(object item);
        public abstract string GetSaveFileName(string initialFileName, string extention);

        public abstract string GetSaveFolderName();

        public virtual Process StartProcess(string fileName, string arguments = null)
        {
            Process process = null;
            try
            {
                process = Process.Start(fileName, arguments);
            }
            catch (Exception ex)
            {
                ThrowException(ex);
            }
            return process;
        }

        public IDependencyResolver Container { get; set; }

        public virtual object ResolveType(Type type)
        {
            return Container == null ? null : Container.ResolveType(type);
        }

        public void AddOnInstanceRegistered<T>(Action processRegisteredObject)
        {
            _instanceRegisteredActions.Add(new KeyValuePair<Type, Action>(typeof(T), processRegisteredObject));
        }

        private List<KeyValuePair<Type, Action>> _instanceRegisteredActions = new List<KeyValuePair<Type, Action>>();

        private void OnInstanceRegistered(Type type)
        {
            foreach(var item in _instanceRegisteredActions)
            {
                if(item.Key == type)
                {
                    item.Value();
                }
            }
        }

        public void RegisterInstance(Type type, object instance)
        {
            Container.RegisterInstance(type, instance);
            OnInstanceRegistered(type);
        }

        public object ResolveType(string typeName)
        {
            return Container.ResolveType(typeName);
        }

        public void RegisterType<I, T>()
        {
            Container.RegisterType<I, T>();
        }

        public void RegisterTypeForNavigation<T>()
        {
            Container.RegisterTypeForNavigation<T>();
        }

        public virtual void ClearTabs()
        {
            throw new NotImplementedException();
        }

        public void RegisterInstance(Type type, string key, object instance)
        {
            Container.RegisterInstance(type, key, instance);
            OnInstanceRegistered(type);
        }

        public object ResolveInstance(Type type, string key)
        {
            return Container.ResolveInstance(type, key);
        }

        public virtual bool AllowSaveRequests { get { return true; } }

        public virtual bool IsTabbedApplication {  get { return true; } }

        //only implemented by tabbed application controllers
        public virtual object ActiveTabItem { get; set; }

        public virtual string Version => null;

        //runs all registered on navigate events
        protected void OnNavigatedTo(object objectNavigatedTo)
        {
            var navigationEvents = Container.ResolveType<NavigationEvents>();
            foreach (var eventAction in navigationEvents.EventActions)
                eventAction(objectNavigatedTo);
        }

        public Type AppImageType { get; set; }
    }
}