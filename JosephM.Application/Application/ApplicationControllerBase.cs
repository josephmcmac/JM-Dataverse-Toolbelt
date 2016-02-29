#region

using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        protected ApplicationControllerBase(string applicationName, IDependencyResolver container)
        {
            Container = container;
            Dispatcher = Dispatcher.CurrentDispatcher;
            ApplicationName = applicationName;
        }

        public abstract void Remove(string regionName, object item);

        public abstract IEnumerable<object> GetObjects(string regionName);

        public abstract void RequestNavigate(string regionName, Type type, UriQuery uriQuery);

        public Dispatcher Dispatcher { get; private set; }

        public abstract void UserMessage(string message);

        public abstract bool UserConfirmation(string message);

        public virtual void DoOnMainThread(Action action)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                (SendOrPostCallback)
                    delegate { action(); },
                null);
        }

        public abstract void OpenRecord(string recordType, string fieldMatch, string fieldValue,
            Type maintainViewModelType);

        public virtual void NavigateTo(Type type)
        {
            throw new NotImplementedException();
        }

        public virtual void DoOnAsyncThread(Action action)
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
                        UserMessage(string.Concat("Warning unhandled exception:\n",
                            ex.DisplayString()));
                    }
                });
            thread.IsBackground = true;
            thread.Start();
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

        public string ApplicationPath
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
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

        public object ResolveType(Type type)
        {
            return Container.ResolveType(type);
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

        public virtual void OpenHelp(string qualified)
        {
            throw new NotImplementedException();
        }
    }
}