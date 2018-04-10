#region

using JosephM.Application.Application;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Core.Test;
using Prism.Regions;
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
            : this(new FakesDependencyContainer())
        {
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
                var uri = new Uri(type.FullName, UriKind.Relative);
                var navigationParameters = new NavigationParameters();
                if (uriQuery.Arguments != null)
                {
                    foreach (var item in uriQuery.Arguments)
                    {
                        navigationParameters.Add(item.Key, item.Value);
                    }
                }
                var navigationContext = new NavigationContext(new FakeRegionNavigationService(), uri, navigationParameters);
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
            if (_regions.ContainsKey(RegionNames.MainTabRegion))
                _regions[RegionNames.MainTabRegion].Clear();
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
            RequestNavigate(RegionNames.MainTabRegion, type, uriQuery);
        }
    }
}