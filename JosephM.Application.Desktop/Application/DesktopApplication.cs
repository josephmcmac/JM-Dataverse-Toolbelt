using JosephM.Application.Application;
using JosephM.Application.Options;
using JosephM.Application.ViewModel.ApplicationOptions;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Notification;
using JosephM.Core.AppConfig;
using JosephM.Wpf.Application;
using System.Windows;

namespace JosephM.Application.Desktop.Application
{
    /// <summary>
    ///     Class For A Desktop Application Instance To Load Modules The Run
    /// </summary>
    public class DesktopApplication : ApplicationBase
    {
        public static DesktopApplication Create(string applicatonName)
        {
            var controller = new DesktopApplicationController(applicatonName, new DependencyContainer());
            controller.RegisterType<IDialogController, DialogController>();
            var options = new ApplicationOptionsViewModel(controller);
            var settingsManager = new DesktopSettingsManager(controller);
            var notifications = new NotificationsViewModel(controller);
            controller.RegisterInstance(notifications);
            return new DesktopApplication(controller, options, settingsManager, notifications);
        }

        private DesktopApplication(DesktopApplicationController applicationController, IApplicationOptions applicationOptions, ISettingsManager settingsManager, NotificationsViewModel notifications)
            : base(applicationController, applicationOptions, settingsManager)
        {
            ApplicationController = applicationController;
            ApplicationOptions = applicationOptions;
            Notifications = notifications;
        }

        public DesktopApplicationController ApplicationController { get; set; }

        public IApplicationOptions ApplicationOptions { get; set; }

        public NotificationsViewModel Notifications { get; set; }

        public string ApplicationName { get { return ApplicationController.ApplicationName; } }

        public void Run()
        {
            var shell = ApplicationController.ResolveType<Shell>();
            System.Windows.Application.Current.MainWindow = (Window)shell;
            System.Windows.Application.Current.MainWindow.DataContext = this;
            System.Windows.Application.Current.MainWindow.Show();
        }


    }
}