using JosephM.Application;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Core.Serialisation;
using JosephM.ObjectMapping;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.Wpf.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using JosephM.Application.ViewModel.TabArea;

namespace JosephM.Xrm.Vsix.Application
{
    public class VsixApplicationController : ApplicationControllerBase
    {
        public VsixApplicationController(IDependencyResolver dependencyResolver, string applicationName)
            : base(applicationName, dependencyResolver)
        {
        }

        private Action<object> _remove = null;

        public void SetRemoveMethod(Action<object> action)
        {
            _remove = action;
        }
        public override void Remove(object item)
        {
            if (_remove != null)
                _remove(item);
        }

        public override void NavigateTo(Type type, UriQuery uriQuery = null)
        {
            var navigationObject = ResolveType(type);
            NavigateTo(navigationObject, uriQuery, showCompletionScreen: true, isModal: false);
        }

        public override void NavigateTo(object item)
        {
            NavigateTo(item, null, showCompletionScreen: true, isModal: false);
        }

        public void NavigateTo(object navigationObject, UriQuery uriQuery, bool showCompletionScreen = true, bool isModal = false)
        {
            uriQuery = uriQuery ?? new UriQuery();

            foreach (var arg in uriQuery.Arguments)
            {
                var dialogProperty = navigationObject.GetType().GetProperty(arg.Key);
                if (dialogProperty != null)
                {
                    if (dialogProperty.PropertyType == typeof(bool))
                        navigationObject.SetPropertyValue(dialogProperty.Name, bool.Parse(arg.Value));
                    else
                    {
                        var argObject = JsonHelper.JsonStringToObject(arg.Value, dialogProperty.PropertyType);
                        var propertyValue = navigationObject.GetPropertyValue(dialogProperty.Name) ?? argObject;
                        var mapper = new ClassSelfMapper();
                        mapper.Map(argObject, propertyValue);
                    }
                }
            }
            if (navigationObject is TabAreaViewModelBase)
                LoadViewModel((TabAreaViewModelBase)navigationObject, showCompletionScreen: showCompletionScreen, isModal: isModal);
            else
                throw new NotImplementedException("Not implemented for type " + navigationObject?.GetType().Name);
        }

        public virtual void LoadViewModel(TabAreaViewModelBase viewModel, bool showCompletionScreen = true, bool isModal = false)
        {
            LoadDialogIntoWindow(viewModel, showCompletionScreen, isModal);
        }

        public static void LoadDialogIntoWindow(TabAreaViewModelBase viewModel, bool showCompletionScreen = true, bool isModal = false)
        {
            var window = new WindowShellWindow
            {
                Title = viewModel.TabLabel
            };
            window.DataContext = viewModel.ApplicationController;
            //var dialogControl = new DialogForm();
            //dialogControl.DataContext = dialog;
            window.SetContent(viewModel);

            Action closeMethod = () =>
            {
                viewModel.DoOnMainThread(() =>
                {
                    window.Close();
                });
            };

            if (viewModel.ApplicationController is VsixApplicationController)
            {
                var vsixController = (VsixApplicationController)viewModel.ApplicationController;
                vsixController.SetRemoveMethod((item) =>
                {
                    if (item == viewModel)
                        closeMethod();
                });
            }
            if (viewModel is DialogViewModel)
            {
                var dialog = (DialogViewModel)viewModel;
                if (!showCompletionScreen)
                    dialog.OverideCompletionScreenMethod = closeMethod;
            }

            if (isModal)
                window.ShowDialog();
            else
                window.Show();
        }

        public override void UserMessage(string message)
        {
            DoOnMainThread(
                () => System.Windows.MessageBox.Show(message));
        }

        public override bool UserConfirmation(string message)
        {
            // Configure the message box to be displayed 
            var messageBoxText = message;
            const string caption = "Confirm";
            const MessageBoxButton button = MessageBoxButton.YesNo;
            const MessageBoxImage icon = MessageBoxImage.Warning;
            var result = System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
            return result == MessageBoxResult.Yes;
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

        public override IEnumerable<object> GetObjects()
        {
            throw new NotImplementedException();
        }

        public override bool AllowSaveRequests {  get { return false; } }

        public override bool IsTabbedApplication { get { return false; } }

        public override object ResolveType(Type type)
        {
            if (type == typeof(ISavedXrmConnections))
            {
                var settingsManager = Container.ResolveType<ISettingsManager>();
                return settingsManager.Resolve<XrmPackageSettings>();
            }
            else
                return base.ResolveType(type);
        }
    }
}
