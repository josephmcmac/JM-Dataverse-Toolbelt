using System;
using System.Collections.Generic;
using System.Windows;
using JosephM.Application;
using JosephM.Application.Application;
using JosephM.Core.AppConfig;

namespace JosephM.XRM.VSIX.Dialogs
{
    class VsixApplicationController : ApplicationControllerBase
    {
        public VsixApplicationController(string applicationName, IDependencyResolver container) : base(applicationName, container)
        {
        }

        private Action<object> _remove = null;

        public void SetRemoveMethod(Action<object> action)
        {
            _remove = action;
        }
        public override void Remove(string regionName, object item)
        {
            if (_remove != null)
                _remove(item);
        }

        public override void RequestNavigate(string regionName, Type type, UriQuery uriQuery)
        {
            throw new NotImplementedException();
        }

        public override void UserMessage(string message)
        {
            DoOnMainThread(
                () => MessageBox.Show(message));
    //        VsShellUtilities.ShowMessageBox(
    //this.ServiceProvider,
    //"Finished",
    //"XRM",
    //OLEMSGICON.OLEMSGICON_INFO,
    //OLEMSGBUTTON.OLEMSGBUTTON_OK,
    //OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        public override bool UserConfirmation(string message)
        {
            // Configure the message box to be displayed 
            var messageBoxText = message;
            const string caption = "Confirm";
            const MessageBoxButton button = MessageBoxButton.YesNo;
            const MessageBoxImage icon = MessageBoxImage.Warning;
            var result = MessageBox.Show(messageBoxText, caption, button, icon);
            return result == MessageBoxResult.Yes;
        }

        public override void OpenRecord(string recordType, string fieldMatch, string fieldValue, Type maintainViewModelType)
        {
            throw new NotImplementedException();
        }

        public override string GetSaveFileName(string initialFileName, string extention)
        {
            throw new NotImplementedException();
        }

        public override string GetSaveFolderName()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<object> GetObjects(string regionName)
        {
            throw new NotImplementedException();
        }

        public override bool AllowSaveRequests {  get { return false; } }
    }
}
