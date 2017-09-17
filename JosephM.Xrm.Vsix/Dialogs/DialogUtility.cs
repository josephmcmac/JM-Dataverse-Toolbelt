using System;
using System.Windows;
using System.Windows.Controls;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Wpf.Application;

namespace JosephM.XRM.VSIX.Dialogs
{
    public static class DialogUtility
    {
        public static IDialogController CreateDialogController()
        {
            return new DialogController(new VsixApplicationController(null));
        }

        public static void LoadDialog(DialogViewModel dialog, bool showCompletion = true, bool isModal = false)
        {
            var window = new Window
            {
                Title = "XRM Dialog"
            };
            var scroll = new ScrollViewer()
            {
                Content = "XRM Dialog"
            };
            window.Content = scroll;
            var dialogControl = new DialogForm();
            dialogControl.DataContext = dialog;
            scroll.Content = dialogControl;
            
            Action closeMethod = () => { dialog.DoOnMainThread(() =>
            {
                window.Close();
            }); };

            if (dialog.ApplicationController is VsixApplicationController)
            {
                var vsixController = (VsixApplicationController) dialog.ApplicationController;
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
    }
}
