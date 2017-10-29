using JosephM.Application;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.AppConfig;
using JosephM.Core.Extentions;
using JosephM.Core.Serialisation;
using JosephM.ObjectMapping;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Wpf.Application;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;

namespace JosephM.XRM.VSIX.Dialogs
{
    public class VsixApplicationController : ApplicationControllerBase
    {
        public VsixApplicationController(IDependencyResolver dependencyResolver) : base("VSIX", dependencyResolver)
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
            uriQuery = uriQuery ?? new UriQuery();
            var navigationObject = ResolveType(type);

            if (navigationObject is DialogViewModel)
            {
                var dialog = navigationObject as DialogViewModel;
                foreach(var arg in uriQuery.Arguments)
                {
                    var dialogProperty = dialog.GetType().GetProperty(arg.Key);
                    if(dialogProperty != null)
                    {
                        if (dialogProperty.PropertyType == typeof(bool))
                            dialog.SetPropertyValue(dialogProperty.Name, bool.Parse(arg.Value));
                        else
                        {
                            var argObject = JsonHelper.JsonStringToObject(arg.Value, dialogProperty.PropertyType);
                            var propertyValue = dialog.GetPropertyValue(dialogProperty.Name) ?? argObject;
                            var mapper = new ClassSelfMapper();
                            mapper.Map(argObject, propertyValue);
                        }
                    }
                }

                LoadDialog(dialog);
            }
            else
                throw new NotImplementedException("Not implemented for type " + type.Name);
        }

        public virtual void LoadDialog(DialogViewModel dialog)
        {
            LoadDialogIntoWindow(dialog);
        }

        public static void LoadDialogIntoWindow(DialogViewModel dialog, bool showCompletion = true, bool isModal = false)
        {
            var window = new Window
            {
                Title = "XRM Dialog"
            };
            var content = new WindowShell();
            window.Content = content;
            var dialogControl = new DialogForm();
            dialogControl.DataContext = dialog;
            content.Content = dialogControl;

            Action closeMethod = () =>
            {
                dialog.DoOnMainThread(() =>
                {
                    window.Close();
                });
            };

            if (dialog.ApplicationController is VsixApplicationController)
            {
                var vsixController = (VsixApplicationController)dialog.ApplicationController;
                vsixController.SetRemoveMethod((item) =>
                {
                    if (item == dialog)
                        closeMethod();
                });
            }
            if (!showCompletion)
                dialog.OverideCompletionScreenMethod = closeMethod;

            if (isModal)
                window.ShowDialog();
            else
                window.Show();
        }

        public override void UserMessage(string message)
        {
            System.Windows.MessageBox.Show(message);
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

        public override void OpenRecord(string recordType, string fieldMatch, string fieldValue, Type maintainViewModelType)
        {
            throw new NotImplementedException();
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

        public override IEnumerable<object> GetObjects(string regionName)
        {
            throw new NotImplementedException();
        }

        public override bool AllowSaveRequests {  get { return false; } }

        public override bool ForceElementWindowHeight { get { return true; } }

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
