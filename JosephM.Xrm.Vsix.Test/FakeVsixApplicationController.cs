using JosephM.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Core.AppConfig;
using JosephM.Core.Test;
using JosephM.XRM.VSIX.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace JosephM.Xrm.Vsix.Test
{
    public class FakeVsixApplicationController : VsixApplicationController
    {
        public FakeVsixApplicationController(IDependencyResolver dependencyResolver) : base(dependencyResolver)
        {
             
        }

        public override void LoadDialog(DialogViewModel dialog, bool showCompletionScreen = true, bool isModal = false)
        {
            ClearTabs();
            _regions.Add(dialog);
        }

        public override void Remove(string regionName, object item)
        {
            _regions.Remove(item);
        }

        public override IEnumerable<object> GetObjects(string regionName)
        {
            return _regions;
        }

        private readonly List<object> _regions = new List<object>();

        public override void UserMessage(string message)
        {
        }

        public override bool UserConfirmation(string message)
        {
            return true;
        }

        public override void DoOnMainThread(Action action)
        {
            action();
        }

        public override void DoOnAsyncThread(Action action)
        {
            action();
        }

        public override void ThrowException(Exception ex)
        {
            throw new FakeUserMessageException(ex);
        }

        public override string GetSaveFileName(string initialFileName, string extention)
        {
            return Path.Combine(TestConstants.TestFolder, initialFileName == "*" ? "FakeFileName." + extention : initialFileName);
        }

        public override string GetSaveFolderName()
        {
            return TestConstants.TestFolder;
        }

        public override void ClearTabs()
        {
             _regions.Clear();
        }

        public override Process StartProcess(string fileName, string arguments = null)
        {
            return null;
        }
    }
}
