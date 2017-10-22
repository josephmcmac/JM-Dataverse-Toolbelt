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

        public static void LoadDialog(DialogViewModel dialog, bool showCompletion = true, bool isModal = false, Action postCompletion = null)
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
            
            Action closeMethod = () => { dialog.DoOnMainThread(() =>
            {
                window.Close();
                if (postCompletion != null)
                    postCompletion();
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
