#region

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Threading;
using JosephM.Core.Extentions;
using JosephM.Core.Utility;
using JosephM.Record.Application.Navigation;

#endregion

namespace JosephM.Record.Application.Controller
{
    /// <summary>
    ///     Base Class For Implementing IApplicationController
    /// </summary>
    public abstract class ApplicationControllerBase : IApplicationController
    {
        protected ApplicationControllerBase(string applicationName)
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
            ApplicationName = applicationName;
        }

        public abstract void Remove(string regionName, object item);

        public abstract void AddToRegion(string regionName, object navigationErrorViewModel);

        public abstract void RequestNavigate(string regionName, Uri uri);

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
            new Thread(
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
                }).Start();
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

        public void SeralializeObjectToFile(object theObject, string fileName)
        {
            try
            {
                var serializer = new DataContractSerializer(theObject.GetType());
                FileUtility.CheckCreateFolder(Path.GetDirectoryName(fileName));
                using (
                    var fileStream = new FileStream(fileName, FileMode.Create))
                {
                    serializer.WriteObject(fileStream, theObject);
                }
            }
            catch (Exception ex)
            {
                UserMessage(string.Format("Error Saving Object\n{0}", ex.DisplayString()));
            }
        }
    }
}