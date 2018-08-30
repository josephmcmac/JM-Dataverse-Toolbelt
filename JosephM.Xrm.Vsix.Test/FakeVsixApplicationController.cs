using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Core.AppConfig;
using JosephM.Core.Test;
using JosephM.Xrm.Vsix.Application;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace JosephM.Xrm.Vsix.Test
{
    public class FakeVsixApplicationController : VsixApplicationController
    {
        public override bool RunThreadsAsynch => false;
        public FakeVsixApplicationController(IDependencyResolver dependencyResolver)
            : base(dependencyResolver, "ScriptedVsixApplication")
        {
             
        }

        public override void LoadDialog(DialogViewModel dialog, bool showCompletionScreen = true, bool isModal = false)
        {
            ClearTabs();
            _loadedObjects.Add(dialog);
        }

        public override void Remove(object item)
        {
            _loadedObjects.Remove(item);
        }

        public override IEnumerable<object> GetObjects()
        {
            return _loadedObjects.ToArray();
        }

        private readonly List<object> _loadedObjects = new List<object>();

        public override void UserMessage(string message)
        {
            Debug.Write(message);
        }

        public override bool UserConfirmation(string message)
        {
            return true;
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
             _loadedObjects.Clear();
        }

        public override Process StartProcess(string fileName, string arguments = null)
        {
            return null;
        }
    }
}
