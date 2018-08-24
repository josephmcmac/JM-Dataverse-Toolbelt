#region

using JosephM.Application.Application;
using JosephM.Core.AppConfig;
using JosephM.Core.Test;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

#endregion

namespace JosephM.Application.ViewModel.Fakes
{
    /// <summary>
    ///     Object for access to the main UI thread and adding or removing UI items
    /// </summary>
    public class FakeApplicationController : ApplicationControllerBase
    {
        public override bool RunThreadsAsynch => false;
        public FakeApplicationController(IDependencyResolver dependencyResolver)
            : base("Test Script Application", dependencyResolver)
        {
            AddNotification("FAKE", "FAKE NOTIFICATION");
        }

        public FakeApplicationController()
            : this(new DependencyContainer())
        {
        }

        public override void Remove(object item)
        {
            if( _loadedObjects.Contains(item))
                _loadedObjects.Remove(item);
        }

        public override IEnumerable<object> GetObjects()
        {
            return _loadedObjects.ToArray();
        }

        private readonly List<object> _loadedObjects = new List<object>();


        private void RequestNavigate(Type type, UriQuery uriQuery)
        {
            ClearTabs();

            var resolvedType = Container.ResolveType(type);

            OnNavigatedTo(resolvedType);

            _loadedObjects.Add(resolvedType);
        }

        public override void UserMessage(string message)
        {
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

        public override void OpenFile(string fileName)
        {
            //nope
        }

        public override void NavigateTo(Type type, UriQuery uriQuery)
        {
            RequestNavigate(type, uriQuery);
        }
    }
}