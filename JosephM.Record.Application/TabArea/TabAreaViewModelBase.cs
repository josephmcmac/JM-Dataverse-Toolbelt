#region

using System;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using JosephM.Core.Extentions;
using JosephM.Record.Application.Constants;
using JosephM.Record.Application.Controller;
using JosephM.Record.Application.Navigation;
using JosephM.Record.Application.Shared;

#endregion

namespace JosephM.Record.Application.TabArea
{
    /// <summary>
    ///     Base Class For A ViewModel Object Displayed In The Main Tab Area Region Of The Application
    /// </summary>
    public abstract class TabAreaViewModelBase : ViewModelBase, INavigationAware
    {
        protected TabAreaViewModelBase(IApplicationController controller)
            : base(controller)
        {
            LoadingViewModel = new LoadingViewModel(controller);
        }

        public virtual string TabLabel
        {
            get { return GetType().Name.SplitCamelCase(); }
        }

        public ICommand TabCloseCommand
        {
            get { return new DelegateCommand(OnTabClose); }
        }

        protected void OnTabClose()
        {
            if (ConfirmTabClose())
                ApplicationController.Remove(RegionNames.MainTabRegion, this);
        }

        public void DoWhileLoading(Action action)
        {
            DoWhileLoading(null, action);
        }

        public void DoWhileLoading(string message, Action action)
        {
            LoadingViewModel.DoWhileLoading(message, action);
        }

        public LoadingViewModel LoadingViewModel { get; private set; }

        protected virtual bool ConfirmTabClose()
        {
            return true;
        }

        #region INavigationAware Members

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        #endregion
    }
}