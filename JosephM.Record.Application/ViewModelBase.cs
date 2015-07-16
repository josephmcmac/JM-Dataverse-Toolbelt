#region

using System;
using System.ComponentModel;
using JosephM.Record.Application.Constants;
using JosephM.Record.Application.Controller;
using JosephM.Record.Application.Navigation;

#endregion

namespace JosephM.Record.Application
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

        public IApplicationController ApplicationController { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
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
            var prismQuery = new Microsoft.Practices.Prism.UriQuery();
            if (uriQuery != null)
            {
                foreach (var arg in uriQuery.Arguments)
                    prismQuery.Add(arg.Key, arg.Value);
            }
            var uri = new Uri(typeof(T).FullName + prismQuery, UriKind.Relative);
            ApplicationController.RequestNavigate(RegionNames.MainTabRegion, uri);
        }
    }
}