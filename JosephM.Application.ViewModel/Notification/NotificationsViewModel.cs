using JosephM.Application.Application;
using JosephM.Application.ViewModel.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.Application.ViewModel.Notification
{
    public class NotificationsViewModel : ViewModelBase
    {
        public ObservableCollection<NotificationViewModel> Notifications { get; }

        public NotificationsViewModel(IApplicationController controller)
            : base(controller)
        {
            Notifications = new ObservableCollection<NotificationViewModel>();
        }

        public void SetNotification(string id, string notification, bool isLoading = false, Dictionary<string, Action> actions = null)
        {
            var matchingIds = Notifications
                .Where(kv => kv.Key == id)
                .ToArray();
            ApplicationController.DoOnMainThread(() =>
            {
                var newNotification = new NotificationViewModel(id, notification, isLoading, actions, ApplicationController);
                foreach (var item in matchingIds)
                {
                    Notifications.Remove(item);
                    newNotification.OpenActions = item.OpenActions;
                }
                Notifications.Add(newNotification);
            });
        }
    }
}
