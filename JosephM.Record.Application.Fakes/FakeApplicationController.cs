using JosephM.Application.Application;
using JosephM.Core.AppConfig;
using JosephM.Core.Test;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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

        public override void NavigateTo(Type type, UriQuery uriQuery)
        {
            var resolvedType = Container.ResolveType(type);
            NavigateTo(resolvedType);
        }

        public override void NavigateTo(object item)
        {
            ClearTabs();
            OnNavigatedTo(item);
            _loadedObjects.Add(item);
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
    }
}