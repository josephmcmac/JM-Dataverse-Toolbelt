#region

using JosephM.Application.Application;
using JosephM.Core.Extentions;
using JosephM.Core.Test;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
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
        public FakeApplicationController()
            : base("Test Script Application", new FakesDependencyContainer(new UnityContainer()))
        {
            AddNotification("FAKE", "FAKE NOTIFICATION");
        }

        public override void Remove(string regionName, object item)
        {
            if (_regions.ContainsKey(regionName) && _regions[regionName].Contains(item))
                _regions[regionName].Remove(item);
        }

        public override IEnumerable<object> GetObjects(string regionName)
        {
            return _regions.ContainsKey(regionName)
                ? _regions[regionName]
                : new List<object>();
        }

        private readonly IDictionary<string, List<object>> _regions = new Dictionary<string, List<object>>();


        public override void RequestNavigate(string regionName, Type type, UriQuery uriQuery)
        {
            ClearTabs();

            var resolvedType = Container.ResolveType(type);

            if (!_regions.ContainsKey(regionName))
                _regions.Add(regionName, new List<object>());
            _regions[regionName].Add(resolvedType);

            if (type.IsTypeOf(typeof(INavigationAware)))
            {
                var uri = JosephM.Application.ViewModel.Extentions.Extentions.ToPrismNavigationUriType(type, uriQuery);
                var navigationContext = new NavigationContext(new FakeRegionNavigationService(), uri);
                ((INavigationAware)resolvedType).OnNavigatedTo(navigationContext);
            }
        }

        public override void UserMessage(string message)
        {
        }


        public override bool UserConfirmation(string message)
        {
            return true;
        }

        public override void OpenRecord(string recordType, string fieldMatch, string fieldValue,
            Type maintainViewModelType)
        {
            throw new NotImplementedException();
        }

        public void OpenRecord(string entityType, string fieldMatch, string fieldValue)
        {
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
            throw new ApplicationException("Unexpected Error throw By Application", ex);
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
            if (_regions.ContainsKey(RegionNames.MainTabRegion))
                _regions[RegionNames.MainTabRegion].Clear();
        }

        public override Process StartProcess(string fileName, string arguments = null)
        {
            //var process = base.StartProcess(fileName, arguments);
            //process.Kill();
            return null;
        }

        public override void NavigateTo(Type type, UriQuery uriQuery)
        {
            RequestNavigate(RegionNames.MainTabRegion, type, uriQuery);
        }
    }
}