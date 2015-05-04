#region

using JosephM.Record.Application.Navigation;
using System;

#endregion

namespace JosephM.Record.Application.Controller
{
    /// <summary>
    ///     Object For Accessing Variables, Threads And Common Operations Required By An Application
    /// </summary>
    public interface IApplicationController
    {
        /// <summary>
        ///     The Name Of The Application
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        ///     Removes An Object From The UI Region In The Application
        /// </summary>
        void Remove(string regionName, object item);

        /// <summary>
        ///     Adds An Object To The UI Region In The Application
        /// </summary>
        void AddToRegion(string regionName, object navigationErrorViewModel);

        /// <summary>
        ///     Invokes A Request To Open Something defined by The Uri In The UI Region
        /// </summary>
        void RequestNavigate(string regionName, Uri uri);

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
        ///     Invokes A Request To Open A Record In Its Maintain Record Entry Form
        /// </summary>
        void OpenRecord(string entityType, string fieldMatch, string fieldValue, Type maintainViewModelType);

        /// <summary>
        ///     Invokes An Action To Be Started Asynchronously To The Current Thread
        /// </summary>
        void DoOnAsyncThread(Action action);

        /// <summary>
        ///     Folder Where User Defined Settings Are Stored By The Application
        /// </summary>
        string SettingsPath { get; }

        /// <summary>
        ///     The Folder Where The Application Is Installed
        /// </summary>
        string ApplicationPath { get; }

        void ThrowException(Exception ex);

        void NavigateTo(Type type, UriQuery uriQuery);
    }
}