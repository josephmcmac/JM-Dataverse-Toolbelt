#region

using System;
using System.Windows;
using Microsoft.Practices.Prism.Regions;
using JosephM.Record.Application.Constants;
using JosephM.Record.Application.Controller;
using JosephM.Record.Application.Navigation;

#endregion

namespace JosephM.Prism.Infrastructure.Prism
{
    /// <summary>
    ///     Implementation Of IApplicationController For The Prism Application
    /// </summary>
    internal class ApplicationController : ApplicationControllerBase
    {
        public ApplicationController(IRegionManager regionManager, string applicationName)
            : base(applicationName)
        {
            RegionManager = regionManager;
        }

        private IRegionManager RegionManager { get; set; }

        public override void Remove(string regionName, object item)
        {
            if (RegionManager.Regions[regionName].Views.Contains(item))
                DoOnMainThread(() => RegionManager.Regions[regionName].Remove(item));
        }


        public override void AddToRegion(string regionName, object navigationErrorViewModel)
        {
            DoOnMainThread(() => RegionManager.AddToRegion(regionName, navigationErrorViewModel));
        }

        public override void RequestNavigate(string regionName, Uri uri)
        {
            RegionManager.RequestNavigate(regionName, uri, ProcessNavigationResult);
        }

        private void ProcessNavigationResult(NavigationResult navigationResult)
        {
            if (navigationResult.Result == false)
            {
                var navigationErrorViewModel = new NavigationErrorViewModel(navigationResult.Error, this);

                RegionManager.AddToRegion(RegionNames.MainTabRegion, navigationErrorViewModel);
            }
        }


        public override void UserMessage(string message)
        {
            DoOnMainThread(
                () => MessageBox.Show(message));
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

        public override void OpenRecord(string recordType, string fieldMatch, string fieldValue,
            Type maintainViewModelType)
        {
            var uriQuery = new Microsoft.Practices.Prism.UriQuery();
            uriQuery.Add(NavigationParameters.RecordType, recordType);
            uriQuery.Add(NavigationParameters.RecordIdName, fieldMatch);
            uriQuery.Add(NavigationParameters.RecordId, fieldValue);
            var uri = new Uri(maintainViewModelType.FullName + uriQuery, UriKind.Relative);
            RequestNavigate(RegionNames.MainTabRegion, uri);
        }

        public override void NavigateTo(Type type, UriQuery uriQuery)
        {
            var prismQuery = new Microsoft.Practices.Prism.UriQuery();
            if (uriQuery != null)
            {
                foreach (var arg in uriQuery.Arguments)
                {
                    prismQuery.Add(arg.Key, arg.Value);
                }
            }
            var uri = new Uri(type.FullName + prismQuery, UriKind.Relative);
            RequestNavigate(RegionNames.MainTabRegion, uri);
        }
    }
}