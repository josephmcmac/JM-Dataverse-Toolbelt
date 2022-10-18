using JosephM.Core.AppConfig;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace JosephM.Application.Application
{
    /// <summary>
    ///     Object For Accessing Variables, Threads And Common Operations Required By An Application
    /// </summary>
    public interface IApplicationController : IDependencyResolver
    {
        bool CurrentThreadIsDispatcher();
        bool RunThreadsAsynch { get; }
        /// <summary>
        ///     The Name Of The Application
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        ///     Removes An Object From The Application UI
        /// </summary>
        void Remove(object item);

        /// <summary>
        ///     Gets All Objects Loaded In The Application UI
        /// </summary>
        IEnumerable<object> GetObjects();


        /// <summary>
        ///     Invokes A Popup With A Message For The User
        /// </summary>
        void UserMessage(string message);

        /// <summary>
        ///     Invokes A Popup With A Message Requiring The User To Confirm An Action
        /// </summary>
        bool UserConfirmation(string message);

        /// <summary>
        ///     Invokes An Action To Be Started On The Main STA Thread Of The Application
        /// </summary>
        void DoOnMainThread(Action action);

        /// <summary>
        ///     Invokes An Action To Be Started Asynchronously To The Current Thread
        /// </summary>
        void DoOnAsyncThread(Action action);

        /// <summary>
        ///     Folder Where User Defined Settings Are Stored By The Application
        /// </summary>
        string SettingsPath { get; }

        /// <summary>
        ///     The Folder Where The Log Should Be Output
        /// </summary>
        string LogPath { get; set; }

        void ThrowException(Exception ex);

        void NavigateTo(object item);

        void NavigateTo(Type type, UriQuery uriQuery);

        string GetSaveFileName(string initialFileName, string extention);

        string GetSaveFolderName();

        Process StartProcess(string fileName, string arguments = null);

        void AddNotification(string id, string notification, bool isLoading = false, Dictionary<string, Action> actions = null);

        void OpenFile(string fileName);

        bool AllowSaveRequests { get; }

        bool IsTabbedApplication { get; }

        object ActiveTabItem { get; set; }

        Type AppImageType { get; set; }

        void AddLogger(IApplicationLogger applicationLogger);

        void LogEvent(string eventame, IDictionary<string, string> properties = null);

        string Version { get; }
    }
}