#region

using JosephM.Application;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Navigation;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

#endregion

namespace JosephM.Prism.Infrastructure.Prism
{
    /// <summary>
    ///     Implementation Of IApplicationController For The Prism Application
    /// </summary>
    internal class PrismApplicationController : ApplicationControllerBase
    {
        public PrismApplicationController(IRegionManager regionManager, string applicationName, IDependencyResolver container)
            : base(applicationName, container)
        {
            RegionManager = regionManager;
        }

        private IRegionManager RegionManager { get; set; }

        public override void Remove(string regionName, object item)
        {
            if (RegionManager.Regions[regionName].Views.Contains(item))
                DoOnMainThread(() => RegionManager.Regions[regionName].Remove(item));
        }

        public override IEnumerable<object> GetObjects(string regionName)
        {
            return RegionManager.Regions[regionName].Views;
        }

        public override void RequestNavigate(string regionName, Type type, UriQuery uriQuery)
        {
            var uri = new Uri(type.FullName, UriKind.Relative);
            var navigationParameters = new NavigationParameters();
            if (uriQuery != null && uriQuery.Arguments != null)
            {
                foreach (var item in uriQuery.Arguments)
                {
                    navigationParameters.Add(item.Key, item.Value);
                }
            }
            RegionManager.RequestNavigate(regionName, uri, ProcessNavigationResult, navigationParameters);
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

        //public override void OpenRecord(string recordType, string fieldMatch, string fieldValue,
        //    Type maintainViewModelType)
        //{
        //    var uriQuery = new UriQuery();
        //    uriQuery.Add("RecordType", recordType);
        //    uriQuery.Add(NavigationParameters.RecordIdName, fieldMatch);
        //    uriQuery.Add(NavigationParameters.RecordId, fieldValue);
        //    RequestNavigate(RegionNames.MainTabRegion, maintainViewModelType, uriQuery);
        //}

        public override void NavigateTo(Type type, UriQuery uriQuery)
        {
            RequestNavigate(RegionNames.MainTabRegion, type, uriQuery);
        }

        public override string GetSaveFileName(string initialFileName, string extention)
        {
            var selectFolderDialog = new SaveFileDialog() { DefaultExt = extention, FileName = initialFileName, Filter = string.Format("{0} files |*{0}", extention) };
            var result = selectFolderDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                return selectFolderDialog.FileName;
            }
            return null;
        }

        public override string GetSaveFolderName()
        {
            var selectFolderDialog = new FolderBrowserDialog { ShowNewFolderButton = true };
            var dialogResult = selectFolderDialog.ShowDialog();
            return dialogResult == DialogResult.OK
                ? selectFolderDialog.SelectedPath
                : null;
        }

        public override void SeralializeObjectToFile(object theObject, string fileName)
        {
            try
            {
                base.SeralializeObjectToFile(theObject, fileName);
            }
            catch (Exception ex)
            {
                UserMessage(string.Format("Error Saving Object\n{0}", ex.DisplayString()));
            }
        }
    }
}