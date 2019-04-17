using System;
using System.ComponentModel;
using JosephM.Application.Application;

namespace JosephM.Application.ViewModel
{
    /// <summary>
    ///     Base Class For All View Models Active In The Application With Access To The Application Controller Object And
    ///     Ability To Notify The UI With Property Changed Events
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        public ViewModelBase(IApplicationController controller)
        {
            ApplicationController = controller;
        }

        public int StandardPageSize { get { return 50; } }


        public IApplicationController ApplicationController { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void DoOnMainThread(Action action)
        {
            ApplicationController.DoOnMainThread(action);
        }

        public void DoOnAsynchThread(Action action)
        {
            ApplicationController.DoOnAsyncThread(action);
        }

        public void NavigateTo<T>(UriQuery uriQuery)
        {
            ApplicationController.NavigateTo(typeof(T), uriQuery);
        }
    }
}