#region

using JosephM.Core.AppConfig;
using System;
using System.Diagnostics;

#endregion

namespace JosephM.Application.Application
{
    /// <summary>
    ///     Object For Accessing Variables, Threads And Common Operations Required By An Application
    /// </summary>
    public interface IApplicationController : IDependencyResolver
    {
        bool RunThreadsAsynch { get; }
        /// <summary>
        ///     The Name Of The Application
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        ///     Removes An Object From The UI Region In The Application
        /// </summary>
        void Remove(string regionName, object item);

        /// <summary>
        ///     Invokes A Request To Open Something defined by The Uri In The UI Region
        /// </summary>
        void RequestNavigate(string regionName, Type type, UriQuery uriQuery);

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

        void NavigateTo(Type type, UriQuery uriQuery);

        string GetSaveFileName(string initialFileName, string extention);

        string GetSaveFolderName();

        void SeralializeObjectToFile(object theObject, string fileName);

        Process StartProcess(string fileName, string arguments = null);

        void OpenHelp(string fileName);
        void AddNotification(string id, string notification, bool isLoading = false);

        void OpenFile(string fileName);

        bool AllowSaveRequests { get; }

        bool ForceElementWindowHeight { get; }
    }
}