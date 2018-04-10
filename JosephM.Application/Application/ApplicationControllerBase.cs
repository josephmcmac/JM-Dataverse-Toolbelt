#region

using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Core.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Threading;

#endregion

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
            Notifications = new ObservableCollection<Notification>();
        }

        public void AddNotification(string id, string notification, bool isLoading = false)
        {
            DoOnMainThread(() =>
            {
                var matchingIds = Notifications
                    .Where(kv => kv.Key == id)
                    .ToArray();
                foreach (var item in matchingIds)
                {
                    Notifications.Remove(item);
                }
                Notifications.Add(new Notification(id, notification, isLoading));
            });
        }

        public virtual void OpenFile(string fileName)
        {
            Process.Start(fileName);
        }

        public ObservableCollection<Notification> Notifications { get; private set; }

        public abstract void Remove(string regionName, object item);

        public abstract IEnumerable<object> GetObjects(string regionName);

        public abstract void RequestNavigate(string regionName, Type type, UriQuery uriQuery);

        public Dispatcher Dispatcher { get; private set; }

        public abstract void UserMessage(string message);

        public abstract bool UserConfirmation(string message);

        public void DoOnMainThread(Action action)
        {
            if (!RunThreadsAsynch)
            {
                action();
            }
            else
            {
                Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
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

        public virtual void NavigateTo(Type type)
        {
            throw new NotImplementedException();
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
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "JosephM Xrm", ApplicationName);
            }
        }

        public string ApplicationName { get; set; }

        public string LogPath
        {
            get; set;
        }

        public virtual void ThrowException(Exception ex)
        {
            UserMessage(ex.DisplayString());
        }

        public virtual void NavigateTo(Type type, UriQuery uriQuery)
        {
            throw new NotImplementedException();
        }

        public abstract string GetSaveFileName(string initialFileName, string extention);

        public abstract string GetSaveFolderName();

        public virtual void SeralializeObjectToFile(object theObject, string fileName)
        {
            var serializer = new DataContractSerializer(theObject.GetType());
            FileUtility.CheckCreateFolder(Path.GetDirectoryName(fileName));
            using (
                var fileStream = new FileStream(fileName, FileMode.Create))
            {
                serializer.WriteObject(fileStream, theObject);
            }
        }

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

        public void RegisterInstance(Type type, object instance)
        {
            Container.RegisterInstance(type, instance);
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

        public virtual void OpenHelp(string fileName)
        {
            throw new NotImplementedException();
        }

        public void RegisterInstance(Type type, string key, object instance)
        {
            Container.RegisterInstance(type, key, instance);
        }

        public object ResolveInstance(Type type, string key)
        {
            return Container.ResolveInstance(type, key);
        }

        public virtual bool AllowSaveRequests { get { return true; } }

        public virtual bool ForceElementWindowHeight {  get { return false; } }

        public class Notification
        {
            public Notification(string key, string value, bool isLoading)
            {
                Key = key;
                Value = value;
                IsLoading = isLoading;
            }

            public string Key { get; set; }
            public string Value { get; set; }
            public bool IsLoading { get; set; }
        }
    }
}